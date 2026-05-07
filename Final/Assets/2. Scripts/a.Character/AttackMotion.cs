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
                
                //Debug.Log("trail");
                StartCoroutine(TrailWeapon());
            }
        }
    }
    IEnumerator TrailWeapon()
    {
        if (currentWeapon != null)
        {
            scWeaponBase weaponScript = currentWeapon.GetComponent<scWeaponBase>();

            if (weaponScript != null)
            {
                if (weaponScript.pS == null) weaponScript.pS = player.pS;

                attackSpeed = weaponScript.attackSpeed;
                //Debug.Log("공격 중인 무기: " + currentWeapon.name);
            }
        }
        yield return new WaitForSeconds(0.3f);
        trail.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        trail.SetActive(false);
        yield return new WaitForSeconds(1f / attackSpeed);
        //player.isAttacking = false;
    }

    void WeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0);
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(4, 1f);

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1);
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipWeapon(2);
            anim.SetLayerWeight(3, 1f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 0f);
        }
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

        scWeaponBase weaponScript = currentWeapon.GetComponent<scWeaponBase>();
        if (weaponScript == null)
        {
            // 만약 루트에 없다면 자식에서도 찾아봄
            weaponScript = currentWeapon.GetComponentInChildren<scWeaponBase>();
        }

        if (weaponScript != null)
        {
            // 무기(영체)에게 플레이어의 스탯(그림자)을 직접 전달
            weaponScript.pS = player.pS;
            Debug.Log($"{currentWeapon.name}에 스탯 연결 완료!");
        }
        else
        {
            Debug.LogError("생성된 무기에서 scWeaponBase를 찾을 수 없습니다!");
        }
    }
}