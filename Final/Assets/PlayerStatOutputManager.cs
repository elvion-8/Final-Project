using UnityEngine;
using UnityEngine.UI;

public class PlayerStatOutputManager : MonoBehaviour
{
    public Text hp;
    public Text wDmg;
    public Text wAS;
    public Text wCP;
    public Text wCD;
    public Text cost;

    public int hpN;
    public int wDmgN;
    public int wASN;
    public int wCPN;
    public int wCDN;
    public int costN;
    scPlayerStat stat;

    void Awake()
    {
        stat = Managers.Data.stat;
    }
    void Start()
    {

    }


    void Update()
    {
        hpN = stat.hpUpgrade;
        wDmgN = stat.weaponDmgUpgradeCnt;
        wASN = stat.weaponAttackSpeed;
        wCPN = stat.weaponCritProbUpgradeCnt;
        wCDN = stat.weaponCritDmgUpgradeCnt;
        costN = stat.cost;

        hp.text = hpN.ToString();
        wDmg.text = wDmgN.ToString();
        wAS.text = wASN.ToString();
        wCP.text = wCPN.ToString();
        wCD.text = wCDN.ToString();
        cost.text = "cost : " + costN.ToString();
    }
}
