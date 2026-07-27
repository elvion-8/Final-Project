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
    private float hitTime;
    private bool invincibility;     //무적 여부
    private groundCheck groundCheck;
    private bool IsGrounded
    {
        get
        {
            return groundCheck != null && groundCheck.isGrounded && MoveDir.y <= 0f;
        }
    }

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
    private Coroutine attackRoutineCor;
    [HideInInspector] public WeaponHitVFXState currentHitVFXState; // 현재 활성화된 타격 VFX 상태

    // Poton Viow /////////////////////////////////

    PhotonView pv = null;

    Vector3 currPos = Vector3.zero;
    Quaternion currRot = Quaternion.identity;

    //플레이어의 Id를 저장하는 변수
    public int playerId = -1;

    private float syncSpeed;   // 원격 플레이어의 애니메이션 속도를 담을 변수
    private bool syncIsRunning; // 원격 플레이어의 달리기 상태 변수
    private bool isFirstSync = false; // 첫 네트워크 위치 동기화 여부

    public static PlayerCtrl localPlayer;


    /////////////////////////////////////////////////////

    [Space(10)]
    [Header("Wall & Ledge Climb")]
    public LayerMask obstacleLayer;
    public float maxClimbHeight = 2.5f;
    public float wallCheckDistance = 1.0f;
    public float ledgeForwardOffset = 0.5f;
    public float ledgeUpperHeightOffset = 2.2f;

    private bool isWallClimbing = false;
    private bool isLedgeClimbing = false;
    public bool IsClimbing => isWallClimbing || isLedgeClimbing;
    private float climbHeightCounter = 0f;
    private float climbStartHeight = 0f;
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
    int attackLayerIndex;
    int activeWeaponLayerIndex = -1;
    ComboAttack combo;
    AttackMotion attackMotion;
    AttackSound sound;

    CameraMove camera;
    [HideInInspector] public PlayerStatController statController;

    void Awake()

    {
        statController = GetComponent<PlayerStatController>();
        if (statController == null)
        {
            statController = gameObject.AddComponent<PlayerStatController>();
        }
        stat = Managers.Data.stat;
        input = Managers.Input;
        Debug.Log(input.ToString());
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;
        GameObject hpBarObj = GameObject.FindGameObjectWithTag("HpBar");
        if (hpBarObj != null) { hpBar = GameObject.FindGameObjectWithTag("HpBar").GetComponent<Image>(); }
        else return;

        myTr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();

        if (pv.ownerId == null) { playerId = 1001; }
        else playerId = pv.ownerId;

        if (pv.isMine || PhotonNetwork.inRoom == false)   // 자신인 경우 또는 오프라인 테스트인 경우
        {
            localPlayer = this;
            //메인 카메라에 추가된 추적 대상을 연결
            Camera.main.GetComponent<CameraMove>().playerPos = myTr;
        }
        else            // 자신의 네트워크 객체가 아닐때
        {
            Collider col = GetComponent<Collider>();
            if (col != null && !(col is CharacterController))
            {
                col.enabled = false;
            }
        }

        currPos = myTr.position;
        currRot = myTr.rotation;
        combo = GetComponent<ComboAttack>();
        attackMotion = GetComponent<AttackMotion>();
        groundCheck = GetComponentInChildren<groundCheck>();
        camera = CameraMove.Instance;
        sound = GetComponentInChildren<AttackSound>();
    }

    public float MoveSpeedVal => statController != null ? statController.MoveSpeed : 1.3f;
    public float RunSpeedVal => statController != null ? statController.RunSpeed : runningSpeed;
    public float JumpPowerVal => statController != null ? statController.JumpPower : jumpPower;
    public int JumpCountVal => statController != null ? statController.JumpCount : jumpCnt;

    void Start()
    {
        MoveDir = Vector3.zero;
        jumpPower = 9.0f;
        gravity = 20.0f;
        hp = Mathf.RoundToInt(statController != null ? statController.MaxHP : 1000f);
        fullHp = hp;

        if (obstacleLayer.value == 0)
        {
            obstacleLayer = LayerMask.GetMask("Obstacle");
        }
        animD = anim.GetLayerIndex("Hit");
        animDie = anim.GetLayerIndex("Die");
        attackLayerIndex = anim.GetLayerIndex("Attack");
    }


    void SetAnimatorSpeed()
    {
        if (isDie)
        {
            anim.speed = 1.0f;
            return;
        }

        if (isAttacking)
        {
            anim.speed = statController != null ? statController.AttackSpeedMultiplier : 1.0f;
        }
        else if (isWallClimbing || isLedgeClimbing)
        {
            anim.speed = statController != null ? statController.ClimbSpeedMultiplier : 1.0f;
        }
        else
        {
            anim.speed = 1.0f;
        }
    }

    void Update()
    {
        if (pv.isMine || PhotonNetwork.inRoom == false)
        {
            if (isDie) return;

            // 입력
            GetInput();

            // 벽타기
            if (Climb()) { input.ResetKey(); SetAnimatorSpeed(); return; }

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
            anim.SetBool("isGrounded", IsGrounded);
            //ResetInputTriggers();
            input.ResetKey();

            SetAnimatorSpeed();
        }
        else   //내가 아닌 플레이어
        {
            if (!isAttacking && anim != null && anim.applyRootMotion)
            {
                anim.applyRootMotion = false;
            }

            myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 10.0f);
            myTr.rotation = Quaternion.Slerp(myTr.rotation, currRot, Time.deltaTime * 10.0f);

            anim.SetFloat("Speed", syncSpeed);
            anim.SetBool("isGrounded", IsGrounded);

            if (syncSpeed > 0.1f) // 상대방이 움직이고 있다면
            {
                if (syncIsRunning)
                {
                    anim.SetBool("Run", true);
                    anim.SetBool("Walk", false);
                }
                else
                {
                    anim.SetBool("Walk", true);
                    anim.SetBool("Run", false);
                }
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

        if (!IsGrounded && !isRolling && !isAttacking)
        {
            if (v > 0.1f && (input.jumpKey || input.isJumpHeld))
            {
                RaycastHit wallHit;
                Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

                if (Physics.Raycast(rayOrigin, transform.forward, out wallHit, wallCheckDistance, obstacleLayer))
                {
                    float wallAngle = Vector3.Angle(wallHit.normal, Vector3.up);
                    if (wallAngle > 70f && wallAngle < 110f)
                    {
                        transform.rotation = Quaternion.LookRotation(-wallHit.normal);
                        isWallClimbing = true;
                        climbStartHeight = transform.position.y;
                        climbHeightCounter = 0f;
                        MoveDir = Vector3.zero;
                        anim.applyRootMotion = true;
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
            if(attackMotion.GetCurrentWeaponType() <= -1) return;

            // 공격 시 화면(카메라)이 바라보는 방향으로 몸 회전
            Transform cameraTransform = cmTr;
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            if (cameraTransform != null)
            {
                Vector3 lookDir = cameraTransform.forward;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
            

            if (combo != null)
            {
                combo.IsComboAttack();
            }
            else
            {
                if (isAttacking) return;
                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("NetworkAttack", PhotonTargets.All);
                }
                else NetworkAttack();
            }
        }
    }

    [PunRPC]
    public void NetworkAttack(int combo, int weaponType)
    {
        isAttacking = true;
        anim.applyRootMotion = true; // 루트 모션 활성화
        anim.SetInteger("ComboCount", combo);
        anim.SetInteger("WeaponType", weaponType);
        anim.SetTrigger("Attack");
        if (attackRoutineCor != null) StopCoroutine(attackRoutineCor);
        attackRoutineCor = StartCoroutine(AttackRoutine());
    }

    [PunRPC]
    public void NetworkAttack()
    {
        isAttacking = true;
        anim.applyRootMotion = true; // 루트 모션 활성화
        anim.SetTrigger("Attack");
        if (attackRoutineCor != null) StopCoroutine(attackRoutineCor);
        attackRoutineCor = StartCoroutine(AttackRoutine());
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
        if (IsGrounded)
        {
            isMultiJump = false;
            tempJumpCnt = 0;
        }

        // 점프
        if (IsGrounded && !isRolling && input.jumpKey)
        {
            MoveDir.y = JumpPowerVal;

            if (PhotonNetwork.inRoom)
            {
                pv.RPC("NetworkJump", PhotonTargets.All);
            }
            else NetworkJump();

            isMultiJump = true;
        }
        // 추가 점프
        else if (input.jumpKey && !IsGrounded && tempJumpCnt < JumpCountVal && isMultiJump)
        {
            tempJumpCnt++;
            MoveDir.y = JumpPowerVal;

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
        if (isAttacking)
        {
            MoveDir.x = 0;
            MoveDir.z = 0;
            return;
        }

        if (IsGrounded)
        {
            if (!isRolling)
            {
                MoveDir = moveDir.normalized * moveSpeed;
                MoveDir.y = -2f;
                if (inputDir.magnitude > 0.1f)
                {
                    stopTimer = 0f;
                    Vector3 lookDir = new Vector3(MoveDir.x, 0f, MoveDir.z);
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    if (input.runKey)
                    {
                        anim.SetBool("Run", true);
                        moveSpeed = RunSpeedVal;
                    }
                    else
                    {
                        anim.SetBool("Walk", true);
                        moveSpeed = MoveSpeedVal;
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

    void ResetAttackLayer(bool isReset)
    {
        if (isReset)
        {
            if (attackLayerIndex != -1)
            {
                anim.SetLayerWeight(attackLayerIndex, 0f);
            }
            else
            {
                activeWeaponLayerIndex = -1;
                for (int i = 3; i <= 8; i++)
                {
                    if (anim.GetLayerWeight(i) > 0.5f)
                    {
                        activeWeaponLayerIndex = i;
                        anim.SetLayerWeight(i, 0f);
                        break;
                    }
                }
            }
        }
        else
        {
            if (attackLayerIndex != -1)
            {
                anim.SetLayerWeight(attackLayerIndex, 1f);
            }
            else if (activeWeaponLayerIndex != -1)
            {
                anim.SetLayerWeight(activeWeaponLayerIndex, 1f);
            }
        }
    }

    IEnumerator Rolling()
    {
        anim.applyRootMotion = false;
        isAttacking = false;
        isRolling = true;
        ResetAttackLayer(true);
        anim.SetTrigger("Roll");
        gpV.TriggerVib(0.2f, 0.3f, 0.3f);
        gpV.TriggerVib(0.5f, 0.5f, 0.5f);

        Vector3 rollDir;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            rollDir = moveDir.normalized;
            transform.rotation = Quaternion.LookRotation(rollDir);
        }
        else
        {
            rollDir = transform.forward;
        }

        rollDir *= rollPower;
        MoveDir.x = rollDir.x;
        MoveDir.z = rollDir.z;

        yield return new WaitForSecondsRealtime(0.8f);
        isRolling = false;
        ResetAttackLayer(false);
    }


    IEnumerator AttackRoutine()
    {
        // Wait a short duration for animator state to transition in
        yield return new WaitForSeconds(0.15f);

        attackLayerIndex = anim.GetLayerIndex("Attack");
        if (attackLayerIndex == -1)
        {
            for (int i = 1; i < anim.layerCount; i++)
            {
                if (anim.GetLayerWeight(i) > 0.5f)
                {
                    attackLayerIndex = i;
                    break;
                }
            }
            if (attackLayerIndex == -1) attackLayerIndex = 0;
        }

        if (combo != null)
        {
            // Dynamically wait as long as we are executing combo attacks
            while (combo.IsInComboAnimation())
            {
                yield return null;
            }
        }
        else
        {
            AnimatorStateInfo stateInfo;
            if (anim.IsInTransition(attackLayerIndex))
            {
                stateInfo = anim.GetNextAnimatorStateInfo(attackLayerIndex);
            }
            else
            {
                stateInfo = anim.GetCurrentAnimatorStateInfo(attackLayerIndex);
            }

            float duration = stateInfo.length > 0f ? stateInfo.length : 1f;
            yield return new WaitForSeconds(Mathf.Max(0f, duration - 0.15f));
        }

        isAttacking = false;
        anim.applyRootMotion = false;
    }

    public int TakeDamage(int damage)
    {
        hp -= damage;
        hpBar.fillAmount = (float)hp / (float)fullHp;
        Debug.Log("takeDamage : " + damage);
        gpV.TriggerVib(0.3f, 0.3f, 0.5f);

        // Reset combo on hit
        if (combo != null) combo.ResetCombo();
        else isAttacking = false;

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
        if(camera!=null)camera.Shake(0.5f,0.3f,1.5f,true);
        invincibility = false;
    }


    void UpdateWallClimbing(float verticalInput)
    {
        if (verticalInput <= 0.1f)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            anim.applyRootMotion = false;
            return;
        }

        climbHeightCounter = transform.position.y - climbStartHeight;

        if (climbHeightCounter >= maxClimbHeight)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            anim.applyRootMotion = false;
            return;
        }

        RaycastHit chestHit;
        bool hasWallAtChest = Physics.Raycast(transform.position + Vector3.up * 1.0f, transform.forward, out chestHit, wallCheckDistance, obstacleLayer);

        if (!hasWallAtChest)
        {
            isWallClimbing = false;
            anim.SetBool("IsWallClimbing", false);
            anim.applyRootMotion = false;
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

        // 루트 모션 재생 시간 동안 대기합니다. (클라이밍 재생 속도를 반영)
        float climbMult = statController != null ? statController.ClimbSpeedMultiplier : 1.0f;
        float duration = 1.2f / climbMult;
        yield return new WaitForSeconds(duration);

        isLedgeClimbing = false;
        anim.applyRootMotion = false;
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
        Managers.gameOver.OnGameOver();
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
            stream.SendNext(anim != null ? anim.GetBool("Run") : false);
        }
        else
        {
            //언박싱 (아바타들)
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();

            // 3번째: 속도 값
            this.syncSpeed = (float)stream.ReceiveNext();
            // 4번째: 달리기 상태 값
            this.syncIsRunning = (bool)stream.ReceiveNext();

            if (!isFirstSync)
            {
                myTr.position = currPos;
                myTr.rotation = currRot;
                isFirstSync = true;
            }
        }
    }
}
