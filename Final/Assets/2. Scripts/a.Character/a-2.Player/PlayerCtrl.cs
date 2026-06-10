using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour, ITakeDamage
{
    [Header("Player Move")]
    [Range(1f, 5f)]
    [Tooltip("기본값 : 1.3")]
    private float moveSpeed;
    [Range(1f, 20f)]
    [Tooltip("기본값 : 10")]
    public float rotationSpeed = 10.0f;
    [Range(1f, 10f)]
    [Tooltip("기본값 : 6")]
    public float runningSpeed = 6f;
    [Range(1f, 10f)]
    [Tooltip("기본값 : 7")]
    public float rollPower;
    [Range(1f, 10f)]
    public float attackSpeed = 1;
    public int jumpCnt = 0;
    private int tempJumpCnt = 0;
    [Header("피격 시 무적 시간")]
    [SerializeField] private float hitTime;
    private bool invincibility;     //무적 여부

    [Space(10)]
    [Header("playerStat")]
    [Range(1f, 1000f)]
    public int hp;
    private int fullHp;
    public Image hpBar;

    scPlayerStat stat;
    InputManager input;

    [Space(10)]
    private CharacterController charCon;
    private Transform cmTr;
    private Transform myTr;

    private float jumpPower;
    [Range(1f, 20f)]
    [Tooltip("기본값 : 6.5")]
    public float jumpForwardForce;
    private float gravity;
    [Header("others")]
    public Animator anim;
    public Vector3 MoveDir;
    private bool isDie;
    public bool isAttacking = false;        //무기들에서 공격 여부를 참조하기 때문에 퍼블릭


    private bool isRolling;

    // Poton Viow /////////////////////////////////

    PhotonView pv = null;

    Vector3 currPos = Vector3.zero;
    Quaternion currRot = Quaternion.identity;

    //플레이어의 Id를 저장하는 변수
    public int playerId = -1;

    private float syncSpeed;   // 원격 플레이어의 애니메이션 속도를 담을 변수


    /////////////////////////////////////////////////////

    [Space(10)]
    [Header("Wall & Ledge Climb")]
    public LayerMask obstacleLayer;
    public float maxClimbHeight = 2.5f;
    public float climbSpeed = 2.0f;
    public float wallCheckDistance = 1.0f;
    public float ledgeForwardOffset = 0.5f;
    public float ledgeUpperHeightOffset = 2.2f;

    private bool isWallClimbing = false;
    private bool isLedgeClimbing = false;
    private float climbHeightCounter = 0f;
    private bool isMultiJump = false;

    //입력용 변수
    private float h;
    private float v;
    private Vector3 inputDir;
    private Vector3 moveDir;
    private Vector3 camForward;
    private Vector3 camRight;
    float stopTimer = 0f;
    float cancelDelay = 0.02f;

    [SerializeField] private csGamePadVibMng gpV;
    int animD;
    int animDie;

    void Awake()

    {
        stat = Managers.Data.stat;
        input = Managers.Input;
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;
        GameObject hpBarObj = GameObject.FindGameObjectWithTag("HpBar");
        if (hpBarObj != null) { hpBar = GameObject.FindGameObjectWithTag("HpBar").GetComponent<Image>(); }
        else return;

        myTr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();


        //pv.ObservedComponents[0] = this;

        //pv.synchronization = ViewSynchronization.UnreliableOnChange;
        if (pv.ownerId == null) { playerId = 1001; }
        else playerId = pv.ownerId;

        if (pv.isMine)   // 자신인 경우
        {
            //메인 카메라에 추가된 추적 대상을 연결
            Camera.main.GetComponent<CameraMove>().playerPos = myTr;
        }
        else if(PhotonNetwork.inRoom == false)
        {
            Camera.main.GetComponent<CameraMove>().playerPos = myTr;
        }
        else            // 자신의 네트워크 객체가 아닐때
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        currPos = myTr.position;
        currRot = myTr.rotation;

    }

    void Start()
    {
        MoveDir = Vector3.zero;
        jumpPower = 9.0f;
        gravity = 20.0f;
        hp += stat.hpUpgrade * 50;
        fullHp = hp;

        if (obstacleLayer.value == 0)
        {
            obstacleLayer = LayerMask.GetMask("Obstacle");
        }
        animD = anim.GetLayerIndex("Hit");
        animDie = anim.GetLayerIndex("Die");
    }


    void Update()
    {
        if (pv.isMine || PhotonNetwork.inRoom == false)
        {
            if (isDie) return;

            // 입력
            GetInput();

            // 벽타기
            if (Climb()) {input.ResetKey();return;}

            // 공격
            Attack();
            // 구르기
            Roll();

            // 이동
            Movement();
            // 점프
            Jump();

            // 중력 및 이동 값 적용
            ExecuteMove();

            anim.SetFloat("Speed", inputDir.magnitude);
            //ResetInputTriggers();
            input.ResetKey();
        }
        else   //내가 아닌 플레이어
        {
            myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 3.0f);
            myTr.rotation = Quaternion.Slerp(myTr.rotation, currRot, Time.deltaTime * 3.0f);

            anim.SetFloat("Speed", syncSpeed);

            if (syncSpeed > 0.1f) // 상대방이 움직이고 있다면
            {
                anim.SetBool("Walk", true);
            }
            else // 상대방이 멈췄다면
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Run", false);
            }
        }

    }

    //입력
    void GetInput()
    {
        h = input.move.x;
        v = input.move.y;
        inputDir = new Vector3(h, 0, v);

        if (cmTr != null)
        {
            camForward = cmTr.forward;
            camRight = cmTr.right;
            camForward.y = 0;
            camRight.y = 0;
            moveDir = camForward * v + camRight * h;
        }
    }

    [PunRPC]
    // 벽타기
    bool Climb()
    {
        if (isLedgeClimbing) return true;

        if (isWallClimbing)
        {
            UpdateWallClimbing(v);
            return true;
        }

        if (!charCon.isGrounded && !isRolling && !isAttacking)
        {
            if (v > 0.1f && (input.jumpKey/*isJumpTriggered || isJumpHeld*/))
            {
                RaycastHit wallHit;
                Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
                Debug.DrawRay(rayOrigin, transform.forward * wallCheckDistance, Color.red, 2f);

                if (Physics.Raycast(rayOrigin, transform.forward, out wallHit, wallCheckDistance, obstacleLayer))
                {
                    float wallAngle = Vector3.Angle(wallHit.normal, Vector3.up);
                    if (wallAngle > 70f && wallAngle < 110f)
                    {
                        transform.rotation = Quaternion.LookRotation(-wallHit.normal);
                        isWallClimbing = true;
                        climbHeightCounter = 0f;
                        MoveDir = Vector3.zero;
                        anim.SetBool("IsWallClimbing", true);
                        anim.SetBool("Walk", false);
                        anim.SetBool("Run", false);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // 공격

    void Attack()
    {
        if (input.attackKey)
        {

            if (!isAttacking) 
            {
                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("NetworkAttack", PhotonTargets.All);
                }
                else NetworkAttack();
            }
            
        }
    }

    [PunRPC]
    void NetworkAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack");
        StartCoroutine(AttackRoutine());
    }

    // 구르기
    void Roll()
    {
        if (input.rollingKey && !isRolling)
        {
            if (PhotonNetwork.inRoom)
            {
                pv.RPC("NetworkRoll", PhotonTargets.All);
            }
            else NetworkRoll();
        }
    }

    [PunRPC]
    void NetworkRoll()
    {
        StartCoroutine(Rolling());
    }

    // 점프

    void Jump()
    {
        if (charCon.isGrounded)
        {
            input.isJumpHeld = false;
            isMultiJump = false;
            tempJumpCnt = 0;
        }

        // 점프
        if (charCon.isGrounded && !isRolling && input.jumpKey)
        {
            MoveDir.y = jumpPower;

            if (PhotonNetwork.inRoom)
            {
                pv.RPC("NetworkJump", PhotonTargets.All);
            }
            else NetworkJump();

            isMultiJump = true;
        }
        // 추가 점프
        else if (input.jumpKey && !charCon.isGrounded && tempJumpCnt < jumpCnt && isMultiJump)
        {
            tempJumpCnt++;
            MoveDir.y = jumpPower;

            Vector3 jumpDir = transform.forward.normalized;
            MoveDir.x = jumpDir.x * jumpForwardForce;
            MoveDir.z = jumpDir.z * jumpForwardForce;

            if (jumpDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(jumpDir), rotationSpeed * Time.deltaTime);
            }
            if (PhotonNetwork.inRoom)
            {
                pv.RPC("NetworkJump", PhotonTargets.All);
            }
            else NetworkJump();
        }
    }

    [PunRPC]
    void NetworkJump()
    {
        anim.SetTrigger("Jump");
        gpV.TriggerVib(0.2f, 0.1f, 0.1f);
    }

    // 이동
    void Movement()
    {
        if (charCon.isGrounded)
        {
            if (!isRolling)
            {
                MoveDir = moveDir.normalized * moveSpeed;
                if (inputDir.magnitude > 0.1f)
                {
                    stopTimer = 0f;
                    Quaternion targetRotation = Quaternion.LookRotation(MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    if (input.runKey)
                    {
                        anim.SetBool("Run", true);
                        moveSpeed = runningSpeed;
                    }
                    else
                    {
                        anim.SetBool("Walk", true);
                        moveSpeed = 1.3f;
                        anim.SetBool("Run", false);
                    }
                }
                else
                {
                    stopTimer += Time.deltaTime;
                    if (stopTimer >= cancelDelay)
                    {
                        anim.SetBool("Walk", false);
                        anim.SetBool("RightSide", false);
                        anim.SetBool("LeftSide", false);
                        anim.SetBool("Run", false);
                        input.runKey = false;
                    }
                }
            }
        }
        else
        {
            if (!isWallClimbing && !isLedgeClimbing)
            {
                if (inputDir.magnitude > 0.1f)
                {
                    Vector3 lookDir = camForward * v + camRight * h;
                    if (lookDir.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }
            }
        }
    }

    // 중력 적용, 캐릭터 실제 이동
    void ExecuteMove()
    {
        MoveDir.y -= gravity * Time.deltaTime;
        charCon.Move(MoveDir * Time.deltaTime);
    }

    
    IEnumerator Rolling()
    {
        isRolling = true;
        anim.SetTrigger("Roll");
        gpV.TriggerVib(0.2f, 0.3f, 0.3f);
        gpV.TriggerVib(0.5f, 0.5f, 0.5f);
        Vector3 rollDir = transform.forward * rollPower;
        MoveDir.x = rollDir.x;
        MoveDir.z = rollDir.z;
        yield return new WaitForSecondsRealtime(0.8f);
        isRolling = false;
    }


    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    public int TakeDamage(int damage)
    {
        hp -= damage;
        hpBar.fillAmount = (float)hp / (float)fullHp;
        Debug.Log("takeDamage : " + damage);
        gpV.TriggerVib(0.3f, 0.3f, 0.5f);
        StartCoroutine(Hit());
        if (hp <= 0)
        {
            Die();
        }
        return damage;
    }
    IEnumerator Hit()
    {
        invincibility = true;
        anim.SetTrigger("Hit");
        anim.SetLayerWeight(animD, 1f);
        yield return new WaitForSeconds(hitTime);
        anim.SetLayerWeight(animD, 0f);
        Debug.Log(animD);
        Debug.Log("i'm hit!");
        invincibility = false;
    }

    
    void UpdateWallClimbing(float verticalInput)
    {
        if (verticalInput <= 0.1f)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            return;
        }

        MoveDir = Vector3.up * climbSpeed;
        climbHeightCounter += climbSpeed * Time.deltaTime;

        if (climbHeightCounter >= maxClimbHeight)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            MoveDir = -transform.forward * 1.5f;
            return;
        }

        charCon.Move(MoveDir * Time.deltaTime);

        RaycastHit chestHit;
        bool hasWallAtChest = Physics.Raycast(transform.position + Vector3.up * 1.0f, transform.forward, out chestHit, wallCheckDistance, obstacleLayer);

        if (!hasWallAtChest)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            return;
        }

        RaycastHit headHit;
        bool hasWallAtHead = Physics.Raycast(transform.position + Vector3.up * ledgeUpperHeightOffset, transform.forward, out headHit, wallCheckDistance, obstacleLayer);

        if (!hasWallAtHead)
        {
            Vector3 ledgeCheckStart = chestHit.point - chestHit.normal * 0.15f;
            ledgeCheckStart.y = transform.position.y + ledgeUpperHeightOffset + 0.5f;
            Debug.DrawRay(ledgeCheckStart, Vector3.down * (ledgeUpperHeightOffset + 1.0f), Color.yellow, 2f);

            RaycastHit ledgeHit;
            if (Physics.Raycast(ledgeCheckStart, Vector3.down, out ledgeHit, ledgeUpperHeightOffset + 1.0f, obstacleLayer))
            {
                // 착지 시 캐릭터가 벽 안쪽으로 충분히 진입하도록 위치 조정
                Vector3 landingPoint = ledgeHit.point - chestHit.normal * 0.25f;
                StartCoroutine(LedgeClimbRoutine(landingPoint, chestHit.normal));
            }
        }
    }

    
    IEnumerator LedgeClimbRoutine(Vector3 ledgeSurfacePoint, Vector3 wallNormal)
    {
        isLedgeClimbing = true;
        isWallClimbing = false;
        anim.SetBool("IsWallClimbing", false);
        anim.SetTrigger("LedgeClimb");

        charCon.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 targetPos = ledgeSurfacePoint;
        targetPos.y = ledgeSurfacePoint.y + 0.05f;

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            if (percent < 0.5f)
            {
                // 1단계: 수직 위로 상승
                float t = percent / 0.5f;
                Vector3 intermediatePos = new Vector3(startPos.x, targetPos.y, startPos.z);
                transform.position = Vector3.Lerp(startPos, intermediatePos, t);
            }
            else
            {
                // 2단계: 착지점으로 전진
                float t = (percent - 0.5f) / 0.5f;
                Vector3 intermediatePos = new Vector3(startPos.x, targetPos.y, startPos.z);
                transform.position = Vector3.Lerp(intermediatePos, targetPos, t);
            }

            yield return null;
        }

        transform.position = targetPos;
        charCon.enabled = true;
        isLedgeClimbing = false;

        MoveDir = Vector3.zero;
    }

    
    void Die()
    {
        if (PhotonNetwork.inRoom)
        {
            pv.RPC("NetworkDie", PhotonTargets.All);
        }
        else NetworkDie();
    }

    [PunRPC]
    void NetworkDie()
    {
        charCon.enabled = false;
        anim.SetTrigger("Die");
        anim.SetLayerWeight(animDie, 1f);
        Debug.Log("you die");

    }

    // 포톤 추가
    // 네트워크 객체 생성 완료시 자동 호출되는 함수
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //info.sender.TagObject = this.GameObject;
        // 네트워크 플레이어 생성시 전달 인자 확인
        object[] data = pv.instantiationData;
        Debug.Log((int)data[0]);
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보를 송신
        if (stream.isWriting)
        {
            //박싱 (로컬플레이어)
            stream.SendNext(myTr.position);
            stream.SendNext(myTr.rotation);

            float mySpeed = inputDir.magnitude;
            stream.SendNext(mySpeed);
        }
        else
        {
            //언박싱 (아바타들)
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();

            // [추가] 3번째 데이터 순서에 맞춰 속도 값을 수신받아 변수에 저장합니다.
            this.syncSpeed = (float)stream.ReceiveNext();
        }
    }
}
