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
    public void PlayAttackSFX(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;

        string sfxName = soundName;
        if (sfxName.Equals("spear", System.StringComparison.OrdinalIgnoreCase) || 
            sfxName.Equals("Scythe", System.StringComparison.OrdinalIgnoreCase))
        {
            sfxName = "other1";
        }
        else if (sfxName.Equals("Hammer", System.StringComparison.OrdinalIgnoreCase))
        {
            sfxName = "other2";
        }

        Managers.Sound.PlaySFX(sfxName);
    }
}
