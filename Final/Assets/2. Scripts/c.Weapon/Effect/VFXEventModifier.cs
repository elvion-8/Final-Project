using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VFXEventModifier : MonoBehaviour
{
    [Header("기능 활성화 토글")]
    public bool useVFXSet = true;         // 매니저 세트 이펙트 사용 여부
    public bool useTrailRenderer = false; // 테일 잔상 사용 여부

    [Header("1. 매니저 이펙트 세트 선택")]
    public string selectedActionID;
    public int selectedActionIndex = -1;

    [Header("2. 독립 테일 레너더 설정")]
    public TrailRenderer targetTrail;

    [Header("VFX 스폰 위치 설정 (공통 적용)")]
    public Transform targetTransform;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    // 생성된 인스턴스들 관리
    [SerializeField] private GameObject instantiatedWeaponVFX;
    [SerializeField] private GameObject instantiatedParticleVFX;

    private string currentLoadedActionID;
    private int currentLoadedActionIndex = -1;

    public void ChangeActionByIndex(int newIndex)
    {
        if (selectedActionIndex == newIndex) return;
        selectedActionIndex = newIndex;
        selectedActionID = string.Empty;
        ClearCurrentVFX();
    }

    // 애니메이션 이벤트 ON
    public void TurnOnVFX()
    {
        // 1. 테일 잔상 처리
        if (useTrailRenderer && targetTrail != null) targetTrail.emitting = true;

        // 2. 이펙트 세트 동시 처리
        if (!useVFXSet || Managers.weaponEffect == null) return;

        WeaponEffectManager.VFXSetData activeSet = default;

        if (selectedActionIndex >= 0)
        {
            if ((instantiatedWeaponVFX != null || instantiatedParticleVFX != null) && currentLoadedActionIndex != selectedActionIndex)
                ClearCurrentVFX();
            activeSet = Managers.weaponEffect.GetVFXSetByIndex(selectedActionIndex);
        }
        else if (!string.IsNullOrEmpty(selectedActionID))
        {
            if ((instantiatedWeaponVFX != null || instantiatedParticleVFX != null) && currentLoadedActionID != selectedActionID)
                ClearCurrentVFX();
            activeSet = Managers.weaponEffect.GetVFXSet(selectedActionID);
        }

        Transform parentTransform = targetTransform != null ? targetTransform : transform;

        // [동시 소환 A] 무기 VFX가 세트에 존재하면 생성 및 재생
        if (activeSet.weaponVFXPrefab != null)
        {
            if (instantiatedWeaponVFX == null)
            {
                instantiatedWeaponVFX = Instantiate(activeSet.weaponVFXPrefab, parentTransform);
                currentLoadedActionID = selectedActionID;
                currentLoadedActionIndex = selectedActionIndex;
            }
            SetTransformOffset(instantiatedWeaponVFX.transform);
            instantiatedWeaponVFX.SetActive(true);
        }

        // [동시 소환 B] 파티클 VFX가 세트에 존재하면 생성 및 재생
        if (activeSet.particleVFXPrefab != null)
        {
            if (instantiatedParticleVFX == null)
            {
                instantiatedParticleVFX = Instantiate(activeSet.particleVFXPrefab, parentTransform);
                currentLoadedActionID = selectedActionID;
                currentLoadedActionIndex = selectedActionIndex;
            }
            SetTransformOffset(instantiatedParticleVFX.transform);
            instantiatedParticleVFX.SetActive(true);

            ParticleSystem ps = instantiatedParticleVFX.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    // 애니메이션 이벤트 OFF (동시에 끄기)
    public void TurnOffVFX()
    {
        if (useTrailRenderer && targetTrail != null) targetTrail.emitting = false;
        if (instantiatedWeaponVFX != null) instantiatedWeaponVFX.SetActive(false);
        if (instantiatedParticleVFX != null) instantiatedParticleVFX.SetActive(false);
    }

    private void SetTransformOffset(Transform target)
    {
        target.localPosition = positionOffset;
        target.localRotation = Quaternion.Euler(rotationOffset);
    }

    public void ClearCurrentVFX()
    {
        if (instantiatedWeaponVFX != null) { Destroy(instantiatedWeaponVFX); instantiatedWeaponVFX = null; }
        if (instantiatedParticleVFX != null) { Destroy(instantiatedParticleVFX); instantiatedParticleVFX = null; }
        currentLoadedActionID = string.Empty;
        currentLoadedActionIndex = -1;
    }
}