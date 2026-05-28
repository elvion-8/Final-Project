using UnityEngine;
using System.Collections;
using System;

public class CameraMove : MonoBehaviour
{
    private Transform cmTr;
    public Transform playerPos;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2.0f;
    public float distance = 7f;
    [Range(-3f,10f)]
    [Tooltip("카메라가 바라보는 플레이어의 높이 오프셋")]
    public float cameraHeight = 1.5f;
    [Tooltip("락온 시 높이 보정")]
    public float height = 1.5f;

    [Header("Collision Settings (벽 뚫기 방지)")]
    [Tooltip("카메라가 통과하지 못하는 장애물 레이어")]
    public LayerMask obstacleLayers;
    [Tooltip("카메라 충돌체 반지름")]
    public float cameraRadius = 0.3f;
    [Tooltip("카메라 기존위치 복귀 속도")]
    public float smoothSpeed = 10f;

    private float mouseX;
    private float mouseY;
    private float rotX;
    private float rotY;
    private float currentDistance;

    //=====================Lockon 구현
    public bool lockOn;
    private GameObject[] enemys;
    private Transform EnemyTarget;
    private bool isEnemyDetected;
    private bool isDie;
    private float hf;

    void Awake()
    {
        cmTr = GetComponent<Transform>();
<<<<<<< HEAD
        if (playerPos == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerPos = player.transform;
        }
        currentDistance = distance;
=======
>>>>>>> 651f1686a12820bebca92b25f32805a9a253a36c
    }

    void Start()
    {
<<<<<<< HEAD
        if (mouseSensitivity <= 0f) mouseSensitivity = 2.0f;

        Vector3 angles = transform.eulerAngles;
        rotX = angles.y;
        rotY = angles.x;

=======
        StartCoroutine(FindPlayerTarget());
        mouseSensitivity = 2.0f;
>>>>>>> 651f1686a12820bebca92b25f32805a9a253a36c
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    IEnumerator FindPlayerTarget()
    {
        while (playerPos == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                // 찾은 플레이어의 Transform을 할당
                playerPos = player.transform;
            }
            yield return new WaitForSeconds(0.2f); // 0.2초마다 확인
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            lockOn = !lockOn;
            if (lockOn) 
            {
                LockOnStart();
            }
            else
            {
                isEnemyDetected = false;
                EnemyTarget = null;
            }
        }
    }

    void LockOnStart()
    {
        StartCoroutine(LookSetting());
    }

    IEnumerator LookSetting()
    {
        while (lockOn && !isDie)
        {
            enemys = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemys.Length == 0)
            {
                EnemyTarget = null;
                isEnemyDetected = false;
                lockOn = false;
                yield break;
            }

            Transform closestEnemy = enemys[0].transform;
            float closestDist = (closestEnemy.position - playerPos.position).sqrMagnitude;

            foreach (GameObject _Enemy in enemys)
            {
                float currentDist = (_Enemy.transform.position - playerPos.position).sqrMagnitude;

                if (currentDist < closestDist)
                {
                    closestEnemy = _Enemy.transform;
                    closestDist = currentDist;
                }
            }

            EnemyTarget = closestEnemy;
            isEnemyDetected = true;
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    void LateUpdate()
    {
<<<<<<< HEAD
        if (playerPos == null) 
=======
        if (playerPos == null)
>>>>>>> 651f1686a12820bebca92b25f32805a9a253a36c
        {
            return;
        }

<<<<<<< HEAD
        if (!lockOn) { MouseMove(); }
        else { LockOn(); }
=======
        if (!lockOn){MouseMove();}
        if(lockOn){LockOn();}
>>>>>>> 651f1686a12820bebca92b25f32805a9a253a36c
    }

    void MouseMove()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotX += mouseX;
        rotY -= mouseY;
        
        rotY = Mathf.Clamp(rotY, -20f, 75f); 

        Quaternion rotation = Quaternion.Euler(rotY, rotX, 0);

        Vector3 targetPos = playerPos.position + (Vector3.up * cameraHeight);
        Vector3 direction = -(rotation * Vector3.forward);
        
        float targetDistance = distance;
        RaycastHit hit;

        if (Physics.SphereCast(targetPos, cameraRadius, direction, out hit, distance, obstacleLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, 0.1f, distance);
        }

        if (targetDistance < currentDistance)
            currentDistance = targetDistance;
        else
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        Vector3 finalPosition = targetPos + (direction * currentDistance);

        cmTr.rotation = rotation;
        cmTr.position = finalPosition;
    }

    void LockOn()
    {
        if (lockOn && isEnemyDetected && EnemyTarget != null && !isDie)
        {
            Vector3 targetPos = playerPos.position + (Vector3.up * cameraHeight);
            
            Vector3 dirFrom = (playerPos.position - EnemyTarget.position).normalized;
            hf = (float)Math.Round(dirFrom.y / 10 + 0.4f, 1);
            dirFrom.y = hf;

            Vector3 desiredPosition = playerPos.position + (dirFrom * height) + (dirFrom * distance);
            Vector3 direction = (desiredPosition - targetPos).normalized;
            float desiredDist = Vector3.Distance(targetPos, desiredPosition);

            float targetDistance = desiredDist;
            RaycastHit hit;

            if (Physics.SphereCast(targetPos, cameraRadius, direction, out hit, desiredDist, obstacleLayers))
            {
                targetDistance = hit.distance;
            }

            if (targetDistance < currentDistance)
                currentDistance = targetDistance;
            else
                currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

            Vector3 finalPosition = targetPos + (direction * currentDistance);
            
            transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * 10f);

            Vector3 lookDir = EnemyTarget.position - transform.position;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
}