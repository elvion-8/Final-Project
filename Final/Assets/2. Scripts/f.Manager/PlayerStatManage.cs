using UnityEngine;

public class PlayerStatManage : MonoBehaviour
{
    public scPlayerStat stat;
    public bool isMultiPlayer = false;

    void Awake()
    {
        if (isMultiPlayer)
        {
            stat = new scPlayerStat();
        }
        else
        {
            stat = Managers.Data.stat;
        }
    }

    private bool TryUpgradeStat(ref int upgradeCntField, string statName)
    {
        if (stat == null) return false;

        int costRequired = (1 + upgradeCntField) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            upgradeCntField += 1;
            SaveAndNotify();
            Debug.Log($"{statName} Upgrade Successful!");
            return true;
        }

        Debug.Log("재화 부족");
        return false;
    }

    public void UpgradeHp() => TryUpgradeStat(ref stat.hpUpgrade, "HP");
    public void UpgradeWeaponDmg() => TryUpgradeStat(ref stat.weaponDmgUpgradeCnt, "Weapon Damage");
    public void UpgradeCritProb() => TryUpgradeStat(ref stat.weaponCritProbUpgradeCnt, "Crit Probability");
    public void UpgradeCritDmg() => TryUpgradeStat(ref stat.weaponCritDmgUpgradeCnt, "Crit Damage");
    public void UpgradeAttackSpeed() => TryUpgradeStat(ref stat.weaponAttackSpeed, "Attack Speed");

    private void SaveAndNotify()
    {
        if (!isMultiPlayer && Managers.Data != null)
        {
            Managers.Data.SaveGame();
        }

        // Notify active PlayerStatController to reload permanent stats
        PlayerStatController[] activeControllers = FindObjectsOfType<PlayerStatController>();
        foreach (var controller in activeControllers)
        {
            controller.LoadPermanentStats();
        }
        PlayerStatController.OnStatsChanged?.Invoke();
    }
}
