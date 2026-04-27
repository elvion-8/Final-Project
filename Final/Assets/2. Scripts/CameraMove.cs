using UnityEngine;
using System.Collections;
using System;

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
    //=====================Lockon 구현
    public bool lockOn;
    private GameObject[] enemys;
    private Transform EnemyTarget;
    private bool isEnemyDetected;
    private Quaternion enemyLookRotation;
    private bool isDie;
    private float hf;



    void Awake()
    {
        cmTr = GetComponent<Transform>();
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Start()
    {
        mouseSensitivity = 2.0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
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
        StartCoroutine(this.LookSetting());
    }

    IEnumerator EnemyLockOn()
    {
        while (!lockOn)
        {
            StartCoroutine(this.LookSetting());
        }
        yield return 0;

    }

    IEnumerator LookSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);

            enemys = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemys.Length == 0)
            {
                EnemyTarget = null;
                isEnemyDetected = false;
                continue;
            }

            Transform closestEnemy = enemys[0].transform;
            float closestDist = (closestEnemy.position - cmTr.position).sqrMagnitude;

            foreach (GameObject _Enemy in enemys)
            {
                float currentDist = (_Enemy.transform.position - cmTr.position).sqrMagnitude;

                if (currentDist < closestDist)
                {
                    closestEnemy = _Enemy.transform;
                    closestDist = currentDist;
                }
            }

            EnemyTarget = closestEnemy;
            isEnemyDetected = true;
        }
    }
    void LateUpdate()
    {
        if(!lockOn){MouseMove();}
        if(lockOn){LockOn();}
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

    void LockOn()
    {
        if (lockOn && isEnemyDetected && EnemyTarget != null && !isDie)
        {
            Vector3 dirFrom = (playerPos.position - EnemyTarget.position).normalized;

            hf = (float)Math.Round(dirFrom.y / 10 + 0.4f,1);

            dirFrom.y = hf;

            Vector3 dir = playerPos.position + (dirFrom * height) + (dirFrom * distance);

            transform.position = Vector3.Lerp(transform.position, dir, Time.deltaTime * 10f);

            Vector3 lookDir = EnemyTarget.position - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(lookDir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
