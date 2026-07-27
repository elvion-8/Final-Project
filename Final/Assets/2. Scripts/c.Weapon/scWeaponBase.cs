using UnityEngine;

public abstract class scWeaponBase : MonoBehaviour, IWeaponStats
{
    [Header("Base")]
    [SerializeField][Range(1,500)] protected int baseAttackDmg;
    [SerializeField][Range(1,100)] protected int baseAttackRange;
    [SerializeField][Range(1,100)] protected int baseCritProb;
    [SerializeField][Range(1,500)] protected int baseCritDmg;
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
            // 자신 및 자식 콜라이더 예외 처리
            if (other.transform.root == player.transform.root || other.gameObject == player.gameObject)
            {
                return;
            }

            if (Time.time < HitTime + 0.15f)
            {
                return;
            }

            // ITakeDamage 인터페이스를 구현한 대상(EnemyCtrl, brokenPillar 등) 탐색
            ITakeDamage target = other.GetComponentInParent<ITakeDamage>();
            if (target == null)
            {
                target = other.GetComponent<ITakeDamage>();
            }

            if (target != null && target is Component comp && comp.transform.root == player.transform.root)
            {
                return;
            }

            if (target != null)
            {
                if (player.currentHitVFXState != null)
                {
                    Vector3 contactPoint = other.ClosestPoint(transform.position);
                    player.currentHitVFXState.TriggerHitVFX(contactPoint);
                }

                int finalDamage = Damage();
                if (target is EnemyCtrl enemy)
                {
                    enemy.TakeDamage(finalDamage, player.gameObject);
                }
                else
                {
                    target.TakeDamage(finalDamage);
                }
                Debug.Log($"[scWeaponBase] 타겟({target.GetType().Name})에게 데미지 전달: {finalDamage}");

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
            multiplier = 1.0f + (criticalDmg / 100f);
            Debug.Log($"[크리티컬] 기본공격력: {attackDmg} | 치명타확률: {criticalProb}% | 추가치명타데미지: +{criticalDmg}% ({multiplier:F2}배) ➔ 최종 데미지: {Mathf.RoundToInt(attackDmg * multiplier)}");
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
