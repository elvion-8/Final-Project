using UnityEngine;
using System.Collections.Generic;

public class WeaponEffectManager : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponVFXData
    {
        public string weaponID;       
        public GameObject vfxPrefab;  
    }

    [Header("전체 무기 VFX 데이터 리스트")]
    public List<WeaponVFXData> weaponVFXList = new List<WeaponVFXData>();

    // 무기 ID로 프리팹을 검색하는 함수 (Managers를 통해 호출됨)
    public GameObject GetVFXPrefab(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var data in weaponVFXList)
        {
            if (data.weaponID == id) return data.vfxPrefab;
        }
        
        Debug.LogWarning($"[WeaponEffectManager] {id}에 해당하는 VFX 프리팹을 찾을 수 없습니다.");
        return null;
    }

    // 에디터 스크립트용 ID 배열 반환 함수
    public string[] GetWeaponIDList()
    {
        if (weaponVFXList == null || weaponVFXList.Count == 0) return new string[0];

        string[] ids = new string[weaponVFXList.Count];
        for (int i = 0; i < weaponVFXList.Count; i++)
        {
            ids[i] = string.IsNullOrEmpty(weaponVFXList[i].weaponID) ? "Unnamed" : weaponVFXList[i].weaponID;
        }
        return ids;
    }

}