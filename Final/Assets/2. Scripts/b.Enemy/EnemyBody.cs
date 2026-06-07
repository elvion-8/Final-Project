// EnemyBody.cs
using UnityEngine;

public class EnemyBody : MonoBehaviour
{
    private EnemyCtrl _enemyCtrl;

    void Awake()
    {
        _enemyCtrl = GetComponentInParent<EnemyCtrl>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 무기 콜라이더가 닿았을 때
        if (other.CompareTag("PlayerWeapon"))
        {
            // 무기 스크립트에서 데미지 값 가져오기
            //WeaponCtrl weapon = other.GetComponent<WeaponCtrl>();
            //int damage = weapon != null ? weapon.damage : 10; // 기본값 10
            //_enemyCtrl?.TakeDamage(damage);
        }
    }
}