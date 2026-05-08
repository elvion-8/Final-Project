using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum SlotType
{
    Weapon,
    Buff,
    Hotbar
}

public class csSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SlotType slotType;
    public Image itemIcon;
    public ItemData itemData;

    public static csSlot draggingSlot = null;
    public static GameObject dragIconGO = null;

    private void Start()
    {
        UpdateSlotUI(itemData);
    }

    public void UpdateSlotUI(ItemData data)
    {
        itemData = data;
        if (itemData != null)
        {
            itemIcon.sprite = itemData.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }
        else
        {
            itemIcon.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null) return;

        draggingSlot = this;

        dragIconGO = new GameObject("DragIcon");
        dragIconGO.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragIconGO.transform.SetAsLastSibling();

        Image img = dragIconGO.AddComponent<Image>();
        img.sprite = itemData.itemIcon;
        img.raycastTarget = false;

        Color c = itemIcon.color;
        c.a = 0.5f;
        itemIcon.color = c;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconGO != null)
        {
            dragIconGO.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconGO != null)
        {
            Destroy(dragIconGO);
            dragIconGO = null;
        }

        if (draggingSlot != null)
        {
            Color c = draggingSlot.itemIcon.color;
            c.a = 1f;
            draggingSlot.itemIcon.color = c;
            draggingSlot = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggingSlot == null) return;
        if (draggingSlot == this) return;

        ItemData draggedItem = draggingSlot.itemData;
        ItemData myItem = this.itemData;

        if (CanAcceptItem(draggedItem) && draggingSlot.CanAcceptItem(myItem))
        {
            draggingSlot.UpdateSlotUI(myItem);
            this.UpdateSlotUI(draggedItem);
        }
    }

    private bool CanAcceptItem(ItemData data)
    {
        if (data == null) return true;

        if (slotType == SlotType.Weapon || slotType == SlotType.Hotbar)
        {
            return data.itemType == ItemType.Weapon;
        }
        else if (slotType == SlotType.Buff)
        {
            return data.itemType == ItemType.Buff;
        }
        return false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(itemData.itemName, itemData.itemDesc);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
