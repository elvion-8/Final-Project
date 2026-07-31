using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

// ============================================================
//  AttackPattern5 — Gravity Pull 패턴
//  1. 코어 이펙트 성장 + 범위 내 플레이어를 중심으로 서서히 끌어당김
//  2. 중력 종료 시점에 전방위 데미지 폭발
//  3. 중력작용 중 로컬 플레이어 줌인, 폭발 시 Shake
//
//  ※ 멀티플레이어 동기화 가정 (PUN1 클래식 문법 기준):
//    - 이동 권한은 각 플레이어 자신의 클라이언트(Owner)에게 있다고 가정
//    - 따라서 "끌어당기기"는 대상 플레이어의 PhotonView를 통해
//      RPC로 전달하고, 실제 이동은 대상 플레이어 스크립트에서 처리
//    - 카메라 연출(줌인/Shake)은 로컬 플레이어가 영향을 받을 때만 실행
// ============================================================
public class AttackPattern5 : MonoBehaviour, IAttackPattern
{
    [Header("코어 이펙트")]
    [Tooltip("인력 코어 VFX 프리팹 (예고 단계에서 점점 성장)")]
    public GameObject coreEffectPrefab;

    [Tooltip("폭발 이펙트 프리팹")]
    public GameObject explosionEffectPrefab;

    [Header("인력(Pull) 설정")]
    [Tooltip("인력이 적용되는 범위 (m)")]
    [Range(1f, 30f)]
    public float pullRange = 12f;

    [Tooltip("끌어당기는 시간 (초) — 이 시간이 지나면 폭발")]
    [Range(0.5f, 5f)]
    public float pullDuration = 2f;

    [Tooltip("끌어당기는 속도 (m/s)")]
    [Range(0.5f, 20f)]
    public float pullSpeed = 4f;

    [Header("폭발 설정")]
    [Tooltip("폭발 반경 (m)")]
    [Range(1f, 15f)]
    public float explosionRadius = 6f;

    [Tooltip("폭발 데미지")]
    [Min(1)]
    public int explosionDamage = 50;

    [Header("카메라 연출")]
    [Tooltip("인력 적용 중 카메라 줌인 정도 (FOV 감소량)")]
    [Range(0f, 20f)]
    public float pullZoomAmount = 8f;

    [Tooltip("폭발 시 카메라 흔들림 강도")]
    [Range(0f, 1f)]
    public float shakeIntensity = 0.3f;

    [Tooltip("폭발 시 카메라 흔들림 지속시간 (초)")]
    [Range(0.1f, 2f)]
    public float shakeDuration = 0.4f;

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;
    private GameObject _coreObj;

    // ────────────────────────────────────────────
    //  IAttackPattern 구현
    // ────────────────────────────────────────────
    public void SetContext(Transform enemyTr, Transform traceTarget)
    {
        _enemyTr     = enemyTr;
        _traceTarget = traceTarget;
    }

    public IEnumerator Execute()
    {
        yield return StartCoroutine(GravityPullRoutine());
    }

    // ────────────────────────────────────────────
    //  전체 루틴
    //  1) 코어 성장 (예고, 데미지 없음)
    //  2) 인력 적용 (pullDuration 동안 대상 끌어당김)
    //  3) 폭발 (전방위 데미지 + 카메라 Shake)
    // ────────────────────────────────────────────
    IEnumerator GravityPullRoutine()
    {
        if (_enemyTr == null) yield break;

        Vector3 corePos = _enemyTr.position;

        // ── 1단계: 코어 예고 ──────────────────────────
        if (coreEffectPrefab != null)
        {
            _coreObj = Instantiate(coreEffectPrefab, corePos, Quaternion.identity);
            _coreObj.transform.SetParent(transform);
        }

        // ── 2단계: 인력 적용 ──────────────────────────
        List<Transform> affected = FindPlayersInRange(corePos, pullRange);
        bool localAffected = IsLocalPlayerAffected(affected);

        if (localAffected)
            StartCoroutine(CameraZoomRoutine(pullZoomAmount, pullDuration));

        foreach (Transform p in affected)
            RequestPull(p, corePos);

        yield return new WaitForSeconds(pullDuration);

        // ── 3단계: 폭발 ────────────────────────────────
        if (_coreObj != null) Destroy(_coreObj);

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, corePos, Quaternion.identity);
            Destroy(fx, 3f);
        }

        ApplyExplosionDamage(corePos, explosionRadius, explosionDamage);

        if (localAffected)
            StartCoroutine(CameraShakeRoutine(shakeIntensity, shakeDuration));
    }

    // ────────────────────────────────────────────
    //  범위 내 플레이어 탐색
    // ────────────────────────────────────────────
    List<Transform> FindPlayersInRange(Vector3 center, float range)
    {
        List<Transform> result = new List<Transform>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (Vector3.Distance(p.transform.position, center) <= range)
                result.Add(p.transform);
        }
        return result;
    }

    // ────────────────────────────────────────────
    //  로컬 플레이어가 영향을 받는지 확인
    //  (카메라 연출은 자기 클라이언트에서만 실행해야 하므로 필요)
    // ────────────────────────────────────────────
    bool IsLocalPlayerAffected(List<Transform> affected)
    {
        foreach (Transform t in affected)
        {
            PhotonView pv = t.GetComponent<PhotonView>();
            if (pv != null && pv.isMine) return true;
        }
        return false;
    }

    // ────────────────────────────────────────────
    //  인력 요청 전송
    //  실제 이동은 대상 플레이어의 소유 클라이언트에서 처리
    //  (플레이어 컨트롤러 스크립트에 RPC_ApplyGravityPull 필요 — 하단 참고)
    // ────────────────────────────────────────────
    void RequestPull(Transform player, Vector3 corePos)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogWarning($"[AttackPattern5] {player.name}: PhotonView가 없습니다. 인력 스킵.");
            return;
        }

        pv.RPC("RPC_ApplyGravityPull", pv.owner, corePos, pullSpeed, pullDuration);
    }

    // ────────────────────────────────────────────
    //  폭발 데미지 적용 (ITakeDamage 인터페이스 활용)
    // ────────────────────────────────────────────
    void ApplyExplosionDamage(Vector3 center, float radius, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        HashSet<ITakeDamage> damagedTargets = new HashSet<ITakeDamage>();
        foreach (Collider col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            if (col.TryGetComponent<ITakeDamage>(out var target))
            {
                if (damagedTargets.Add(target))
                {
                    target.TakeDamage(damage);
                    Debug.Log($"[AttackPattern5] {col.name}에게 폭발 데미지 {damage} 적용");
                }
            }
        }
    }

    // ────────────────────────────────────────────
    //  카메라 연출 — 줌인 (FOV 감소 후 복귀)
    // ────────────────────────────────────────────
    IEnumerator CameraZoomRoutine(float zoomAmount, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        float originalFov = cam.fieldOfView;
        float targetFov   = originalFov - zoomAmount;
        float half = duration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(originalFov, targetFov, t / half);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(targetFov, originalFov, t / half);
            yield return null;
        }

        cam.fieldOfView = originalFov;
    }

    // ────────────────────────────────────────────
    //  카메라 연출 — 흔들림 (Shake)
    // ────────────────────────────────────────────
    IEnumerator CameraShakeRoutine(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Transform camTr = cam.transform;
        Vector3 originalPos = camTr.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            camTr.localPosition = originalPos + Rand.insideUnitSphere * intensity * damper;
            yield return null;
        }

        camTr.localPosition = originalPos;
    }

    void OnDestroy()
    {
        if (_coreObj != null) Destroy(_coreObj);
    }
}

// ============================================================
//  플레이어 컨트롤러 스크립트에 추가해야 하는 RPC 수신 메서드
//  (플레이어 스크립트에 붙여야되서 두현님 확인 후 진행 예정)
// ============================================================
//
// [PunRPC]
// void RPC_ApplyGravityPull(Vector3 corePos, float pullSpeed, float pullDuration)
// {
//     // 자기 자신 소유가 아니면 무시 (이동 권한 보호)
//     if (photonView != null && !photonView.isMine) return;
//
//     StartCoroutine(GravityPullMove(corePos, pullSpeed, pullDuration));
// }
//
// IEnumerator GravityPullMove(Vector3 corePos, float pullSpeed, float pullDuration)
// {
//     float elapsed = 0f;
//     while (elapsed < pullDuration)
//     {
//         elapsed += Time.deltaTime;
//         Vector3 dir = (corePos - transform.position);
//         dir.y = 0f; // 필요시 높이는 고정
//         if (dir.magnitude > 0.1f)
//             transform.position += dir.normalized * pullSpeed * Time.deltaTime;
//         yield return null;
//     }
// }