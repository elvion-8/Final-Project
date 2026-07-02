using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("검기 설정")]
    public float speed = 20f;          // 날아가는 속도
    public float lifetime = 3f;       // 생존 시간 (3초 뒤 자동 삭제)
    public int damage = 10;           // 검기 데미지

    private void Start()
    {
        // 생성되자마자 lifetime 후에 스스로 파괴됨
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 매 프레임마다 앞방향(Z축)으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // 충돌 처리 (Is Trigger가 체크되어 있어야 작동합니다)
    private void OnTriggerEnter(Collider other)
    {
        // 몬스터나 벽에 부딪혔을 때
        if (other.CompareTag("Monster"))
        {
            // 예시: Monster 스크립트가 있다면 데미지를 줌
            // other.GetComponent<Monster>().TakeDamage(damage);

            // 부딪혔을 때 폭발 이펙트 등을 생성하면 더 좋습니다.

            // 충돌 후 검기 삭제
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            // 벽에 부딪혀도 삭제
            Destroy(gameObject);
        }
    }
}