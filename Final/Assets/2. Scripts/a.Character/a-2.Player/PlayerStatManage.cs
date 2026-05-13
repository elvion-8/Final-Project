using UnityEngine;

public class PlayerStatManage : MonoBehaviour
{
    public scPlayerStat stat = scPlayerStat.stat;

    public void UpgradeHp()
    {
        if (stat.cost >= (1+stat.hpUpgrade)*100)
        {
            stat.cost -= (1+stat.hpUpgrade)*100;
            stat.hpUpgrade += 1;
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }
    public void UpgradeWeaponDmg()
    {
        if (stat.cost >= (1+stat.weaponDmgUpgradeCnt)*100)
        {
            stat.cost -= (1+stat.weaponDmgUpgradeCnt)*100;
            stat.weaponDmgUpgradeCnt += 1;
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }
    public void UpgradeCritProb()
    {
        if (stat.cost >= (1+stat.weaponCritProbUpgradeCnt)*100)
        {
            stat.cost -= (1+stat.weaponCritProbUpgradeCnt)*100;
            stat.weaponCritProbUpgradeCnt += 1;
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }
    public void UpgradeCritDmg()
    {
        if (stat.cost >= (1+stat.weaponCritDmgUpgradeCnt)*100)
        {
            stat.cost -= (1+stat.weaponCritDmgUpgradeCnt)*100;
            stat.weaponCritDmgUpgradeCnt += 1;
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }
    public void UpgradeAttackSpeed()
    {
        if (stat.cost >= (1+stat.weaponAttackSpeed)*100)
        {
            stat.cost -= (1+stat.weaponAttackSpeed)*100;
            stat.weaponAttackSpeed += 1;
        }
        else
        {
            Debug.Log("재화 부족");
        }
    }
}
