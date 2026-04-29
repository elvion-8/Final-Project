using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

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

    int damage;
    
    

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
    void OnTriggerEnter(Collider other)
    {
        //ITakeDamage target = other.GetComponent<ITakeDamage>();

        //if(target != null)
        //{
        //    target.TakeDamage(attackDmg);
        //}

        
            if (other.gameObject.tag == "Enemy")
            {
                other.GetComponent<EnemyCtrl>().TakeDamage(Damage());
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

}
