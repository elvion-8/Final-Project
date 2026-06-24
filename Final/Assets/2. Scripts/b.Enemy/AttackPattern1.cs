using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

// ============================================================
//  AttackPattern1 — 장판 패턴
//  별도 프리팹으로 관리. EnemyCtrl이 Instantiate해서 실행.
// ============================================================
public class AttackPattern1 : MonoBehaviour, IAttackPattern
{
    [Header("장판 설정")]
    public GameObject attackZonePrefab;
    [Range(10f, 120f)] public float totalDuration = 60f;
    [Range(1,   20)]   public int   spawnCount    = 10;
    [Range(1,   20)]   public float spawnRadius   = 8f;
    [Range(1,   10)]   public int   groupMin      = 4;
    [Range(1,   10)]   public int   groupMax      = 5;
    public LayerMask groundLayer;

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;

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
    //  장판 소환
    // ────────────────────────────────────────────
    void SpawnZone()
    {
        if (attackZonePrefab == null)
        {
            Debug.LogWarning("[AttackPattern1] AttackZone 프리팹이 없습니다.");
            return;
        }

        Vector3 center    = _traceTarget != null ? _traceTarget.position : _enemyTr.position;
        Vector2 randCircle = Rand.insideUnitCircle.normalized
                           * Rand.Range(1.5f, spawnRadius);

        Vector3 rayOrigin = new Vector3(
            center.x + randCircle.x,
            center.y + 50f,
            center.z + randCircle.y);

        Vector3 spawnPos;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            spawnPos = hit.point;
        else
        {
            spawnPos = new Vector3(rayOrigin.x, center.y, rayOrigin.z);
            Debug.LogWarning("[AttackPattern1] 바닥을 찾지 못했습니다. GroundLayer를 확인하세요.");
        }

        GameObject zone = Instantiate(attackZonePrefab, spawnPos, Quaternion.identity);
        _activeZones.Add(zone);
    }

    void Cleanup()
    {
        foreach (GameObject z in _activeZones)
            if (z != null) Destroy(z);
        _activeZones.Clear();
    }

    void OnDestroy() => Cleanup();
}