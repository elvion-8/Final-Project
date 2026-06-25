using UnityEngine;

// ============================================================
//  LaserHitDamage
//  AttackPattern3가 레이저 프리팹에 AddComponent로 붙여주는 컴포넌트
// ============================================================
public class LaserHitDamage : MonoBehaviour
{
    [HideInInspector]
    public int damage = 30;

    private float _damageCooldown = 0.5f;
    private float _lastHitTime    = -999f;

    private ParticleSystem _ps;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    // ────────────────────────────────────────────
    //  파티클 충돌
    //  (ParticleSystem의 Collision 모듈이 켜져 있어야 작동)
    // ────────────────────────────────────────────
    void OnParticleCollision(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < _lastHitTime + _damageCooldown) return;

        if (other.TryGetComponent<ITakeDamage>(out var target))
        {
            _lastHitTime = Time.time;
            target.TakeDamage(damage);
            Debug.Log($"[LaserHitDamage] {other.name}에게 {damage} 데미지");
        }
    }
}