using UnityEngine;

public class reStartInput : MonoBehaviour
{
    InputManager input;
    // Start is called before the first frame update
    void Awake()
    {
        input = GameObject.Find("@Managers").GetComponent<InputManager>();
    }
    void Start()
    {
        input.ResetAwake();
        Managers.Instance.ReStartInit();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
