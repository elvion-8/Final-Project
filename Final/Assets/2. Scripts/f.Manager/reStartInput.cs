using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        input.ResetAwake();
        Managers.Instance.ReStartInit();
    }
}
