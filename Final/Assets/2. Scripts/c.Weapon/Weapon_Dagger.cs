using System.Security;
using UnityEngine;

public class Weapon_Dagger : scWeaponBase
{
    PlayerCtrl player;
    private int tempJumpCnt;
    Transform pTr;
    
    

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        pTr = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        
    }
    void Start()
    {
        tempJumpCnt = player.jumpCnt;
        player.jumpCnt +=1;
    }

    public override void Skill1()
    {
        RaycastHit rayMove;
        bool SkillTrue = Physics.Raycast(pTr.position + Vector3.up * 1.0f, pTr.forward, out rayMove, 10);
        if (!SkillTrue)
        {
            pTr.position += pTr.forward * 10;
        }
        else { return; }
    }

    public override void Skill2() { }

    void OnDestroy()
    {
        player.jumpCnt = tempJumpCnt;
    }
}
