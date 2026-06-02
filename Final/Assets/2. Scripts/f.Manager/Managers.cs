using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance {get{Init(); return _instance;}}

    private SoundManager _sound;
    public static SoundManager Sound => Instance._sound;

    void Start()
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

            _instance._sound = go.GetComponent<SoundManager>();
            if (_instance._sound == null)
            {
                _instance._sound = go.AddComponent<SoundManager>();
            }
            _instance._sound.Init();
        }
    }
}
