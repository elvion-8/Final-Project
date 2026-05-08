using UnityEngine;

public enum ItemType
{
    Weapon,
    Buff
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    [TextArea(3, 5)]
    public string itemDesc;
    public Sprite itemIcon;
    public GameObject itemPrefab;
}
