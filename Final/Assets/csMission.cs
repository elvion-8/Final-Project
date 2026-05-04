using UnityEngine;
using UnityEngine.SceneManagement;

public class csMission : MonoBehaviour
{
    public GameObject pnlMission;
    private PlayerCtrl player;
    Transform mainCameraPos;
    Quaternion mainCameraRot;
    private bool isPlayerInRange = false;
    private bool isMissionPanelOpen = false;
    Animator anim;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>();
        mainCameraPos = GameObject.FindWithTag("MainCamera").transform;
        anim = player.GetComponentInChildren<Animator>();
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            ExitMission();
        }

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.X) && isMissionPanelOpen == false)
        {
            pnlMission.SetActive(true);
            isMissionPanelOpen = true;
            anim.SetBool("Run", false);
            anim.SetBool("Walk", false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.enabled = false;
            mainCameraPos.position = mainCameraPos.position;
            mainCameraPos.position = mainCameraPos.position; // (이 줄은 의미가 없지만 기존 코드 유지)
            mainCameraRot = mainCameraPos.rotation;

            // 1. 플레이어를 쳐다보게 세팅
            mainCameraPos.LookAt(player.transform.position + Vector3.up * 1.5f);

            // 2. 카메라를 왼쪽으로 회전시켜 플레이어를 화면 우측으로 배치
            mainCameraPos.Rotate(0, -15f, 0, Space.Self);
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
    void ExitMission()
    {
        pnlMission.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.enabled = true;
        isMissionPanelOpen = false;

        if (mainCameraPos != null)
        {
            mainCameraPos.rotation = mainCameraRot;
        }
    }
    public void EasyMode()
    {
        SceneManager.LoadScene("AllTest");
    }
}
