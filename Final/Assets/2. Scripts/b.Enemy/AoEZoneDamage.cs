using UnityEngine;

public class AoEZoneDamage : MonoBehaviour
{
    [HideInInspector] 
    public int damage;

    private bool isActive = false;
    private bool hasDamaged = false; // 장판에 한 번만 데미지를 입게 하기 위한 변수

    // AttackPattern1에서 장판이 활성화될 때 호출
    public void SetActive(bool state)
    {
        isActive = state;
    }

    // 콜라이더 안에 플레이어가 들어오거나 머물고 있을 때 실행
    private void OnTriggerStay(Collider other)
    {
        // 장판이 아직 활성화되지 않았거나, 이미 데미지를 줬다면 무시
        if (!isActive || hasDamaged) return;

        // 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<ITakeDamage>(out var target))
            {
                target.TakeDamage(damage);
                Debug.Log($"[AoEZoneDamage] {other.name}에게 {damage} 장판 데미지 적용");
                hasDamaged = true; 
            }
        }
    }
}