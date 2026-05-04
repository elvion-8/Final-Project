using UnityEngine;

public class csUpgrade : MonoBehaviour
{
    public GameObject pnlUpgrade;
    private PlayerCtrl player;
    Transform mainCameraPos;
    CameraMove camMove;
    private bool isPlayerInRange = false;
    private bool isUpgradePanelOpen = false;
    Animator anim;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>();
        mainCameraPos = GameObject.FindWithTag("MainCamera").transform;
        camMove = mainCameraPos.GetComponent<CameraMove>(); // 스크립트 찾아오기
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
    }
}
