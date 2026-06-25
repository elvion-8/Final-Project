using UnityEngine;

// ============================================================
//  AoEZoneDamage
//  AttackPattern1이 런타임에 장판 오브젝트에 AddComponent로 붙임
// ============================================================
public class AoEZoneDamage : MonoBehaviour
{
    [HideInInspector] public int damage = 20;

    private bool _isActive       = false;
    private bool _hasDealtDamage = false;

    public void SetActive(bool active)
    {
        _isActive = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_isActive)      return;
        if (_hasDealtDamage) return;
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<ITakeDamage>(out var target))
        {
            _hasDealtDamage = true;
            target.TakeDamage(damage);
        }
    }
}