public interface IWeaponStats
{
    public int attackDmg { get;}       //공격력
    public float attackRange { get;}   //공격 범위
    public float attackCoolDown { get;}//공격 쿨다운
    public int attackType { get;}      //공격 타입
    public float weaponMoveSpeed { get;}//무기 이동속도
    public float attackSpeed { get;}   //공격 속도
    public int criticalDmg {get;}      //치명타 데미지
    public int criticalProb {get;}     //치명타 확률
   // public WeaponType Type { get;}      //무기 종류

    public void Equip();                //무기 착용
}