using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class VFXEventModifier : MonoBehaviour
{
    [Header("연동 설정")]
    [HideInInspector] public string selectedWeaponID; 

    [Header("VFX 스폰 위치 설정")]
    public Transform targetTransform; 
    public Vector3 positionOffset = Vector3.zero; 
    public Vector3 rotationOffset = Vector3.zero; 

    private GameObject instantiatedVFX;

    // 5. 애니메이션 이벤트에 의해 호출될 런타임 함수 (ON)
    public void TurnOnVFX()
    {
        if (Managers.weaponEffect == null)
        {
            Debug.LogError("[VFXEventModifier] Managers에 WeaponEffectManager가 세팅되지 않았습니다.");
            return;
        }

        GameObject vfxPrefab = Managers.weaponEffect.GetVFXPrefab(selectedWeaponID);
        if (vfxPrefab == null) return;

        if (instantiatedVFX == null)
        {
            Transform parentTransform = targetTransform != null ? targetTransform : transform;
            instantiatedVFX = Instantiate(vfxPrefab, parentTransform);
        }

        instantiatedVFX.transform.localPosition = positionOffset;
        instantiatedVFX.transform.localRotation = Quaternion.Euler(rotationOffset);
        
        instantiatedVFX.SetActive(true);
    }

    // 5. 애니메이션 이벤트에 의해 호출될 런타임 함수 (OFF)
    public void TurnOffVFX()
    {
        if (instantiatedVFX != null)
        {
            instantiatedVFX.SetActive(false);
        }
    }
}