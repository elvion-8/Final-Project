using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {

    }
    void Start()
    {

    }


    void Update()
    {
        hpN = scPlayerStat.stat.hpUpgrade;
        wDmgN = scPlayerStat.stat.weaponDmgUpgradeCnt;
        wASN = scPlayerStat.stat.weaponAttackSpeed;
        wCPN = scPlayerStat.stat.weaponCritProbUpgradeCnt;
        wCDN = scPlayerStat.stat.weaponCritDmgUpgradeCnt;
        costN = scPlayerStat.stat.cost;

        hp.text = hpN.ToString();
        wDmg.text = wDmgN.ToString();
        wAS.text = wASN.ToString();
        wCP.text = wCPN.ToString();
        wCD.text = wCDN.ToString();
        cost.text = "cost : " + costN.ToString();
    }
}
