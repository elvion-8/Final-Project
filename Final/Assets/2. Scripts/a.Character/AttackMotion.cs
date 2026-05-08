using UnityEngine;
using System.Collections;

public class AttackMotion : MonoBehaviour
{
    public GameObject[] weaponPrefabs;
    public Transform weaponPoint;
    private GameObject currentWeapon;
    public Animator anim;
    private PlayerCtrl player;
    public GameObject trail;
    public float attackSpeed;

    void Awake()
    {
        player = GetComponent<PlayerCtrl>();
        anim = GetComponentInChildren<Animator>();
        attackSpeed = player.attackSpeed;
        if (weaponPoint == null)
        {
            Transform[] allWeapons = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allWeapons)
            {
                if (child.name == "WeaponPoint")
                {
                    weaponPoint = child;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>().isAttacking == false)
        {
            WeaponSwap();
        }
        if (trail != null)
        {
            if (player.isAttacking)
            {
                Debug.Log("trail");
                StartCoroutine(TrailWeapon());
            }
        }
    }
    IEnumerator TrailWeapon()
    {
        if (GameObject.FindWithTag("Weapon") != null)
        {
            attackSpeed = GameObject.FindWithTag("Weapon").GetComponent<IWeaponStats>().attackSpeed;

        }
        yield return new WaitForSeconds(0.3f);
        trail.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        trail.SetActive(false);
        yield return new WaitForSeconds(1f / attackSpeed);
    }

    void WeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(4, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 1f);
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
        trail = currentWeapon.GetComponentInChildren<TrailRenderer>(true).gameObject;
    }
}