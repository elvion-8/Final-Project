using UnityEngine;

public class AttackSound : MonoBehaviour
{
    public ItemData itemData;
    public void ChangeWeapon(ItemData newData)
    {
        itemData = newData;
    }
    public void PlayAttackSFX()
    {
        if(itemData != null)
        {
            Managers.Sound.PlaySFX(itemData.attackSoundName);
        }
    }
}
