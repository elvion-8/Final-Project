using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

/// <summary>
/// 적 사망 시 아이템 드랍을 담당하는 컴포넌트.
/// EnemyCtrl과 분리해서 관리 (죽음 처리와 드랍 로직의 책임 분리).
/// </summary>
public class EnemyItemDrop : Photon.MonoBehaviour 
{
    [System.Serializable]
    public class DropEntry
    {
        [Tooltip("드랍할 FieldItem 프리팹")]
        public GameObject fieldItemPrefab;
        public ItemData itemData;
        [Range(0, 1)] public float dropChance = 1f;
        public int minCount = 1;
        public int maxCount = 1;
    }

    [Header("아이템 드랍 테이블")]
    public DropEntry[] dropTable;

    [Tooltip("드랍 위치를 중심에서 얼마나 흩뿌릴지 (min, max 반경)")]
    public Vector2 dropScatterRadius = new Vector2(0.3f, 1.2f);

    public void Drop(Vector3 origin)
    {
        if (!PhotonNetwork.isMasterClient) return;
        //if (dropTable == null || dropTable.Length == 0) return;

        foreach (var entry in dropTable)
        {
            if (entry.fieldItemPrefab == null || entry.itemData == null) continue;
            if (Rand.value > entry.dropChance) continue;

            int count = Rand.Range(entry.minCount, entry.maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Rand.insideUnitCircle.normalized
                                  * Rand.Range(dropScatterRadius.x, dropScatterRadius.y);
                Vector3 pos = origin + new Vector3(offset.x, 0.3f, offset.y);

                SpawnDropItem(entry, pos);
            }
        }
    }

    void SpawnDropItem(DropEntry entry, Vector3 pos)
    {   
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return; 
        }
        GameObject dropObj = PhotonNetwork.Instantiate(entry.fieldItemPrefab.name, pos, Quaternion.identity, 0);
        //GameObject dropObj = Instantiate(entry.fieldItemPrefab, pos, Quaternion.identity);

        FieldItem fi = dropObj.GetComponent<FieldItem>();
        if (fi != null)
            fi.itemData = entry.itemData;
        else
            Debug.LogWarning($"[EnemyItemDrop] {entry.fieldItemPrefab.name}: FieldItem 컴포넌트가 없습니다.");
    }
}
