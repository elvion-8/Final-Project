using UnityEngine;

public class Weapon_Sword : MonoBehaviour, IWeaponStats
{
    public int attackDmg { get; private set;} = 100;       //공격력
    public float attackRange { get; private set;}=5f;   //공격 범위
    public float attackCoolDown { get; private set;}=1f;//공격 쿨다운
    public int attackType { get; private set;}=1;      //공격 타입
    public float weaponMoveSpeed { get; private set;}=1f;//무기 이동속도
    public float attackSpeed { get; private set;}=1f;   //공격 속도
    public int criticalDmg {get; private set;}=2;      //치명타 데미지
    public int criticalProb {get; private set;}=10;     //치명타 확률
    //public WeaponType Type { get; private set;}=      //무기 종류

    void Awake()
    {
        //애니메이션 컴포넌트 연결
    }
    //[ContextMenu.test("")]
    public void Equip()                //무기 착용
    {Debug.Log("스워드임");
    Debug.Log(attackDmg);}

    public void Skill1()
    {}
    public void Skill2()
    {}
}
