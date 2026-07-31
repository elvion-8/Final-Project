using UnityEngine;

public class brokenPillar : MonoBehaviour, ITakeDamage
{
    public int hp = 300;
    public Animator anim;
    private bool isBroken = false;

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    public int TakeDamage(int damage)
    {
        if (isBroken) return hp;

        hp -= damage;
        Debug.Log($"[brokenPillar] 피격! 받은 데미지: {damage}, 남은 HP: {hp}");

        if (hp <= 0)
        {
            hp = 0;
            isBroken = true;
            if (anim != null)
            {
                anim.SetTrigger("broken");
            }
            Debug.Log("[brokenPillar] 기둥 파괴! broken 트리거 작동");
        }

        return hp;
    }
}
