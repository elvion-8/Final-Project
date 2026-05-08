using UnityEngine;
using UnityEngine.EventSystems;

public class TrashZone : MonoBehaviour, IDropHandler
{
    public Transform playerTransform;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (csSlot.draggingSlot != null)
        {
            ItemData droppedItem = csSlot.draggingSlot.itemData;
            if (droppedItem != null)
            {
                // 프리팹이 있다면 맵에 생성 (플레이어 근처)
                if (droppedItem.itemPrefab != null && playerTransform != null)
                {
                    Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.5f;
                    Instantiate(droppedItem.itemPrefab, dropPosition, Quaternion.identity);
                }

                // 인벤토리에서 삭제
                csSlot.draggingSlot.UpdateSlotUI(null);
                Debug.Log(droppedItem.itemName + "을(를) 버렸습니다.");
            }
        }
    }
}
