using UnityEngine;

public class WeaponHitVFXState : WeaponVFXState
{
    [Header("타격 이펙트 설정")]
    public GameObject hitVFXPrefab;
    [Range(0f, 1f)] public float hitTime = 0.5f;

    [Header("루프 애니메이션 여부")]
    public bool isLooping = false;

    [Header("배치 옵션")]
    public bool useWeaponPosition = true;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    private bool _hasSpawned = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _hasSpawned = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitVFXPrefab == null) return;

        float normalizedTime = isLooping ? (stateInfo.normalizedTime % 1f) : stateInfo.normalizedTime;

        if (normalizedTime >= hitTime)
        {
            if (!_hasSpawned)
            {
                Transform baseTransform = useWeaponPosition ? GetWeaponTransform(animator) : animator.transform;
                Vector3 spawnPos = baseTransform.position + baseTransform.TransformDirection(positionOffset);
                Quaternion spawnRot = baseTransform.rotation * Quaternion.Euler(rotationOffset);

                SpawnHitVFX(hitVFXPrefab, spawnPos, spawnRot);
                _hasSpawned = true;
            }
        }
        else
        {
            if (normalizedTime < hitTime)
            {
                _hasSpawned = false;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hasSpawned = false;
    }
}
