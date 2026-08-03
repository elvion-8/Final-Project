using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class csMission : MonoBehaviour
{
    public GameObject pnlMission;
    private LobbyPlayerCtrl player;
    Transform mainCameraPos;
    Vector3 tempMainCamPos;
    Quaternion mainCameraRot;
    CameraMove camMove;
    private bool isPlayerInRange = false;
    private bool isMissionPanelOpen = false;
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
        if (isMissionPanelOpen)
        {
            if (IsExitKeyPressed())
            {
                ExitMission();
            }
        }
        else
        {
            if (isPlayerInRange && IsInteractKeyPressed())
            {
                pnlMission.SetActive(true);
                isMissionPanelOpen = true;
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
    void ExitMission()
    {
        pnlMission.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.enabled = true;
        isMissionPanelOpen = false;

        if (camMove != null) camMove.enabled = true;

        if (mainCameraPos != null)
        {
            mainCameraPos.rotation = mainCameraRot;
            mainCameraPos.position = tempMainCamPos;
        }
    }
    public void EasyMode()
    {
        SceneManager.LoadScene("AllTest");
       
    }
}
