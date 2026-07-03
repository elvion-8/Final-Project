using UnityEngine;


public class ResetManager : MonoBehaviour
{
    GameObject temp;

    void Awake()
    {
        temp = GameObject.Find("@Managers");
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (temp != null)
        {
            temp.SetActive(false);
            
            temp.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
