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
        input.ReseAwake();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
