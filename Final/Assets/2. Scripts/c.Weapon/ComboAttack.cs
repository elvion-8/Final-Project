using System.Collections;
using UnityEngine;

public class ComboAttack : MonoBehaviour
{
    private int comboCount = 0;
    public int maxCombo = 3;
    public float comboTime = 0.8f;
    PlayerCtrl player;
    Animator anim;
    private Coroutine comboResetCor;

    bool comboAttack = false;

    private readonly int hashComboCount = Animator.StringToHash("ComboCount");
    private readonly int hashAttackTrigger = Animator.StringToHash("Attack");

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        anim = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Animator>();
    }

    public void IsComboAttack()
    {
        if(player.isAttacking==true)
        {
            comboAttack = true;
            comboCount+=1;
            anim.SetInteger(hashComboCount,comboCount);
            anim.SetTrigger(hashAttackTrigger);
            comboResetCor = StartCoroutine(ResetComboTimer());
        }
    }
    IEnumerator ResetComboTimer()
    {
        yield return new WaitForSeconds(comboTime);
        ResetCombo();
    }

    void ResetCombo()
    {
        comboCount = 0;
        comboAttack = false;
        anim.SetInteger(hashComboCount,comboCount);
        if(comboResetCor != null) StopCoroutine(comboResetCor);
        player.isAttacking = false;
    }
}
