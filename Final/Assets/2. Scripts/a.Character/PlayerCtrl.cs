using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private CharacterController charCon;
    private float moveSpeed;
    private float jumpPower;
    private float gravity;
    public Animator anim;
    public Vector3 MoveDir;
    public bool isDie;
    public float rotationSpeed = 10.0f;

    void Awake()
    {
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
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

        if (charCon.isGrounded)
        {
            MoveDir = inputDir.normalized * moveSpeed;

            if (inputDir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                //transform.rotation = Quaternion.LookRotation(inputDir);
                anim.SetBool("Run", true);
            }
            else
            {
                anim.SetBool("Run", false);
            }
            if (Input.GetButton("Jump")) MoveDir.y = jumpPower;
        }
        MoveDir.y -= gravity * Time.deltaTime;
        charCon.Move(MoveDir * Time.deltaTime);
    }
}