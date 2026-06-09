using System.Collections;
using UnityEngine;


public class Weapon_Spear: scWeaponBase
{
    PlayerCtrl player;
    private int tempJumpCnt;
    Transform pTr;

    public Animator anim;

    public Vector3 z35 = new Vector3(0, 0, 35);

    WaitForSeconds delay1 = new WaitForSeconds(0.4f);

    WaitForSeconds delay2 = new WaitForSeconds(1.0f);

    WaitForSeconds delay3 = new WaitForSeconds(0.04f);

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        pTr = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        anim = GameObject.Find("Player").GetComponentInChildren<Animator>();
    }

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
       
        anim.SetTrigger("Skill1");

        RaycastHit rayMove;

        yield return delay1;

        for (int i = 0; i < 16; i++)
        {
            bool SkillTrue = Physics.Raycast(pTr.position + Vector3.up * 1.0f, pTr.forward / 2, out rayMove, 10);
            if (!SkillTrue)
            {
                pTr.position += pTr.forward / 2;
            }

            yield return delay3;
        }

        yield return delay2;

        transform.localRotation = Quaternion.Euler(Vector3.zero);

    }

}
