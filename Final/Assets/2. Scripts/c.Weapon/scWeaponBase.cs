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

    private scPlayerStat pS;

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
    public int Durability { get; }     //내구도


    public void Equip()                //무기 착용
    {}

    public abstract void Skill1();               //무기별 기본 스킬 2개 정도는 넣기!

    public abstract void Skill2();
}
