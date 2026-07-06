using UnityEngine;

public class WeaponHitVFXState : WeaponVFXState
{
    [Header("타격 이펙트 설정")]
    public GameObject hitVFXPrefab;

    [Header("배치 옵션")]
    public Vector3 rotationOffset;

    private Animator _animator;
    private float _lastSpawnTime = 0f;
    private const float SPAWN_COOLDOWN = 0.02f; // 중복 생성 방지용 쿨다운

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _animator = animator;
        _lastSpawnTime = 0f;

        // 플레이어에게 현재 타격 VFX 상태 등록
        var player = animator.GetComponentInParent<PlayerCtrl>();
        if (player != null)
        {
            player.currentHitVFXState = this;
        }
    }

    // 물리 충돌 시 즉시 타격 이펙트 소환
    public void TriggerHitVFX(Vector3 spawnPos)
    {
        if (hitVFXPrefab == null || _animator == null) return;
        if (Time.time < _lastSpawnTime + SPAWN_COOLDOWN) return;

        Quaternion spawnRot = _animator.transform.rotation * Quaternion.Euler(rotationOffset);
        camera.Shake(0.2f,0.3f,10f,true);

        SpawnHitVFX(hitVFXPrefab, spawnPos, spawnRot);
        _lastSpawnTime = Time.time;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponentInParent<PlayerCtrl>();
        if (player != null && player.currentHitVFXState == this)
        {
            player.currentHitVFXState = null;
        }
    }
}
