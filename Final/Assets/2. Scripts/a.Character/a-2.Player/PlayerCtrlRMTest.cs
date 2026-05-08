using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PlayerCtrlRMTest : MonoBehaviour, ITakeDamage
{
    [Header("Player Move (Blend Tree 0~1)")]
    [Range(0f, 1f)]
    [Tooltip("블렌드 트리 걷기 파라미터 (기본: 0.5)")]
    public float walkBlendValue = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("블렌드 트리 달리기 파라미터 (기본: 1.0)")]
    public float runBlendValue = 1.0f;
    [Tooltip("애니메이션 전환 부드러움 정도 (낮을수록 즉각 반응)")]
    public float speedDampTime = 0.1f;

    [Header("Action Settings")]
    [Range(1f, 20f)]
    public float rotationSpeed = 10.0f;
    [Range(1f, 10f)]
    public float rollPower = 7f; // 루트 모션 미적용 구르기일 경우를 대비한 여분
    [Range(1f, 10f)]
    public float attackSpeed = 1;

    [Space(10)]
    [Header("playerStat")]
    [Range(1f, 1000f)]
    public int hp;
    private int fullHp;
    public Image hpBar;

    // scPlayerStat pS; 

    [Space(10)]
    private CharacterController charCon;
    private Transform cmTr;

    private float jumpPower;
    private float gravity;
    [Header("others")]
    public Animator anim;
    public Vector3 MoveDir; 
    private bool isDie;
    public bool isAttacking = false; 

    public float input;

    private bool isRolling;

    // 최적화를 위한 애니메이터 파라미터 해시 캐싱
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashWalk = Animator.StringToHash("Walk");
    private readonly int hashRun = Animator.StringToHash("Run");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashRoll = Animator.StringToHash("Roll");
    private readonly int hashJump = Animator.StringToHash("Jump");

    void Awake()
    {
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;
        anim.SetFloat(hashSpeed, 0f);
    }

    void Start()
    {
        MoveDir = Vector3.zero;
        jumpPower = 9.0f;
        gravity = 20.0f;
        fullHp = hp;
    }

    void Update()
    {
        if (isDie) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v);
        Vector3 camForward = cmTr.forward;
        Vector3 camRight = cmTr.right;
        camForward.y = 0;
        camRight.y = 0;
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // 1. 전투 및 구르기 액션
        if (Input.GetMouseButtonDown(0))
        {
            if (!isAttacking && !isRolling && charCon.isGrounded)
            {
                isAttacking = true;
                anim.SetTrigger(hashAttack);
                StartCoroutine(AttackRoutine());
            }
        }

        if (Input.GetButtonDown("LeftCtrl") && !isRolling && !isAttacking && charCon.isGrounded)
        {
            StartCoroutine(Rolling());
        }

        // 2. 이동 및 회전 로직 (Blend Tree 0~1 맵핑)
        if (!isRolling && !isAttacking)
        {
            float targetSpeed = 0f;

            if (inputDir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if (charCon.isGrounded)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        anim.SetBool(hashRun, true);
                        anim.SetBool(hashWalk, false);
                        targetSpeed = runBlendValue; // 1.0
                    }
                    else
                    {
                        anim.SetBool(hashWalk, true);
                        anim.SetBool(hashRun, false);
                        targetSpeed = walkBlendValue; // 0.5
                    }
                }
            }
            else
            {
                anim.SetBool(hashWalk, false);
                anim.SetBool(hashRun, false);
                targetSpeed = 0f; // 0.0
            }

            // [핵심] 그냥 SetFloat을 쓰면 애니메이션이 뚝뚝 끊깁니다. 
            // dampTime을 넣어주면 0 -> 0.5 -> 1.0 으로 부드럽게 값이 전환됩니다.
            anim.SetFloat(hashSpeed, targetSpeed, speedDampTime, Time.deltaTime);
        }

        // 3. 중력 및 점프 제어
        if (charCon.isGrounded)
        {
            if (MoveDir.y < 0) MoveDir.y = -2f;

            if (Input.GetButtonDown("Jump") && !isRolling && !isAttacking)
            {
                MoveDir.y = jumpPower;
                anim.SetTrigger(hashJump);
            }
        }
        else
        {
            MoveDir.y -= gravity * Time.deltaTime;
        }
    }

    private void OnAnimatorMove()
    {
        if (isDie || Time.deltaTime == 0) return;

        Vector3 rootMotionDelta = anim.deltaPosition;
        rootMotionDelta.y = MoveDir.y * Time.deltaTime;

        charCon.Move(rootMotionDelta);
    }

    IEnumerator Rolling()
    {
        isRolling = true;
        anim.SetTrigger(hashRoll);
        yield return new WaitForSeconds(1f); 
        isRolling = false;
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    public int TakeDamage(int damage)
    {
        hp -= damage;
        hpBar.fillAmount = (float)hp / (float)fullHp;
        Debug.Log("takeDamage : " + damage);
        if (hp <= 0) Die();
        return damage;
    }

    void Die()
    {
        Debug.Log("you die");
    }
}