using System.Collections;
using UnityEngine;

public class ComboAttack : MonoBehaviour
{
    private int comboCount = 0;
    public int maxCombo = 3;
    public float comboTime = 0.8f;
    PlayerCtrl player;
    Animator anim;
    AttackMotion attackMotion;
    private Coroutine comboResetCor;

    bool comboAttack = false;

    private readonly int hashComboCount = Animator.StringToHash("ComboCount");
    private readonly int hashWeaponType = Animator.StringToHash("WeaponType");
    private readonly int hashAttackTrigger = Animator.StringToHash("Attack");

    void Awake()
    {
        player = GetComponent<PlayerCtrl>();
        anim = GetComponentInChildren<Animator>();
        attackMotion = GetComponent<AttackMotion>();
    }

    public void IsComboAttack()
    {
        int weaponTypeVal = 0;
        if (attackMotion != null)
        {
            weaponTypeVal = attackMotion.GetCurrentWeaponType();
        }
        int currentMaxCombo = maxCombo;
        float currentComboTime = comboTime;

        if (attackMotion != null && attackMotion.currentWeaponData != null)
        {
            currentMaxCombo = attackMotion.currentWeaponData.maxCombo;
            currentComboTime = attackMotion.currentWeaponData.comboTime;
        }

        if (player.isAttacking == false)
        {
            player.isAttacking = true;
            comboCount = 1;
            TriggerAttack(comboCount, weaponTypeVal, currentComboTime);
        }
        else
        {
            if (comboResetCor != null)
            {
                comboCount += 1;
                if (comboCount > currentMaxCombo) comboCount = 1;
                TriggerAttack(comboCount, weaponTypeVal, currentComboTime);
            }
        }
    }

    private void TriggerAttack(int combo, int weaponType, float customComboTime)
    {
        if (comboResetCor != null) StopCoroutine(comboResetCor);
        comboResetCor = StartCoroutine(ResetComboTimer(customComboTime));

        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.inRoom)
        {
            pv.RPC("NetworkAttack", PhotonTargets.All, combo, weaponType);
        }
        else
        {
            player.NetworkAttack(combo, weaponType);
        }
    }

    IEnumerator ResetComboTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetCombo();
    }

    void ResetCombo()
    {
        comboCount = 0;
        comboAttack = false;
        anim.SetInteger(hashComboCount, comboCount);
        anim.applyRootMotion = false; // 루트 모션 비활성화
        if (comboResetCor != null) StopCoroutine(comboResetCor);
        player.isAttacking = false;
    }
}
