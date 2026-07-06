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
    PhotonView pv;

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
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            if (player.isAttacking)
            {
                int layer = GetAttackLayer();
                float normTime = GetCurrentNormalizedTime();

                if (transitionPending)
                {
                    // 현재 공격 애니메이션의 실제 재생 시간(초)을 가져옴
                    float animLength = 1.0f;
                    if (anim != null && layer != -1)
                    {
                        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layer);
                        if (stateInfo.length > 0.01f)
                        {
                            animLength = stateInfo.length;
                        }
                    }

                    // 최소 0.5초 대기를 보장하여 빠른 무기(검)의 트랜지션 씹힘 방지
                    float transitionLimit = Mathf.Max(0.5f, animLength * 0.6f);
                    float transitionStartCheck = 0.15f; // 트랜지션 안정성을 위해 고정

                    if ((Time.time - lastAttackTime >= transitionStartCheck && normTime < 0.25f) || Time.time - lastAttackTime > transitionLimit)
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
                    if (PhotonNetwork.inRoom)
                    {
                        pv.RPC("ResetCombo", PhotonTargets.AllBuffered);
                    }
                    else
                    {
                        ResetCombo();
                    }
                }
            }
        }
    }

    public void IsComboAttack()
    {
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            if (!IsInComboAnimation())
            {
                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("ResetCombo", PhotonTargets.AllBuffered);
                }
                else
                {
                    ResetCombo();
                }
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
        if (pv.isMine || !PhotonNetwork.inRoom)
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
    }

    [PunRPC]
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
