using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Buff
}

public enum WeaponType
{
    Dagger,Sword,Axe,Spear,Scythe,Hammer
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;   //아이템 유형
    public WeaponType weaponType; //무기 유형(ItemType가 Weapon일 경우)
    public string itemName;     //아이템 이름
    [TextArea(3, 5)]
    public string itemDesc;     //아이템 설명
    public Sprite itemIcon;     //아이템 icon
    public Sprite iconLit;      //선택 시 출력 될 icon 이미지
    public GameObject itemPrefab;   //슬롯이 참조할 아이템 프리펩
    public string attackSoundName;

    [Header("Combo Settings")]
    public int maxCombo = 3;
    public float comboTime = 0.8f;

    [Header("Item Effects (Buff/Special)")]
    public List<ItemEffect> effects = new List<ItemEffect>();

    public void OnAcquire(PlayerCtrl player)
    {
        if (effects == null) return;
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.OnApply(player);
            }
        }
    }

    public void OnLost(PlayerCtrl player)
    {
        if (effects == null) return;
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.OnRemove(player);
            }
        }
    }
}
