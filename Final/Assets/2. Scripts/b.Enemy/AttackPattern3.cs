using System.Collections;
using UnityEngine;

// ============================================================
//  AttackPattern3 — 레이저 패턴
//  [예고] 파티클 생성 후 위치 고정 (데미지 없음)
//  [발사] 방향 설정 + 데미지 활성화
// ============================================================
public class AttackPattern3 : MonoBehaviour, IAttackPattern
{
    [Header("레이저 설정")]
    [Tooltip("레이저 이펙트 프리팹 (Projectile 18 nova orange)")]
    public GameObject laserPrefab;

    [Tooltip("레이저 최대 사거리 (m)")]
    public float laserRange = 20f;

    [Tooltip("예고 시간 (초) — 파티클 생성 후 위치 고정, 데미지 없음")]
    [Range(0.1f, 5f)]
    public float warningDuration = 1f;

    [Tooltip("발사 지속시간 (초) — 데미지 활성화")]
    [Range(1f, 30f)]
    public float fireDuration = 3f;

    [Tooltip("발사 간격 (초) — 레이저 재발사 간격")]
    [Range(0.1f, 5f)]
    public float fireInterval = 0.5f;

    [Tooltip("투사체 속도 — 파티클 Start Speed를 덮어씀")]
    [Range(0f, 200f)]
    public float projectileSpeed = 0f;

    [Tooltip("레이저 발사 위치 오프셋 (적 기준)")]
    public Vector3 firePointOffset = new Vector3(0f, 1.5f, 2f);

    [Header("데미지 설정")]
    [Tooltip("레이저 1회 피격 데미지")]
    [Min(1)]
    public int damage = 30;

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;

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
        yield return StartCoroutine(LaserRoutine());
    }

    // ────────────────────────────────────────────
    //  전체 루틴
    //  1) 예고: 파티클 소환 + 위치 고정 (warningDuration)
    //  2) 발사: 데미지 활성화 + fireDuration 동안 반복
    // ────────────────────────────────────────────
    IEnumerator LaserRoutine()
    {
        if (laserPrefab == null)
        {
            Debug.LogWarning("[AttackPattern3] laserPrefab이 연결되지 않았습니다.");
            yield break;
        }

        if (_enemyTr == null) yield break;

        // 발사 위치 계산
        Vector3 firePos = _enemyTr.position + firePointOffset;

        // 사거리 체크
        if (_traceTarget != null)
        {
            float dist = Vector3.Distance(firePos, _traceTarget.position);
            if (dist > laserRange)
            {
                Debug.Log($"[AttackPattern3] 사거리 초과 ({dist:F1}m), 패턴 스킵");
                yield break;
            }
        }

        // 발사 방향 결정 (예고 시점의 플레이어 위치로 고정)
        Vector3 fireDir = GetFireDirection(firePos);

        // ── 1단계: 예고 ──────────────────────────────
        GameObject warningObj = Instantiate(laserPrefab, firePos, Quaternion.LookRotation(fireDir));
        warningObj.transform.SetParent(transform);

        // 파티클 일시정지 → 위치에서 대기만 함 (데미지 없음)
        ParticleSystem[] warnParticles = warningObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in warnParticles)
            ps.Pause();

        yield return new WaitForSeconds(warningDuration);

        // ── 2단계: 발사 ──────────────────────────────
        Destroy(warningObj);

        float elapsed = 0f;
        while (elapsed < fireDuration)
        {
            FireLaser(firePos, fireDir);
            yield return new WaitForSeconds(fireInterval);
            elapsed += fireInterval;
        }
    }

    // ────────────────────────────────────────────
    //  레이저 발사 (데미지 활성화)
    // ────────────────────────────────────────────
    void FireLaser(Vector3 firePos, Vector3 fireDir)
    {
        GameObject laser = Instantiate(laserPrefab, firePos, Quaternion.LookRotation(fireDir));
        laser.transform.SetParent(transform);

        // 투사체 속도 적용 (0이면 프리팹 기본값 유지)
        if (projectileSpeed > 0f)
            ApplySpeed(laser, projectileSpeed);

        // 데미지 컴포넌트 주입
        LaserHitDamage hitDmg = laser.AddComponent<LaserHitDamage>();
        hitDmg.damage = damage;
    }

    // ────────────────────────────────────────────
    //  파티클 Start Speed 덮어쓰기
    //  루트 + 모든 자식 ParticleSystem에 적용
    // ────────────────────────────────────────────
    void ApplySpeed(GameObject obj, float speed)
    {
        foreach (ParticleSystem ps in obj.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startSpeed = speed;
        }
    }

    // ────────────────────────────────────────────
    //  발사 방향 계산
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

    void OnDestroy()
    {
        foreach (Transform child in transform)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }
}