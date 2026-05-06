using UnityEngine;

public class scGetDamageTest : MonoBehaviour
{
    public GameObject target;
    ITakeDamage damageable;

    void Awake()
    {
        damageable = target.GetComponent<ITakeDamage>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            GetDamage();
        }
    }
    public void GetDamage()
    {
        damageable.TakeDamage(10);
    }
}
