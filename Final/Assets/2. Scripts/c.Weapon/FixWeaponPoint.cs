using UnityEngine;

public class FixWeaponPoint : MonoBehaviour
{
    public Vector3 fixedLocalRotation;

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(fixedLocalRotation);
    }
}
