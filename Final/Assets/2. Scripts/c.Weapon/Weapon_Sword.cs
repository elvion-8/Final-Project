using UnityEngine;


public class Weapon_Sword : scWeaponBase
{
    public Animator anim;

    public override void Skill1()
    {
        Debug.Log("스킬애니1");
        anim = GameObject.Find("Player").GetComponentInChildren<Animator>();

        anim.SetTrigger("Skill1");
    }
    public override void Skill2()
    {}
    
}
