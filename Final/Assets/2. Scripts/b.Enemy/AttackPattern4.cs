using System.Collections;
using UnityEngine;
using Rand = UnityEngine.Random;

// ============================================================
//  AttackPattern4 — 코어 개방 빔(Iris Overload) 패턴
//  [예고] 겉껍질 개방 이펙트 + 카메라 서서히 줌인
//  [발사] 완전 개방 시점에 코어에서 강력한 관통 빔 발사 + Shake·화면 플래시
//
//  ※ AttackPattern3(레이저)의 발사 로직을 기반으로 확장.
//    반복 발사가 아니라 "한 번의 강력한 빔"이라는 점이 차이.
//  ※ 카메라 연출은 필살기 성격이라 모든 클라이언트에서 각자 실행
// ============================================================
public class AttackPattern4 : MonoBehaviour, IAttackPattern
{
    [Header("코어 개방 이펙트")]
    [Tooltip("예고 단계에서 재생할 코어/이빨고리 개방 VFX 프리팹")]
    public GameObject coreOpenEffectPrefab;

    [Header("빔 설정")]
    [Tooltip("빔 이펙트 프리팹 (AttackPattern3의 laserPrefab과 동일 계열 사용 가능)")]
    public GameObject beamPrefab;

    [Tooltip("빔 최대 사거리 (m)")]
    public float beamRange = 25f;

    [Tooltip("예고 시간 (초) — 고리 개방 + 카메라 줌인 진행")]
    [Range(0.5f, 5f)]
    public float warningDuration = 1.5f;

    [Tooltip("빔 유지 시간 (초) — 데미지 활성화 구간")]
    [Range(0.5f, 5f)]
    public float beamSustainDuration = 1.2f;

    [Tooltip("투사체(빔) 속도 — 0이면 프리팹 기본값 유지")]
    [Range(0f, 200f)]
    public float beamSpeed = 0f;

    [Tooltip("빔 발사 위치 오프셋 (적 기준)")]
    public Vector3 firePointOffset = new Vector3(0f, 1.5f, 2f);

    [Header("데미지 설정")]
    [Tooltip("빔 1회 피격 데미지")]
    [Min(1)]
    public int damage = 60;

    [Header("카메라 연출")]
    [Tooltip("예고 중 카메라 줌인 정도 (FOV 감소량)")]
    [Range(0f, 20f)]
    public float warningZoomAmount = 10f;

    [Tooltip("발사 순간 카메라 흔들림 강도")]
    [Range(0f, 1f)]
    public float shakeIntensity = 0.35f;

    [Tooltip("발사 순간 카메라 흔들림 지속시간 (초)")]
    [Range(0.1f, 2f)]
    public float shakeDuration = 0.35f;

    [Tooltip("발사 순간 화면 플래시 색상")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.8f);

    [Tooltip("화면 플래시 지속시간 (초)")]
    [Range(0.05f, 1f)]
    public float flashDuration = 0.25f;

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;
    private GameObject _coreOpenObj;
    private GameObject _beamObj;

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
        yield return StartCoroutine(IrisOverloadRoutine());
    }

    // ────────────────────────────────────────────
    //  전체 루틴
    //  1) 예고: 코어 개방 VFX + 카메라 줌인 (warningDuration)
    //  2) 발사: 빔 생성 + 데미지 활성화 + Shake/플래시 (beamSustainDuration)
    // ────────────────────────────────────────────
    IEnumerator IrisOverloadRoutine()
    {
        if (_enemyTr == null) yield break;

        Vector3 firePos = _enemyTr.position + firePointOffset;

        // 사거리 체크 (AttackPattern3과 동일 방식)
        if (_traceTarget != null)
        {
            float dist = Vector3.Distance(firePos, _traceTarget.position);
            if (dist > beamRange)
            {
                Debug.Log($"[AttackPattern4] 사거리 초과 ({dist:F1}m), 패턴 스킵");
                yield break;
            }
        }

        Vector3 fireDir = GetFireDirection(firePos);

        // ── 1단계: 예고 (코어 개방 + 줌인) ──────────────
        if (coreOpenEffectPrefab != null)
        {
            _coreOpenObj = Instantiate(coreOpenEffectPrefab, firePos, Quaternion.LookRotation(fireDir));
            _coreOpenObj.transform.SetParent(transform);
        }

        StartCoroutine(CameraZoomRoutine(warningZoomAmount, warningDuration));

        yield return new WaitForSeconds(warningDuration);

        if (_coreOpenObj != null) Destroy(_coreOpenObj);

        // ── 2단계: 발사 (데미지 + Shake + 플래시) ────────
        FireBeam(firePos, fireDir);

        StartCoroutine(CameraShakeRoutine(shakeIntensity, shakeDuration));
        StartCoroutine(ScreenFlashRoutine(flashColor, flashDuration));

        yield return new WaitForSeconds(beamSustainDuration);

        if (_beamObj != null) Destroy(_beamObj);
    }

    // ────────────────────────────────────────────
    //  빔 발사 (데미지 컴포넌트 부착)
    // ────────────────────────────────────────────
    void FireBeam(Vector3 firePos, Vector3 fireDir)
    {
        if (beamPrefab == null)
        {
            Debug.LogWarning("[AttackPattern4] beamPrefab이 연결되지 않았습니다.");
            return;
        }

        _beamObj = Instantiate(beamPrefab, firePos, Quaternion.LookRotation(fireDir));
        _beamObj.transform.SetParent(transform);

        if (beamSpeed > 0f)
            ApplySpeed(_beamObj, beamSpeed);

        // AttackPattern3과 동일한 데미지 컴포넌트 재사용
        LaserHitDamage hitDmg = _beamObj.AddComponent<LaserHitDamage>();
        hitDmg.damage = damage;
    }

    void ApplySpeed(GameObject obj, float speed)
    {
        foreach (ParticleSystem ps in obj.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startSpeed = speed;
        }
    }

    // ────────────────────────────────────────────
    //  발사 방향 계산 (예고 종료 시점 위치로 고정)
    // ────────────────────────────────────────────
    Vector3 GetFireDirection(Vector3 firePos)
    {
        if (_traceTarget != null)
        {
            Vector3 targetPos = _traceTarget.position + Vector3.up * 1f;
            return (targetPos - firePos).normalized;
        }
        return _enemyTr.forward;
    }

    // ────────────────────────────────────────────
    //  카메라 연출 — 줌인 (예고 동안 유지, 발사 시점에 원상복구는
    //  Shake 루틴이 끝난 뒤 별도 복귀시켜도 되지만 여기선 예고 종료와
    //  동시에 원래 FOV로 되돌림)
    // ────────────────────────────────────────────
    IEnumerator CameraZoomRoutine(float zoomAmount, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        float originalFov = cam.fieldOfView;
        float targetFov   = originalFov - zoomAmount;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(originalFov, targetFov, t / duration);
            yield return null;
        }

        cam.fieldOfView = targetFov;

        // 발사 직후 서서히 원래 FOV로 복귀
        t = 0f;
        float returnDuration = duration * 0.5f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(targetFov, originalFov, t / returnDuration);
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

    // ────────────────────────────────────────────
    //  카메라 연출 — 화면 플래시
    //  별도 UI 시스템이 없다는 가정하에, 런타임에 풀스크린 Canvas를
    //  직접 생성해서 재생 후 파괴함. 기존 UI 매니저가 있다면
    //  이 부분만 그쪽 호출로 교체하면 됨.
    // ────────────────────────────────────────────
    IEnumerator ScreenFlashRoutine(Color color, float duration)
    {
        GameObject canvasObj = new GameObject("AttackPattern4_FlashCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imgObj = new GameObject("Flash");
        imgObj.transform.SetParent(canvasObj.transform, false);

        UnityEngine.UI.Image img = imgObj.AddComponent<UnityEngine.UI.Image>();
        img.color = color;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        float elapsed = 0f;
        Color startColor = color;
        Color endColor   = new Color(color.r, color.g, color.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        Destroy(canvasObj);
    }

    void OnDestroy()
    {
        if (_coreOpenObj != null) Destroy(_coreOpenObj);
        if (_beamObj != null) Destroy(_beamObj);
    }
}