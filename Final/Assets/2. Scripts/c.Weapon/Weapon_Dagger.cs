using UnityEngine;

public class Weapon_Dagger : scWeaponBase
{
    private PlayerCtrl player;

    void Awake()
    {
        player = GetComponentInParent<PlayerCtrl>();
    }
    void Start()
    {
        player.jumpCnt = 2;
    }
    public override void Skill1(){}   

    public override void Skill2(){}
    void OnDestroy()
    {
        player.jumpCnt=1;
    }
}
