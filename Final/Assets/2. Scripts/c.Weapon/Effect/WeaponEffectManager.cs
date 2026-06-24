using UnityEngine;
using System.Collections.Generic;

public class WeaponEffectManager : MonoBehaviour
{
    [System.Serializable]
    public struct VFXSetData
    {
        public string actionID;          // 액션/스킬 이름 (예: "Double_Slash", "Fire_Swing")

        //[Header("동시 재생할 프리팹들 (선택 사항)")]
        public GameObject weaponVFXPrefab;    // 무기에 붙을 이펙트
        public GameObject particleVFXPrefab;  // 씬/바닥에 생성될 파티클
    }

    [Header("★ 액션별 VFX 세트 관리 리스트")]
    public List<VFXSetData> vfxSetList = new List<VFXSetData>();

    /// <summary>
    /// ID로 해당 액션의 전체 VFX 세트 데이터를 가져옵니다.
    /// </summary>
    public VFXSetData GetVFXSet(string id)
    {
        if (string.IsNullOrEmpty(id)) return default;

        foreach (var data in vfxSetList)
        {
            if (data.actionID == id) return data;
        }

        Debug.LogWarning($"[WeaponEffectManager] {id}에 해당하는 VFX 세트를 찾을 수 없습니다.");
        return default;
    }

    public VFXSetData GetVFXSetByIndex(int index)
    {
        if (index < 0 || index >= vfxSetList.Count) return default;
        return vfxSetList[index];
    }

    /// <summary>
    /// 에디터 팝업 목록용 ID 배열 반환
    /// </summary>
    public string[] GetActionIDList()
    {
        if (vfxSetList == null || vfxSetList.Count == 0) return new string[0];

        string[] ids = new string[vfxSetList.Count];
        for (int i = 0; i < vfxSetList.Count; i++)
        {
            ids[i] = string.IsNullOrEmpty(vfxSetList[i].actionID) ? "Unnamed_Action" : vfxSetList[i].actionID;
        }
        return ids;
    }
}