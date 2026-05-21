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

    [Header("이펙트 프리팹")]
    public GameObject warningEffectPrefab; // 경고 단계 이펙트
    public GameObject activeEffectPrefab;  // 활성화 단계 이펙트

    private bool  _isActive       = false;
    private float _lastDamageTime = -999f;

    // 생성된 이펙트 인스턴스 (단계 전환 시 제거용)
    private GameObject _currentEffect;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        col.enabled   = false;

        StartCoroutine(ZoneRoutine());
    }

    IEnumerator ZoneRoutine()
    {
        // ── 1단계: 경고 이펙트 ──
        _isActive = false;
        SpawnEffect(warningEffectPrefab);

        yield return new WaitForSeconds(warningDuration);

        // ── 2단계: 활성화 이펙트 ──
        _isActive = true;
        GetComponent<Collider>().enabled = true;
        SpawnEffect(activeEffectPrefab); // 경고 이펙트 제거 후 활성 이펙트 생성

        yield return new WaitForSeconds(activeDuration);

        Destroy(gameObject);
    }

    void SpawnEffect(GameObject prefab)
    {
        // 이전 이펙트 제거
        if (_currentEffect != null)
            Destroy(_currentEffect);

        if (prefab == null) return;

        // 자신의 위치/회전에 맞춰 생성, 자식으로 붙임
        _currentEffect = Instantiate(prefab, transform.position, transform.rotation);
        _currentEffect.transform.SetParent(transform);
    }

    void OnTriggerStay(Collider other)
    {
        if (!_isActive) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - _lastDamageTime < damageCooldown) return;

        if (other.TryGetComponent<ITakeDamage>(out var target))
        {
            _lastDamageTime = Time.time;
            target.TakeDamage(damage);
        }
    }
}
