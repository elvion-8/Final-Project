using UnityEngine;
//using UnityEngine.InputSystem;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance {get{Init(); return _instance;}}

    private SoundManager _sound;
    private DataManager _data;
    private InputManager _input;
    public static SoundManager Sound => Instance._sound;
    public static DataManager Data => Instance._data;
    public static InputManager Input => Instance._input;

    void Awake()
    {
        Init();
    }

    static void Init()
    {
        if(_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if(go==null)
            {
                go=new GameObject{name="@Managers"};
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            if(_instance._data == null)
            {
                _instance._data = new DataManager();
            }

            _instance._sound = go.GetComponent<SoundManager>();
            if (_instance._sound == null)
            {
                _instance._sound = go.AddComponent<SoundManager>();
            }
            _instance._input = go.GetComponent<InputManager>();
            if(_instance._input == null)
            {
                _instance._input = go.AddComponent<InputManager>();
            }
            _instance._data.Init();
            _instance._sound.Init();
            _instance._input.Init();
        }
    }
}
