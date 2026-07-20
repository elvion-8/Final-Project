using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class reStartInput : MonoBehaviour
{
    InputManager input;
    GameOver gameOver;
    // Start is called before the first frame update
    void Awake()
    {
        input = GameObject.Find("@Managers").GetComponent<InputManager>();
        gameOver = GameObject.Find("@Managers").GetComponent<GameOver>();

    }
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        input.ResetAwake();
        Managers.Instance.ReStartInit();
    }
}
