using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class csUpgrade : MonoBehaviour
{
    public GameObject pnlUpgrade;
    private LobbyPlayerCtrl player;
    Transform mainCameraPos;
    Vector3 tempMainCamPos;
    Quaternion mainCameraRot;
    CameraMove camMove;
    private bool isPlayerInRange = false;
    private bool isUpgradePanelOpen = false;
    Animator anim;
    scPlayerStat pS;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<LobbyPlayerCtrl>();
        mainCameraPos = GameObject.FindWithTag("MainCamera").transform;
        camMove = mainCameraPos.GetComponent<CameraMove>();
        anim = player.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isUpgradePanelOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                ExitUpgrade();
            }
        }
        else
        {
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.X))
            {
                pnlUpgrade.SetActive(true);
                isUpgradePanelOpen = true;
                anim.SetBool("Run", false);
                anim.SetBool("Walk", false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                player.enabled = false;

                if (camMove != null) camMove.enabled = false;

                tempMainCamPos = mainCameraPos.position;
                mainCameraRot = mainCameraPos.rotation;

                mainCameraPos.LookAt(player.transform.position + Vector3.up * 1.5f);
                mainCameraPos.Rotate(0, -15f, 0, Space.Self);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void ExitUpgrade()
    {
        pnlUpgrade.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.enabled = true;
        isUpgradePanelOpen = false;

        if (camMove != null)
        {
            camMove.enabled = true;
        }

        if (mainCameraPos != null)
        {
            mainCameraPos.rotation = mainCameraRot;
            mainCameraPos.position = tempMainCamPos;
        }
    }
    //===========================================================Player, Weapon Upgrade Button Punc
    public void HpUpgrade()
    {
        int price = 0 + (pS.hpUpgrade);
        if (pS.cost > price)
        {
            pS.cost -= 1;
            pS.hpUpgrade++;
        }
        else Debug.Log("not enough cost!");
    }

    public void WeaponAttackSpeedUpgrade()
    {
        int price = 0 + (pS.weaponAttackSpeed);
        if (pS.cost > price)
        {
            pS.cost -= 1;
            pS.weaponAttackSpeed++;
        }
        else Debug.Log("not enough cost!");
    }

    public void WeaponDmgUpgrade()
    {
        int price = 0 + (pS.weaponDmgUpgradeCnt);
        if (pS.cost > price)
        {
            pS.cost -= 1;
            pS.weaponDmgUpgradeCnt++;
        }
        else Debug.Log("not enough cost!");
    }

    public void WeaponCritProbUpgrade()
    {
        int price = 0 + (pS.weaponCritProbUpgradeCnt);
        if (pS.cost > price)
        {
            pS.cost -= 1;
            pS.weaponCritProbUpgradeCnt++;
        }
        else Debug.Log("not enough cost!");
    }
    public void WeaponCritDmgUpgrade()
    {
        int price = 0 + (pS.weaponCritDmgUpgradeCnt);
        if (pS.cost > price)
        {
            pS.cost -= 1;
            pS.weaponCritDmgUpgradeCnt++;
        }
        else Debug.Log("not enough cost!");
    }
}
