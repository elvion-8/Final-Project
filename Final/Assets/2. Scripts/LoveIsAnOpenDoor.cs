using UnityEngine;

public class LoveIsAnOpenDoor : MonoBehaviour
{
    public Transform door;
    public float doorOpenSpeed;
    public Vector3 openPos;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool isOpen = false;

    void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        if(door!=null)
        {
            startPos = door.position;
            endPos = startPos+new Vector3(4.5f,0,0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isOpen)
        {
            door.position = Vector3.Lerp(door.position,endPos,Time.deltaTime*doorOpenSpeed);
            if(Vector3.Distance(door.position,endPos)<0.01f)
            {
                door.position=endPos;
                isOpen = false;
            }
        }
        else
        {
            door.position = Vector3.Lerp(door.position,startPos,Time.deltaTime*doorOpenSpeed);
            if(Vector3.Distance(door.position,startPos)<0.01f)
            {
                door.position=startPos;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            isOpen = true;
        }
    }
}
