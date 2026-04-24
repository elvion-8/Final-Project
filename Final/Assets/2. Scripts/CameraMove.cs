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
    //=====================Lockon 구현
    public bool lockOn;
    private GameObject[] enemys;
    private Transform EnemyTarget;
    private bool isEnemyDetected;
    private Quaternion enemyLookRotation;
    private bool isDie;



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
        StartCoroutine(this.LookSetting());
    }

    IEnumerator EnemyLockOn()
    {
        while (!lockOn)
        {
            StartCoroutine(this.LookSetting());
            // 가장 가까운 에너미를 찾고 걔한테 시선 고정, 상대를 죽이거나 별도 키를 누르기 전까지 다른 곳으로 시선 돌리면 안됨.
        }
        yield return 0;

    }

    IEnumerator LookSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);

            enemys = GameObject.FindGameObjectsWithTag("Enemy");

            // 1. 예외 처리: 맵에 적이 하나도 없을 경우를 대비
            if (enemys.Length == 0)
            {
                EnemyTarget = null;
                isEnemyDetected = false;
                continue; // 아래 코드를 실행하지 않고 다음 루프로 넘어감
            }

            Transform closestEnemy = enemys[0].transform;
            float closestDist = (closestEnemy.position - cmTr.position).sqrMagnitude;

            foreach (GameObject _Enemy in enemys)
            {
                float currentDist = (_Enemy.transform.position - cmTr.position).sqrMagnitude;

                if (currentDist < closestDist)
                {
                    closestEnemy = _Enemy.transform;
                    // 2. 오타 수정: (EnemyTargets.position = cmTr.position) -> currentDist 사용
                    closestDist = currentDist;
                }
            }

            // 3. 탐색 완료 후 전역 변수에 저장
            EnemyTarget = closestEnemy;
            isEnemyDetected = true;
        }
    }

    // IEnumerator LookSetting()
    // {
    //     while (!isDie)
    //     {
    //         yield return new WaitForSeconds(0.2f);
    //         enemys = GameObject.FindGameObjectsWithTag("Enemy");
    //         Transform EnemyTargets = enemys[0].transform;
    //         float dist = (EnemyTargets.position - cmTr.position).sqrMagnitude;
    //         foreach (GameObject _Enemy in enemys)
    //         {
    //             if ((_Enemy.transform.position - cmTr.position).sqrMagnitude < dist)
    //             {
    //                 EnemyTargets = _Enemy.transform;
    //                 dist = (EnemyTargets.position - cmTr.position).sqrMagnitude;
    //             }
    //         }
    //     }
    // }

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
        // 락온 키가 눌렸고, 적이 감지되었으며, 타겟이 존재하고, 죽지 않았을 때
        if (lockOn && isEnemyDetected && EnemyTarget != null && !isDie)
        {
            // 타겟을 향하는 방향 벡터 계산
            Vector3 dir = EnemyTarget.position - cmTr.position;
            dir.y = 0; // (선택사항) Y축을 0으로 하면 캐릭터가 위아래로 기울지 않고 좌우로만 회전합니다.

            if (dir != Vector3.zero) // 방향 벡터가 0이 아닐 때만 회전
            {
                enemyLookRotation = Quaternion.LookRotation(dir);
                // Slerp를 사용하여 부드럽게 타겟을 바라보도록 회전 (10f는 회전 속도)
                cmTr.rotation = Quaternion.Slerp(cmTr.rotation, enemyLookRotation, Time.deltaTime * 10f);
            }
        }
    }

}
