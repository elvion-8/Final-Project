using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    public static CameraMove Instance { get; private set; }

    private float shakeDuration;
    private float initialShakeDuration;
    private float shakeMagnitude;
    private float shakeFrequency;
    private bool shouldFadeOut;
    private Vector3 currentShakeOffset;
    private float currentRollOffset;

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

    private Vector2 moveInput;
    private bool isLockOnTrigger;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        cmTr = GetComponent<Transform>();
        if (playerPos == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerPos = player.transform;
        }
        currentDistance = distance;
    }

    void Start()
    {
        if (mouseSensitivity <= 0f) mouseSensitivity = 2.0f;

        Vector3 angles = transform.eulerAngles;
        rotX = angles.y;
        rotY = angles.x;

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
        //LockOn();
    }
    #region CallBack Input
    public void OnScreenMove(InputValue value){moveInput = value.Get<Vector2>();}
    public void OnLockOn(InputValue value)
    {
        if(value.isPressed)
        {
            lockOn = !lockOn;
            if(lockOn)
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
    #endregion

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
        if (playerPos == null) 
        {
            return;
        }

        if (!lockOn) { MouseMove(); }
        else { LockOn(); }

        ApplyShake();
    }

    void MouseMove()
    {
        mouseX = moveInput.x * mouseSensitivity*Time.deltaTime;
        mouseY = moveInput.y * mouseSensitivity*Time.deltaTime;
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

    #region Camera Shake Implementation
    public void Shake(float duration, float magnitude, float frequency = 1f, bool fadeOut = true)
    {
        shakeDuration = duration;
        initialShakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeFrequency = frequency;
        shouldFadeOut = fadeOut;
        
        currentShakeOffset = Vector3.zero;
        currentRollOffset = 0f;
    }

    public void ShakeAtPosition(Vector3 epicenter, float maxDistance, float duration, float maxMagnitude, float frequency = 1f, bool fadeOut = true)
    {
        if (playerPos == null) return;
        
        float distanceToPlayer = Vector3.Distance(epicenter, playerPos.position);
        if (distanceToPlayer < maxDistance)
        {
            float distanceFactor = 1f - (distanceToPlayer / maxDistance);
            float calculatedMagnitude = maxMagnitude * distanceFactor;
            Shake(duration, calculatedMagnitude, frequency, fadeOut);
        }
    }

    private void ApplyShake()
    {
        if (shakeDuration > 0)
        {
            shakeDuration -= Time.deltaTime;

            float currentMag = shakeMagnitude;
            if (shouldFadeOut && initialShakeDuration > 0f)
            {
                currentMag = Mathf.Lerp(0f, shakeMagnitude, shakeDuration / initialShakeDuration);
            }

            Vector3 randomPoint = UnityEngine.Random.insideUnitSphere * currentMag;
            
            currentShakeOffset = (transform.right * randomPoint.x) + (transform.up * randomPoint.y);

            currentRollOffset = UnityEngine.Random.Range(-1f, 1f) * currentMag * 15f;

            transform.position += currentShakeOffset;
            transform.Rotate(0, 0, currentRollOffset);
        }
    }
    #endregion
}