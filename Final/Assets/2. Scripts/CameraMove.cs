using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour
{
    private Transform cmTr;
    public Transform playerPos;

    [Header("Camera Settings")]
    public float mouseSensitivity;
    public float distance = 7f;
    public float height = 3f;
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;
    private float mouseX;
    private float mouseY;
    private float rotX;
    private float rotY;
    private bool lockOn;
    private GameObject[] enemys;

    void Awake()
    {
        cmTr = GetComponent<Transform>();
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        mouseSensitivity = 2f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) //g키를 누르면 락온
        {
            if (!lockOn)
            {
                lockOn = true;
                LockOnStart();

            }
            else if (lockOn)
            {
                lockOn = false;
            }
        }

    }
    void LockOnStart()
    {
        StartCoroutine(this.EnemyLockOn());
    }

    IEnumerator EnemyLockOn()
    {
        while(!lockOn)
        {
            // 가장 가까운 에너미를 찾고 걔한테 시선 고정, 상대를 죽이거나 별도 키를 누르기 전까지 다른 곳으로 시선 돌리면 안됨.
        }
        yield return 0;

    }

    void LateUpdate()
    {
        MouseMove();
    }

    void MouseMove()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotX += mouseX;
        rotY -= mouseY;
        rotY = Mathf.Clamp(rotY, -20f, 90f);
        Quaternion rotation = Quaternion.Euler(rotY, rotX, 0);
        Vector3 position = playerPos.position - (rotation * Vector3.forward * distance);

        cmTr.rotation = rotation;
        cmTr.position = position;
    }

}
