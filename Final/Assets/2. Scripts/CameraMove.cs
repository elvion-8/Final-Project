using UnityEngine;

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

    void Awake()
    {
        cmTr = GetComponent<Transform>();
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        mouseSensitivity = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        //MouseMove();            //마우스 화면 이동 함수

    }

    void LateUpdate()
    {
        MouseMove();
    }

    void MouseMove()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotX+=mouseX;
        rotY-=mouseY;
        rotY = Mathf.Clamp(rotY,-20f,90f);
        Quaternion rotation = Quaternion.Euler(rotY,rotX,0);
        Vector3 position = playerPos.position - (rotation * Vector3.forward*distance);

        cmTr.rotation = rotation;
        cmTr.position = position;
    }
}
