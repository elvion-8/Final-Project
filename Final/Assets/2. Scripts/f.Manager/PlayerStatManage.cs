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

    public void UpgradeHp()
    {
        int costRequired = (1 + stat.hpUpgrade) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            stat.hpUpgrade += 1;
            SaveAndNotify();
            Debug.Log("HP Upgrade Successful!");
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }

    public void UpgradeWeaponDmg()
    {
        int costRequired = (1 + stat.weaponDmgUpgradeCnt) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            stat.weaponDmgUpgradeCnt += 1;
            SaveAndNotify();
            Debug.Log("Weapon Damage Upgrade Successful!");
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }

    public void UpgradeCritProb()
    {
        int costRequired = (1 + stat.weaponCritProbUpgradeCnt) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            stat.weaponCritProbUpgradeCnt += 1;
            SaveAndNotify();
            Debug.Log("Crit Probability Upgrade Successful!");
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }

    public void UpgradeCritDmg()
    {
        int costRequired = (1 + stat.weaponCritDmgUpgradeCnt) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            stat.weaponCritDmgUpgradeCnt += 1;
            SaveAndNotify();
            Debug.Log("Crit Damage Upgrade Successful!");
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }

    public void UpgradeAttackSpeed()
    {
        int costRequired = (1 + stat.weaponAttackSpeed) * 100;
        if (stat.cost >= costRequired)
        {
            stat.cost -= costRequired;
            stat.weaponAttackSpeed += 1;
            SaveAndNotify();
            Debug.Log("Attack Speed Upgrade Successful!");
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }

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
