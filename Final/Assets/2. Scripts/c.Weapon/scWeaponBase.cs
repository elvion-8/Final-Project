using UnityEngine;

public abstract class scWeaponBase : MonoBehaviour, IWeaponStats
{
    [Header("Base")]
    [SerializeField][Range(1,100)] protected int baseAttackDmg;
    [SerializeField][Range(1,100)] protected int baseAttackRange;
    [SerializeField][Range(1,100)] protected int baseCritProb;
    [SerializeField][Range(1,100)] protected int baseCritDmg;
    [SerializeField][Range(1,100)] protected int baseAttackSpeed;
    [SerializeField][Range(1,500)] protected int baseDurability;

    private PlayerStatController GetStatController()
    {
        PlayerCtrl player = GetPlayerCtrl();
        return player != null ? player.statController : null;
    }

    public int attackDmg { get {
        var sc = GetStatController();
        return sc != null ? sc.GetWeaponDamage(baseAttackDmg) : baseAttackDmg;
    }}       //공격력
    public float attackRange => baseAttackRange;   //공격 범위
    public float attackCoolDown { get;}//공격 쿨다운
    public int attackType { get;}      //공격 타입
    public float weaponMoveSpeed { get;}  //무기 이동속도
    public float attackSpeed { get {
        var sc = GetStatController();
        return sc != null ? sc.GetWeaponAttackSpeed(baseAttackSpeed) : baseAttackSpeed;
    }}   //공격 속도
    public int criticalDmg { get {
        var sc = GetStatController();
        return sc != null ? sc.GetCritDamage(baseCritDmg) : baseCritDmg;
    }}      //치명타 데미지
    public int criticalProb { get {
        var sc = GetStatController();
        return sc != null ? sc.GetCritProbability(baseCritProb) : baseCritProb;
    }}     //치명타 확률
    public int Durability { get; private set; }     //내구도

    int damage;                        //데미지

    float HitTime;                     //히트후 무적시간

    private PlayerCtrl _playerCtrl;
    private PlayerCtrl GetPlayerCtrl()
    {
        if (_playerCtrl == null)
        {
            _playerCtrl = GetComponentInParent<PlayerCtrl>();
            if (_playerCtrl == null)
            {
                _playerCtrl = transform.root.GetComponentInChildren<PlayerCtrl>();
            }
        }
        return _playerCtrl;
    }

    public void Equip()                //무기 착용
    {}

    protected virtual void OnEnable()
    {
        // 내구도 초기화
        Durability = baseDurability;
    }

    public abstract void Skill1();               //무기별 기본 스킬 2개 정도는 넣기!

    public abstract void Skill2();

    void OnTriggerEnter(Collider other)
    {
        PlayerCtrl player = GetPlayerCtrl();
        if (player != null && player.isAttacking == true)
        {
            if (other.gameObject.tag == "Enemy")
            {
                if (Time.time < HitTime + 0.15f)
                {
                    return;
                }

                if (player.currentHitVFXState != null)
                {
                    Vector3 contactPoint = other.ClosestPoint(transform.position);
                    player.currentHitVFXState.TriggerHitVFX(contactPoint);
                }

                int finalDamage = Damage();
                other.GetComponentInParent<EnemyCtrl>().TakeDamage(finalDamage);

                // 흡혈(Life Steal)
                var sc = GetStatController();
                if (sc != null)
                {
                    float lifeStealPct = sc.GetBuffValue(CharacterStatType.LifeSteal);
                    if (lifeStealPct > 0.001f)
                    {
                        int healAmount = Mathf.RoundToInt(finalDamage * lifeStealPct);
                        if (healAmount > 0)
                        {
                            player.hp = Mathf.Min(player.hp + healAmount, Mathf.RoundToInt(sc.MaxHP));
                            if (player.hpBar != null)
                            {
                                player.hpBar.fillAmount = (float)player.hp / sc.MaxHP;
                            }
                            Debug.Log($"[흡혈] {healAmount} HP 회복 완료 (흡혈률: {lifeStealPct:P0})");
                        }
                    }
                }

                //내구도 감소
                Durability -= 10;

                //피격된 시간
                HitTime = Time.time;

                if (Durability <= 0)
                {
                    WeaponDie();
                }
            }
        }

    }


    public int Damage()
    {
        float multiplier = 1.0f;

        if (Random.Range(0, 100) < criticalProb)
        {
            multiplier = criticalDmg / 100f;
            Debug.Log("크리티컬 히트!");
        }

        damage = Mathf.RoundToInt(attackDmg * multiplier);

        return damage;
    }

    void WeaponDie()
    {
        Debug.Log("내구도 0");
        Destroy(gameObject);
    }
}
