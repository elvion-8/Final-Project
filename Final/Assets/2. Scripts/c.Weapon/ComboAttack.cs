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

    private bool hasBufferedAttack = false;
    private bool transitionPending = false;
    private float lastAttackTime = 0f;

    private readonly int hashComboCount = Animator.StringToHash("ComboCount");
    private readonly int hashWeaponType = Animator.StringToHash("WeaponType");
    private readonly int hashAttackTrigger = Animator.StringToHash("Attack");

    void Awake()
    {
        player = GetComponent<PlayerCtrl>();
        anim = GetComponentInChildren<Animator>();
        attackMotion = GetComponent<AttackMotion>();
    }

    void Update()
    {
        if (player.isAttacking)
        {
            int layer = GetAttackLayer();
            float normTime = GetCurrentNormalizedTime();

            if (transitionPending)
            {
                if ((Time.time - lastAttackTime >= 0.15f && normTime < 0.25f) || Time.time - lastAttackTime > 0.5f)
                {
                    transitionPending = false;
                }
            }

            if (!transitionPending && hasBufferedAttack && !anim.IsInTransition(layer))
            {
                GetWeaponComboStats(out int weaponTypeVal, out int currentMaxCombo);

                if (comboCount < currentMaxCombo)
                {
                    if (normTime >= 0.35f && normTime < 0.9f)
                    {
                        hasBufferedAttack = false;
                        comboCount += 1;
                        TriggerAttack(comboCount, weaponTypeVal);
                    }
                }
                else if (comboCount == currentMaxCombo)
                {
                    if (normTime >= 0.9f)
                    {
                        hasBufferedAttack = false;
                        comboCount = 1;
                        TriggerAttack(comboCount, weaponTypeVal);
                    }
                }
            }
        }
        else
        {
            if (comboCount != 0 || hasBufferedAttack || transitionPending)
            {
                ResetCombo();
            }
        }
    }

    public void IsComboAttack()
    {
        if (!IsInComboAnimation())
        {
            ResetCombo();
        }

        GetWeaponComboStats(out int weaponTypeVal, out int currentMaxCombo);

        if (player.isAttacking == false)
        {
            player.isAttacking = true;
            comboCount = 1;
            hasBufferedAttack = false;
            TriggerAttack(comboCount, weaponTypeVal);
        }
        else
        {
            if (transitionPending)
            {
                hasBufferedAttack = true;
                return;
            }

            float normTime = GetCurrentNormalizedTime();

            if (normTime < 0.35f)
            {
                hasBufferedAttack = true;
            }
            else if (normTime >= 0.35f && normTime < 0.9f)
            {
                if (comboCount < currentMaxCombo)
                {
                    hasBufferedAttack = false;
                    comboCount += 1;
                    TriggerAttack(comboCount, weaponTypeVal);
                }
                else
                {
                    hasBufferedAttack = true;
                }
            }
            else if (normTime >= 0.9f)
            {
                comboCount = 1;
                hasBufferedAttack = false;
                TriggerAttack(comboCount, weaponTypeVal);
            }
        }
    }

    private void GetWeaponComboStats(out int weaponType, out int maxComboLimit)
    {
        weaponType = 0;
        if (attackMotion != null)
        {
            weaponType = attackMotion.GetCurrentWeaponType();
        }

        maxComboLimit = maxCombo;
        if (attackMotion != null && attackMotion.currentWeaponData != null)
        {
            maxComboLimit = attackMotion.currentWeaponData.maxCombo;
        }
    }

    public int GetAttackLayer()
    {
        if (anim == null) return -1;
        int layer = anim.GetLayerIndex("Attack");
        if (layer != -1 && anim.GetLayerWeight(layer) > 0.5f)
        {
            return layer;
        }

        for (int i = 1; i < anim.layerCount; i++)
        {
            if (anim.GetLayerWeight(i) > 0.5f)
            {
                return i;
            }
        }
        return 0;
    }

    private float GetCurrentNormalizedTime()
    {
        if (anim == null) return 0f;
        int layer = GetAttackLayer();
        if (layer == -1) return 0f;

        if (anim.IsInTransition(layer))
        {
            AnimatorStateInfo nextStateInfo = anim.GetNextAnimatorStateInfo(layer);
            return nextStateInfo.normalizedTime;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime;
    }

    public bool IsInComboAnimation()
    {
        if (anim == null) return false;
        int layer = GetAttackLayer();
        if (layer == -1) return false;

        if (anim.IsInTransition(layer))
        {
            AnimatorStateInfo nextStateInfo = anim.GetNextAnimatorStateInfo(layer);
            bool nextIsIdle = nextStateInfo.IsName("New State") || nextStateInfo.IsName("Idle") || nextStateInfo.IsName("Empty");
            return !nextIsIdle;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layer);
        bool currentIsIdle = stateInfo.IsName("New State") || stateInfo.IsName("Idle") || stateInfo.IsName("Empty");
        return !currentIsIdle;
    }

    private void TriggerAttack(int combo, int weaponType)
    {
        lastAttackTime = Time.time;
        transitionPending = true;

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

    public void ResetCombo()
    {
        comboCount = 0;
        hasBufferedAttack = false;
        transitionPending = false;
        if (anim != null)
        {
            anim.SetInteger(hashComboCount, comboCount);
            anim.ResetTrigger(hashAttackTrigger);
            anim.applyRootMotion = false; // 루트 모션 비활성화
        }
        player.isAttacking = false;
    }
}
