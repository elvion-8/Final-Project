using System.Security;
using UnityEngine;

public class Weapon_Dagger : scWeaponBase
{
    PlayerCtrl player;
    private int tempJumpCnt;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
    }
    void Start()
    {
        tempJumpCnt = player.jumpCnt;
        player.jumpCnt +=1;
    }

    public override void Skill1() { }

    public override void Skill2() { }

    void OnDestroy()
    {
        player.jumpCnt = tempJumpCnt;
    }
}
