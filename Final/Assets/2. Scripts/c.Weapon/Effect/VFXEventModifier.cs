using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class VFXEventModifier : MonoBehaviour
{
    [Header("연동 설정")]
    //[HideInInspector]
    public string selectedWeaponID; 
    public int selectedWeaponIndex = -1;

    [Header("VFX 스폰 위치 설정")]
    public Transform targetTransform; 
    public Vector3 positionOffset = Vector3.zero; 
    public Vector3 rotationOffset = Vector3.zero;

    [SerializeField]
    private GameObject instantiatedVFX;

    // 현재 하이어라키에 생성되어 활성화된 이펙트의 정보 기억
    private string currentLoadedVFXID;
    private int currentLoadedVFXIndex = -1;


    
    /// 외부에서 '무기 ID(문자열)'로 변경할 때
    
    public void ChangeWeaponByID(string newWeaponID)
    {
        if (selectedWeaponID == newWeaponID) return;

        selectedWeaponID = newWeaponID;
        selectedWeaponIndex = -1; // ID 기반이므로 인덱스는 초기화
        ClearCurrentVFX();
    }

    
    /// 외부에서 '배열 인덱스 번호'로 변경할 때
    
    public void ChangeWeaponByIndex(int newIndex)
    {
        if (selectedWeaponIndex == newIndex) return;

        selectedWeaponIndex = newIndex;
        selectedWeaponID = string.Empty; // 인덱스 기반이므로 ID는 초기화
        ClearCurrentVFX();
    }



    // 애니메이션 이벤트에 의해 호출될 런타임 함수 (ON)
    public void TurnOnVFX()
    {
        if (Managers.weaponEffect == null)
        {
            Debug.LogError("[VFXEventModifier] Managers에 WeaponEffectManager가 세팅되지 않았습니다.");
            return;
        }



        GameObject vfxPrefab = null;
        
        if (selectedWeaponIndex >= 0)
        {
            if (instantiatedVFX != null && currentLoadedVFXIndex != selectedWeaponIndex)
            {
                ClearCurrentVFX();
            }
            vfxPrefab = Managers.weaponEffect.GetVFXPrefabByIndex(selectedWeaponIndex);
        }
        else if (!string.IsNullOrEmpty(selectedWeaponID))
        {
            if (instantiatedVFX != null && currentLoadedVFXID != selectedWeaponID)
            {
                ClearCurrentVFX();
            }
            vfxPrefab = Managers.weaponEffect.GetVFXPrefab(selectedWeaponID);
        }

        if (vfxPrefab == null) return;


        if (instantiatedVFX == null)
        {
            Transform parentTransform = targetTransform != null ? targetTransform : transform;
            instantiatedVFX = Instantiate(vfxPrefab, parentTransform);

            currentLoadedVFXID = selectedWeaponID;
            currentLoadedVFXIndex = selectedWeaponIndex;
        }

        instantiatedVFX.transform.localPosition = positionOffset;
        instantiatedVFX.transform.localRotation = Quaternion.Euler(rotationOffset);
        
        instantiatedVFX.SetActive(true);
    }

    // 애니메이션 이벤트에 의해 호출될 런타임 함수 (OFF)
    public void TurnOffVFX()
    {
        if (instantiatedVFX != null)
        {
            instantiatedVFX.SetActive(false);
        }
    }
    public void ClearCurrentVFX()
    {
        if (instantiatedVFX != null)
        {
            Destroy(instantiatedVFX);
            instantiatedVFX = null;
        }
        currentLoadedVFXID = string.Empty;
        currentLoadedVFXIndex = -1;
    }
}