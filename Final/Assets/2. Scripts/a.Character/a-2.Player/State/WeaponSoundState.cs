using UnityEngine;

public class WeaponSoundState : StateMachineBehaviour
{
    [Header("사운드 시작 타이밍 설정")]
    public bool useFrameBased = false;

    [Range(0f, 1f)]
    [Tooltip("useFrameBased가 false일 때 사용: 애니메이션 진행률 (0.0 ~ 1.0)")]
    public float soundStartTimeNormalized = 0.2f;

    [Tooltip("useFrameBased가 true일 때 사용: 재생할 시작 프레임 번호")]
    public int targetFrame = 5;
    
    [Tooltip("useFrameBased가 true일 때 사용: 애니메이션의 총 프레임 수")]
    public int totalFrames = 30;

    [Header("루프 애니메이션 여부")]
    public bool isLooping = false;

    private bool _hasPlayed = false;
    private PlayerCtrl _player;
    private AttackSound _sound;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hasPlayed = false;
        
        // PlayerCtrl 및 AttackSound 컴포넌트 획득
        _player = animator.GetComponentInParent<PlayerCtrl>();
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }

        if (_player != null)
        {
            _sound = _player.GetComponentInChildren<AttackSound>();
        }
        else
        {
            _sound = animator.GetComponentInChildren<AttackSound>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float normalizedTime = isLooping ? (stateInfo.normalizedTime % 1f) : stateInfo.normalizedTime;
        float startTime = GetStartTimeNormalized();

        if (normalizedTime >= startTime)
        {
            if (!_hasPlayed)
            {
                PlayWeaponSound();
                _hasPlayed = true;
            }
        }
        else
        {
            _hasPlayed = false;
        }
    }

    private float GetStartTimeNormalized()
    {
        if (useFrameBased && totalFrames > 0)
        {
            return (float)targetFrame / totalFrames;
        }
        return soundStartTimeNormalized;
    }

    private void PlayWeaponSound()
    {
        if (_sound != null)
        {
            _sound.PlayAttackSFX();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hasPlayed = false;
    }
}
