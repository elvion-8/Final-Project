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

    public scPlayerStat pS;

    public int attackDmg { get{if (pS == null) return baseAttackDmg;
            return baseAttackDmg+(pS.weaponDmgUpgradeCnt*5);}}       //공격력
    public float attackRange { get{return baseAttackRange;}}   //공격 범위
    public float attackCoolDown { get;}//공격 쿨다운
    public int attackType { get;}      //공격 타입
    public float weaponMoveSpeed { get;}  //무기 이동속도
    public float attackSpeed { get{if( pS == null) return baseAttackSpeed; 
            return baseAttackSpeed + pS.weaponAttackSpeed;}}   //공격 속도
    public int criticalDmg { get{if( pS == null) return baseCritDmg;
            return baseCritDmg+(pS.weaponCritDmgUpgradeCnt);}}      //치명타 데미지
    public int criticalProb { get{if (pS == null) return baseCritProb;
            return baseCritProb+(pS.weaponCritProbUpgradeCnt);}}     //치명타 확률
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
        //ITakeDamage target = other.GetComponent<ITakeDamage>();

        //if(target != null)
        //{
        //    target.TakeDamage(attackDmg);
        //}

        PlayerCtrl player = GetPlayerCtrl();
        if (player != null && player.isAttacking == true)
        {
            if (other.gameObject.tag == "Enemy")
            {
                // 연속 타격이 씹히지 않도록 짧은 고정 무적 시간(0.15초) 적용
                if (Time.time < HitTime + 0.15f)
                {
                    // 아직 무적 시간 중이라면 데미지 무시
                    return;
                }

                // 타격 이펙트 활성화 및 닿은 지점 전달
                if (player.currentHitVFXState != null)
                {
                    Vector3 contactPoint = other.ClosestPoint(transform.position);
                    player.currentHitVFXState.TriggerHitVFX(contactPoint);
                }

                other.GetComponentInParent<EnemyCtrl>().TakeDamage(Damage());

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
        int multiplier = 1;

        if (Random.Range(0, 30) < criticalProb)
        {
            multiplier = criticalDmg;
            Debug.Log("크리티컬 히트!");
        }

        damage = multiplier * attackDmg;

        return damage;
    }

    void WeaponDie()
    {
        Debug.Log("내구도 0");
        Destroy(gameObject);
    }
}
