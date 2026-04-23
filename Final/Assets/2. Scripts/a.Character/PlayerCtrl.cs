using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private CharacterController charCon;
    private Transform cmTr;
    private float moveSpeed;
    private float jumpPower;
    private float gravity;
    public Animator anim;
    public Vector3 MoveDir;
    public bool isDie;
    public float rotationSpeed = 10.0f;
    private float mouseMove;

    void Awake()
    {
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Start()
    {
        MoveDir = Vector3.zero;
        moveSpeed = 1.3f;
        jumpPower = 8.0f;
        gravity = 20.0f;
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


        if (charCon.isGrounded)
        {
            MoveDir = moveDir.normalized * moveSpeed;

            if (inputDir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                anim.SetBool("Run", true);
            }
            else
            {
                anim.SetBool("Run", false);
            }
            if (Input.GetButton("Jump")) MoveDir.y = jumpPower;
        }
        if (h > 0.5f)
        {
            anim.SetBool("RightSide", true);
        }
        else anim.SetBool("RightSide", false);
        MoveDir.y -= gravity * Time.deltaTime;
        charCon.Move(MoveDir * Time.deltaTime);
    }
}