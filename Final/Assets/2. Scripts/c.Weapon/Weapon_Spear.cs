using System.Collections;
using UnityEngine;


public class Weapon_Spear: scWeaponBase
{
    public Animator anim;

    public Vector3 z35 = new Vector3(0, 0, 35);

    WaitForSeconds delay = new WaitForSeconds(2.5f);

    public override void Skill1()
    {
        StartCoroutine(SkillRoutine());
    }
    public override void Skill2()
    {}

    IEnumerator SkillRoutine()
    {
        transform.localRotation = Quaternion.Euler(-z35);

        Debug.Log("스킬애니1");
        anim = GameObject.Find("Player").GetComponentInChildren<Animator>();

        anim.SetTrigger("Skill1");

        yield return delay;

        transform.localRotation = Quaternion.Euler(Vector3.zero);

    }

}
