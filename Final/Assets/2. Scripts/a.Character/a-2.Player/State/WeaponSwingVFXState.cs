using UnityEngine;

public class WeaponSwingVFXState : WeaponVFXState
{
    [Header("스윙 이펙트 설정")]
    public GameObject swingVFXPrefab;
    [Range(0f, 1f)] public float startTime = 0.2f;
    [Range(0f, 1f)] public float endTime = 0.7f;

    [Header("루프 애니메이션 여부")]
    public bool isLooping = false;

    [Header("배치 옵션")]
    public bool attachToWeapon = true;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    private GameObject _spawnedVFX;
    private bool _hasSpawned = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _spawnedVFX = null;
        _hasSpawned = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (swingVFXPrefab == null) return;

        float normalizedTime = isLooping ? (stateInfo.normalizedTime % 1f) : stateInfo.normalizedTime;

        if (normalizedTime >= startTime && normalizedTime <= endTime)
        {
            if (!_hasSpawned)
            {
                Transform baseTransform = attachToWeapon ? GetWeaponTransform(animator) : animator.transform;
                Vector3 spawnPos = baseTransform.position + baseTransform.TransformDirection(positionOffset);
                Quaternion spawnRot = baseTransform.rotation * Quaternion.Euler(rotationOffset);

                _spawnedVFX = SpawnSwingVFX(swingVFXPrefab, spawnPos, spawnRot, attachToWeapon ? baseTransform : null);
                _hasSpawned = true;
            }
        }
        else
        {
            if (_spawnedVFX != null)
            {
                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.ReturnVFX(_spawnedVFX);
                }
                _spawnedVFX = null;
            }
            
            if (normalizedTime < startTime)
            {
                _hasSpawned = false;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_spawnedVFX != null)
        {
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ReturnVFX(_spawnedVFX);
            }
            _spawnedVFX = null;
        }
        _hasSpawned = false;
    }
}
