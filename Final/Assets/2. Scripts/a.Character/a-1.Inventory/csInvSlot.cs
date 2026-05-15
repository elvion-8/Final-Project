using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum InvSlotType
{
    Weapon,
    Buff,
    Hotbar
}

public class csInvSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Inventory img")]
    public SlotType slotType;
    public Image itemIcon;      //icon img
    public Image borderImg;     //Slot Background img

    [Header("Inventory Slot Sprite")]
    [SerializeField] private Sprite defaultIconImg;
    [SerializeField] private Sprite selectIconImg;
    [SerializeField] private Sprite defaultBorderImg;
    [SerializeField] private Sprite selectBorderImg; //선택 시 발광 하는 이미지
    [SerializeField] private Sprite emptySlot;

    //sprite 이미지를 담을 수 있는 image 변수 2개 선언, sprite가 바뀌면 그에 따라 Image 변수에 스프라이트 이미지가 바뀌는 방식임.

    public ItemData _itemData;
    private bool _isEmpty = true;
    private bool _isSelected = false;
    public event Action<csInvSlot> OnSlotClicked;
    public void Setup(ItemData data)
    {
        _itemData = data;
        _isEmpty = (data == null);
        if(!_isEmpty)
        {
            defaultIconImg = data.itemIcon;
            selectIconImg = data.iconLit;
        }
        //_isSelected = false;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        //if(_isSelected == selected) return;
        _isSelected = selected;
        RefreshVisuals();
    }
    public ItemData GetData() => _itemData;
    public bool IsEmpty => _isEmpty;

    private void RefreshVisuals()
    {
        if(_isEmpty)
        {
            if(itemIcon != null)
            {
                itemIcon.sprite = emptySlot;
                itemIcon.enabled = emptySlot != null;
            }
            SetBorder(false);
            return;
        }
        SetIcon(_isSelected);
        SetBorder(_isSelected);
    }
    void SetIcon(bool lit)
    {
        if(itemIcon == null)return;
        itemIcon.enabled = true;
        Sprite litSprite = (_itemData != null && _itemData.iconLit!=null)?_itemData.iconLit : defaultIconImg;
        Sprite normalSprite = (_itemData != null && _itemData.itemIcon != null) ? _itemData.itemIcon : selectIconImg;

        itemIcon.sprite = lit ? litSprite : normalSprite;
    }

    void SetBorder(bool lit)
    {
        if(borderImg == null) return;

        Sprite litSprite = (_itemData != null && selectBorderImg!=null)?selectBorderImg : defaultBorderImg;
        Sprite normalSprite = (_itemData != null && defaultBorderImg != null) ? defaultBorderImg : selectBorderImg;
        borderImg.sprite = lit ? litSprite : normalSprite;
    }

    public bool CanAcceptItem(ItemData data)
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isEmpty) return;
        OnSlotClicked?.Invoke(this);
    }
}
