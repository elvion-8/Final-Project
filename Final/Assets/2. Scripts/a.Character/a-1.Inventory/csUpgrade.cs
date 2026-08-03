using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (IsExitKeyPressed())
            {
                ExitUpgrade();
            }
        }
        else
        {
            if (isPlayerInRange && IsInteractKeyPressed())
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

    private bool IsInteractKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.JoystickButton0))
            return true;

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;

        return false;
    }

    private bool IsExitKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1))
            return true;

        if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame))
            return true;

        return false;
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
    // Wrappers proxying to PlayerStatManage to prevent breaking Unity inspector events

    private PlayerStatManage cachedStatManage;

    private PlayerStatManage GetStatManage()
    {
        if (cachedStatManage == null)
        {
            cachedStatManage = FindObjectOfType<PlayerStatManage>();
            if (cachedStatManage == null)
            {
                Debug.LogError("PlayerStatManage could not be found in the scene.");
            }
        }
        return cachedStatManage;
    }

    public void HpUpgrade()
    {
        var sm = GetStatManage();
        if (sm != null) sm.UpgradeHp();
    }

    public void WeaponAttackSpeedUpgrade()
    {
        var sm = GetStatManage();
        if (sm != null) sm.UpgradeAttackSpeed();
    }

    public void WeaponDmgUpgrade()
    {
        var sm = GetStatManage();
        if (sm != null) sm.UpgradeWeaponDmg();
    }

    public void WeaponCritProbUpgrade()
    {
        var sm = GetStatManage();
        if (sm != null) sm.UpgradeCritProb();
    }

    public void WeaponCritDmgUpgrade()
    {
        var sm = GetStatManage();
        if (sm != null) sm.UpgradeCritDmg();
    }
}
