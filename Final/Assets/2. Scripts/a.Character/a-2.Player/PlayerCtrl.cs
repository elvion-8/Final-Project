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

    [Space(10)]
    [Header("playerStat")]
    [Range(1f, 1000f)]
    public int hp;
    private int fullHp;
    public Image hpBar;

    scPlayerStat stat = scPlayerStat.stat;

    [Space(10)]
    private CharacterController charCon;
    private Transform cmTr;

    private float jumpPower;
    private float gravity;
    [Header("others")]
    public Animator anim;
    public Vector3 MoveDir;
    private bool isDie;
    public bool isAttacking = false;        //무기들에서 공격 여부를 참조하기 때문에 퍼블릭


    private bool isRolling;


    void Awake()
    {
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Start()
    {
        MoveDir = Vector3.zero;
        jumpPower = 9.0f;
        gravity = 20.0f;
        hp += stat.hpUpgrade * 50;
        fullHp = hp;
    }

    void Update()
    {
        if (isDie) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v);
        Vector3 camForward = cmTr.forward;
        Vector3 camRight = cmTr.right;
        camForward.y = 0;
        camRight.y = 0;
        Vector3 moveDir = camForward * v + camRight * h;

        if (Input.GetMouseButtonDown(0))
        {
            if (!isAttacking)
            {
                isAttacking = true;
                anim.SetTrigger("Attack");
                StartCoroutine(AttackRoutine());
            }
        }

        if (charCon.isGrounded)
        {
            if (!isRolling)
            {
                MoveDir = moveDir.normalized * moveSpeed;
                if (inputDir.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    if (Input.GetKey(KeyCode.LeftShift))
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
                    anim.SetBool("Walk", false);
                    anim.SetBool("RightSide", false);
                    anim.SetBool("LeftSide", false);
                }

                if (Input.GetButtonDown("Jump"))
                {
                    MoveDir.y = jumpPower;
                    anim.SetTrigger("Jump");
                }
            }
            if (Input.GetButtonDown("LeftCtrl") && !isRolling)
            {
                StartCoroutine(Rolling());
            }
            tempJumpCnt = 0;      //땅에 닿으면 연속 점프 카운트 초기화

        }
        if (Input.GetButtonDown("Jump") && tempJumpCnt <= jumpCnt)
        {
            tempJumpCnt++;
            MoveDir.y = jumpPower;
            anim.SetTrigger("Jump");
        }
        MoveDir.y -= gravity * Time.deltaTime;
        charCon.Move(MoveDir * Time.deltaTime);
    }

    IEnumerator Rolling()
    {
        isRolling = true;
        anim.SetTrigger("Roll");
        Vector3 rollDir = transform.forward * rollPower;
        MoveDir.x = rollDir.x;
        MoveDir.z = rollDir.z;
        yield return new WaitForSeconds(1f);
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
        if (hp <= 0)
        {
            Die();
        }
        return damage;
    }
    void Die()
    {
        Debug.Log("you die");
    }
}
