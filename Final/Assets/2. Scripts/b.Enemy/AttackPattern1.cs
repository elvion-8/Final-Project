using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

// ============================================================
//  AttackPattern1 — 장판 패턴 (AttackZone1 통합)
//  EnemyCtrl이 Instantiate해서 Execute() 호출
// ============================================================
public class AttackPattern1 : MonoBehaviour, IAttackPattern
{
    [Header("장판 생성 설정")]
    [Range(10f, 120f)] public float totalDuration = 60f;
    [Range(1,   20)]   public int   spawnCount    = 10;
    [Range(1f,  20f)]  public float spawnRadius   = 8f;
    [Range(1,   10)]   public int   groupMin      = 4;
    [Range(1,   10)]   public int   groupMax      = 5;
    public LayerMask groundLayer;

    [Header("장판 타이밍")]
    public float warningDuration = 2.0f;
    public float activeDuration  = 2.0f;

    [Header("장판 데미지")]
    [Min(1)] public int damage = 20;

    [Header("장판 이펙트 프리팹")]
    public GameObject warningEffectPrefab;
    public GameObject activeEffectPrefab;

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;

    // 활성화된 장판 추적 (Cleanup용)
    private readonly List<GameObject> _activeZones = new List<GameObject>();

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
        // 랜덤 시간에 소환될 spawnCount개의 타임스탬프 생성
        float[] spawnTimes = new float[spawnCount];
        for (int i = 0; i < spawnCount; i++)
            spawnTimes[i] = Rand.Range(0f, totalDuration * 0.9f);
        System.Array.Sort(spawnTimes);

        float elapsed   = 0f;
        int   nextSpawn = 0;

        while (elapsed < totalDuration)
        {
            while (nextSpawn < spawnCount && elapsed >= spawnTimes[nextSpawn])
            {
                int groupCount = Rand.Range(groupMin, groupMax + 1);
                for (int i = 0; i < groupCount; i++)
                    SpawnZone();
                nextSpawn++;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        Cleanup();
    }

    // ────────────────────────────────────────────
    //  장판 소환 + 루틴 시작
    // ────────────────────────────────────────────
    void SpawnZone()
    {
        // 위치 계산
        Vector3 center = _traceTarget != null ? _traceTarget.position : _enemyTr.position;
        Vector2 rand2D = Rand.insideUnitCircle.normalized * Rand.Range(1.5f, spawnRadius);

        Vector3 rayOrigin = new Vector3(
            center.x + rand2D.x,
            center.y + 50f,
            center.z + rand2D.y);

        Vector3 spawnPos;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            spawnPos = hit.point;
        else
        {
            spawnPos = new Vector3(rayOrigin.x, center.y, rayOrigin.z);
            Debug.LogWarning("[AttackPattern1] 바닥을 찾지 못했습니다. GroundLayer를 확인하세요.");
        }

        // 장판 오브젝트 생성
        GameObject zone = new GameObject("AoEZone");
        zone.transform.position = spawnPos;
        zone.transform.SetParent(transform);

        // Collider 추가
        CapsuleCollider col = zone.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.enabled   = false;

        if (activeEffectPrefab != null)
        {
            CapsuleCollider effectCol = activeEffectPrefab.GetComponent<CapsuleCollider>();
            if (effectCol != null)
            {
                // 프리팹의 반경, 높이, 방향, 중심축 보정을 모두 복사 (스케일도 고려)
                col.radius = effectCol.radius * activeEffectPrefab.transform.lossyScale.x;
                col.height = effectCol.height * activeEffectPrefab.transform.lossyScale.y;
                col.direction = effectCol.direction;
                
                // 중심점(Center) 복사 시 프리팹 스케일 반영
                Vector3 scaledCenter = effectCol.center;
                scaledCenter.x *= activeEffectPrefab.transform.lossyScale.x;
                scaledCenter.y *= activeEffectPrefab.transform.lossyScale.y;
                scaledCenter.z *= activeEffectPrefab.transform.lossyScale.z;
                col.center = scaledCenter;
            }

            else
            {
                Debug.LogWarning("[AttackPattern1] 이펙트 프리팹에 CapsuleCollider가 없습니다.");
                col.radius = 3.0f;
                col.height = 10.0f;
            }
        }

        Rigidbody rb = zone.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // 데미지 처리 컴포넌트 추가
        AoEZoneDamage dmg = zone.AddComponent<AoEZoneDamage>();
        dmg.damage = damage;

        _activeZones.Add(zone);

        // 경고 → 활성화 → 제거 루틴
        StartCoroutine(ZoneRoutine(zone, col, dmg));
    }

    // ────────────────────────────────────────────
    //  장판 루틴 (경고 → 활성화 → 제거)
    // ────────────────────────────────────────────

    IEnumerator ZoneRoutine(GameObject zone, CapsuleCollider col, AoEZoneDamage dmg)
    {
        // ── 경고 단계 ──
        GameObject warningFx = SpawnEffect(warningEffectPrefab, zone.transform);
        yield return new WaitForSeconds(warningDuration);

        if (zone == null) yield break;

        // ── 활성화 단계 ──
        if (warningFx != null) Destroy(warningFx);
        col.enabled = true;
        dmg.SetActive(true);
        GameObject activeFx = SpawnEffect(activeEffectPrefab, zone.transform);

        yield return new WaitForSeconds(activeDuration);

        // ── 제거 ──
        _activeZones.Remove(zone);
        Destroy(zone);
    }

    // ────────────────────────────────────────────
    //  이펙트 소환
    // ────────────────────────────────────────────
    GameObject SpawnEffect(GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;

        GameObject fx = Instantiate(prefab, parent.position, prefab.transform.rotation);
        fx.transform.SetParent(parent);
        return fx;
    }

    // ────────────────────────────────────────────
    //  정리
    // ────────────────────────────────────────────
    void Cleanup()
    {
        foreach (GameObject z in _activeZones)
            if (z != null) Destroy(z);
        _activeZones.Clear();
    }

    void OnDestroy() => Cleanup();
}
