using System.Collections;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private CharacterController charCon;
    private Transform cmTr;
    public float moveSpeed;
    private float jumpPower;
    private float gravity;
    public Animator anim;
    public Vector3 MoveDir;
    public bool isDie;
    public float rotationSpeed = 10.0f;
    public float runningSpeed = 3f;
    public GameObject trail;
    public GameObject[] weapons;
    public Transform weaponPoint;
    private GameObject currentWeapon;
    private int weaponIndex = 0;

    void Awake()
    {
        charCon = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cmTr = GameObject.FindGameObjectWithTag("MainCamera").transform;

        if (weaponPoint == null)
        {
            Transform[] allWeapons = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allWeapons)
            {
                if (child.name == "WeaponPoint")
                {
                    weaponPoint = child;
                    break;
                }
            }
        }
    }

    void Start()
    {
        MoveDir = Vector3.zero;
        moveSpeed = 1.3f;
        jumpPower = 9.0f;
        gravity = 20.0f;
    }

    void Update()
    {
        WeaponSwap();
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
            anim.SetTrigger("Attack");
            if (trail != null) { this.StartCoroutine(TrailWeapon()); }//trail.SetActive(true);}
            anim.SetBool("Walk", false);
            anim.SetBool("RightSide", false);
            anim.SetBool("LeftSide", false);
        }

        if (charCon.isGrounded)
        {
            MoveDir = moveDir.normalized * moveSpeed;

            if (inputDir.magnitude > 0.1f)
            {
                if (Mathf.Abs(v) < 0.1f && Mathf.Abs(h) > 0.1f)
                {
                    anim.SetBool("Walk", false);
                    if (h > 0.1f)
                    {
                        anim.SetBool("RightSide", true);
                        anim.SetBool("LeftSide", false);
                    }
                    else if (h < -0.1f)
                    {
                        anim.SetBool("LeftSide", true);
                        anim.SetBool("RightSide", false);
                    }
                }
                else
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
            }
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("RightSide", false);
                anim.SetBool("LeftSide", false);
            }

            if (Input.GetButton("Jump"))
            {
                MoveDir.y = jumpPower;
                anim.SetTrigger("Jump");
            }
        }
        MoveDir.y -= gravity * Time.deltaTime;
        charCon.Move(MoveDir * Time.deltaTime);
    }

    void WeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(3, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1);
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null) return;

        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        Transform spawnPoint = weaponPoint != null ? weaponPoint : transform;
        currentWeapon = Instantiate(weapons[index], spawnPoint.position, spawnPoint.rotation, spawnPoint);
    }
    IEnumerator TrailWeapon()
    {
        yield return new WaitForSeconds(0.3f);
        trail.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        trail.SetActive(false);
    }
}

