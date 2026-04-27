using Unity.VisualScripting;
using UnityEngine;

public class AttackMotion : MonoBehaviour
{
    public GameObject[] weaponPrefabs;
    public Transform weaponPoint;
    private GameObject currentWeapon;
    public Animator anim;
    private PlayerCtrl player;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if(weaponPoint==null)
        {
            Transform[] allWeapons = GetComponentsInChildren<Transform>(true);
            foreach(Transform child in allWeapons)
            {
                if(child.name == "WeaponPoint")
                {
                    weaponPoint = child;
                    break;
                }
            }
        }
    }

    void Update()
    {
        WeaponSwap();
    }

    void WeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(3, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1);
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Length || weaponPrefabs[index] == null) return;

        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        Transform spawnPoint = weaponPoint != null ? weaponPoint : transform;
        currentWeapon = Instantiate(weaponPrefabs[index], spawnPoint.position, spawnPoint.rotation, spawnPoint);
    }
}