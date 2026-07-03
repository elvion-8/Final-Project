using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class WeaponTrailState : WeaponVFXState
{
    [Header("트레일 활성화 구간 설정")]
    [Range(0f, 1f)] public float startTime = 0.2f;
    [Range(0f, 1f)] public float endTime = 0.7f;

    [Header("루프 애니메이션 여부")]
    public bool isLooping = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (trail == null) return;
        
        float normalizedTime = isLooping ? (stateInfo.normalizedTime % 1f) : stateInfo.normalizedTime;
        bool activeTrail = (normalizedTime >= startTime && normalizedTime <= endTime);
        SetTrailActive(activeTrail);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (trail != null) 
            SetTrailActive(false);
    }
}
