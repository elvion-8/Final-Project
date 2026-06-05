using System.Collections;
using UnityEngine;

public class AttackZone1 : MonoBehaviour
{
    [Header("피격 설정")]
    public int   damage         = 20;
    public float damageCooldown = 0.5f;

    [Header("타이밍")]
    public float warningDuration = 2.0f;
    public float activeDuration  = 2.0f;

    [Header("스폰 오프셋")]
    public float spawnRadius    = 3.0f;   // 플레이어 기준 랜덤 반경
    public float minDistance    = 1.5f;   // 플레이어와 최소 거리

    [Header("이펙트 프리팹")]
    public GameObject warningEffectPrefab; // 경고 단계 이펙트
    public GameObject activeEffectPrefab;  // 활성화 단계 이펙트

    private bool  _isActive       = false;
    private bool  _hasDealtDamage = false;
    private GameObject  _currentEffect;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        col.enabled   = false;

        RepositionAroundPlayer();

        StartCoroutine(ZoneRoutine());
    }
    // ────────────────────────────────────────────
    //  플레이어 주변 랜덤 위치 계산
    // ────────────────────────────────────────────
    void RepositionAroundPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 offset;
        int     safetyLimit = 20;

        do
        {
            // XZ 평면에서 랜덤 방향 + 반경
            Vector2 rand2D = Random.insideUnitCircle.normalized
                             * Random.Range(minDistance, spawnRadius);

            offset = new Vector3(rand2D.x, 0f, rand2D.y);
            safetyLimit--;
        }
        while (offset.magnitude < minDistance && safetyLimit > 0);

        transform.position = player.transform.position + offset;
    }

    // ────────────────────────────────────────────
    //  장판예고 > 활성화 > 제거
    // ────────────────────────────────────────────
    IEnumerator ZoneRoutine()
    {
        _isActive = false;
        SpawnEffect(warningEffectPrefab);

        yield return new WaitForSeconds(warningDuration);

        _isActive = true;
        GetComponent<Collider>().enabled = true;
        SpawnEffect(activeEffectPrefab);

        yield return new WaitForSeconds(activeDuration);

        Destroy(gameObject);
    }

    void SpawnEffect(GameObject prefab)
    {
        if (_currentEffect != null)
            Destroy(_currentEffect);

        if (prefab == null) return;

        _currentEffect = Instantiate(prefab, transform.position, transform.rotation);
        _currentEffect.transform.SetParent(transform);
    }

    // ────────────────────────────────────────────
    //  피격 판정
    // ────────────────────────────────────────────
    void OnTriggerEnter(Collider other)          
    {
        if (!_isActive)        return;
        if (_hasDealtDamage)   return;           // 이미 피격했으면 무시
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<ITakeDamage>(out var target))
        {
            _hasDealtDamage = true;        
            target.TakeDamage(damage);
        }
    }
}
