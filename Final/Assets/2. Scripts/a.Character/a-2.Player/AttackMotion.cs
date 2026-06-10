using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class AttackMotion : MonoBehaviour
{
    public GameObject[] weaponPrefabs;
    public Transform weaponPoint;
    private GameObject currentWeapon;
    public Animator anim;
    private PlayerCtrl player;
    public GameObject trail;
    public float attackSpeed;
    private bool isTrailing;
    CharacterController charCon;
    AttackSound sound;

    // Photon view //////////
    PhotonView pv;

    void Awake()
    {
        player = GetComponent<PlayerCtrl>();
        anim = GetComponentInChildren<Animator>();
        attackSpeed = player.attackSpeed;
        charCon = GetComponent<CharacterController>();
        pv = GetComponent<PhotonView>();
        sound = GetComponentInChildren<AttackSound>();

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
        // if (GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>().isAttacking == false)
        // {
        //     WeaponSwap();
        // }
        if (trail != null)
        {
            if (player.isAttacking && !isTrailing)
            {
                isTrailing = false;
                //Debug.Log("trail");
                StartCoroutine(TrailWeapon());
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            WeaponSikll1();
        }
    }

    public void OnWeaponChange(InputValue value)
    {
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            if (GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>().isAttacking == false)
            {
                float weaponIndex = value.Get<float>();
                if (weaponIndex == 0) return;
                Debug.Log(weaponIndex);

                ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem((int)weaponIndex - 1);
                if (hotbarItem == null || hotbarItem.itemPrefab == null) return;

                int prefabIndex = -1;

                for (int i = 0; i < weaponPrefabs.Length; i++)
                {
                    if (weaponPrefabs[i] == hotbarItem.itemPrefab)
                    {
                        prefabIndex = i;
                        break;
                    }
                }

                if (prefabIndex == -1)
                {
                    Debug.LogError("weaponPrefabs 배열에 등록되지 않은 무기 프리팹입니다!");
                    return;
                }

                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("EquipWeapon", PhotonTargets.AllBuffered, prefabIndex);
                }
                else EquipWeapon(prefabIndex);

            }
        }
    }

    IEnumerator TrailWeapon()
    {
        //isTrailing = true;
        if (GameObject.FindWithTag("Weapon") != null)
        {
            attackSpeed = GameObject.FindWithTag("Weapon").GetComponent<IWeaponStats>().attackSpeed;
        }
        yield return new WaitForSeconds(0.3f);
        trail.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        trail.SetActive(false);
        yield return new WaitForSeconds(1f / attackSpeed);
        //isTrailing = false;
    }

    // void WeaponSwap()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha1))
    //     {
    //         EquipWeapon(0);
    //     }
    //     else if (Input.GetKeyDown(KeyCode.Alpha2))
    //     {
    //         EquipWeapon(1);
    //     }
    //     else if (Input.GetKeyDown(KeyCode.Alpha3))
    //     {
    //         EquipWeapon(2);
    //     }
    //     else if (Input.GetKeyDown(KeyCode.Alpha4))
    //     {
    //         EquipWeapon(3);
    //     }
    // }

    [PunRPC]
    public void EquipWeapon(int index)
    {
        if (index < 0 || index > weaponPrefabs.Length) return;
        if (csInvenManager.Instance == null) return;

        ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem(index);

        if (hotbarItem == null || hotbarItem.itemType != ItemType.Weapon || hotbarItem.itemPrefab == null) return;

        // 기존 무기 제거
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        // 애니메이션 레이어 조절 (임시: 무기 종류에 따라 레이어를 설정을 위해 ItemData에 속성을 추가해 구분 예정)
        if (index == 0) //Axe
        {
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 1f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(6, 0f);
            anim.SetLayerWeight(7, 0f);
            anim.SetLayerWeight(8, 0f);
        }
        else if (index == 1) // Dagger
        {
            anim.SetLayerWeight(3, 1f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(6, 0f);
            anim.SetLayerWeight(7, 0f);
            anim.SetLayerWeight(8, 0f);
        }
        else if (index == 2) // Spear
        {
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(6, 1f);
            anim.SetLayerWeight(7, 0f);
            anim.SetLayerWeight(8, 0f);
        }
        else if (index == 3) // Sword
        {
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 1f);
            anim.SetLayerWeight(6, 0f);
            anim.SetLayerWeight(7, 0f);
            anim.SetLayerWeight(8, 0f);
        }
        else if (index == 4) //Hammer
        {
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(6, 0f);
            anim.SetLayerWeight(7, 1f);
            anim.SetLayerWeight(8, 0f);
        }
        else if (index == 5) //Scythe
        {
            anim.SetLayerWeight(3, 0f);
            anim.SetLayerWeight(4, 0f);
            anim.SetLayerWeight(5, 0f);
            anim.SetLayerWeight(6, 0f);
            anim.SetLayerWeight(7, 0f);
            anim.SetLayerWeight(8, 1f);
        }

        Transform spawnPoint = weaponPoint != null ? weaponPoint : transform;
        currentWeapon = Instantiate(weaponPrefabs[index], spawnPoint.position, spawnPoint.rotation, spawnPoint);
        trail = currentWeapon.GetComponentInChildren<TrailRenderer>(true).gameObject;
        sound.ChangeWeapon(hotbarItem);
    }

    void WeaponSikll1()
    {
        charCon.enabled = false;
        GameObject.FindWithTag("Weapon").GetComponent<scWeaponBase>().Skill1();
        Debug.Log("스킬사용");
        charCon.enabled = true;
    }
}