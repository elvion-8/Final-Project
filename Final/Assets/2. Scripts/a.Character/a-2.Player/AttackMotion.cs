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

    private VFXEventModifier vfxModifier;
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
        vfxModifier = GetComponentInChildren<VFXEventModifier>();

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
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
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
                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("WeaponSikll1", PhotonTargets.All,pv.viewID);
                }
                else WeaponSikll1(-1);
            }
        }
    }

    public void OnWeaponChange(InputValue value)
    {
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            if (player.isAttacking == false)
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

    
    [PunRPC]
    public void EquipWeapon(int index)
    {
        if(player.isAttacking) return;
        if (index < 0 || index > weaponPrefabs.Length) return;
        //if (csInvenManager.Instance == null) return;

        //ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem(index);

        //if (hotbarItem == null || hotbarItem.itemType != ItemType.Weapon || hotbarItem.itemPrefab == null) return;

        vfxModifier.ChangeActionByIndex(index);

        // 기존 무기 제거
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        // 애니메이션 레이어 조절 (임시: 무기 종류에 따라 레이어를 설정을 위해 ItemData에 속성을 추가해 구분 예정)
        SetAnimationLayer(index);

        Transform spawnPoint = weaponPoint != null ? weaponPoint : transform;
        currentWeapon = Instantiate(weaponPrefabs[index], spawnPoint.position, spawnPoint.rotation, spawnPoint);

        trail = currentWeapon.GetComponentInChildren<TrailRenderer>(true).gameObject;
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem(index);
            if (hotbarItem != null && sound != null)
            {
                sound.ChangeWeapon(hotbarItem);
            }
        }
    }

    void SetAnimationLayer(int index)
    {
        for (int i = 3; i <= 8; i++) anim.SetLayerWeight(i, 0f); // 우선 전부 0으로 초기화

        if (index == 0) anim.SetLayerWeight(4, 1f);      // Axe
        else if (index == 1) anim.SetLayerWeight(3, 1f); // Dagger
        else if (index == 2) anim.SetLayerWeight(6, 1f); // Spear
        else if (index == 3) anim.SetLayerWeight(5, 1f); // Sword
        else if (index == 4) anim.SetLayerWeight(7, 1f); // Hammer
        else if (index == 5) anim.SetLayerWeight(8, 1f); // Scythe
    }

    [PunRPC]
    void WeaponSikll1(int senderViewID)
    {
        if (PhotonNetwork.inRoom && senderViewID != -1)
        {
            if (pv.viewID != senderViewID) return;
        }

        charCon.enabled = false;

        if (currentWeapon != null)
        {
            currentWeapon.GetComponentInChildren<scWeaponBase>().Skill1();
        }
        else
        {
            Debug.LogError("현재 장착된 무기(currentWeapon)가 없어 스킬을 실행할 수 없습니다.");
        }
        Debug.Log("스킬사용");
        charCon.enabled = true;
    }

    public int WeaponNume()
    {
        string name = GameObject.FindWithTag("Weapon").name;
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            if(weaponPrefabs[i].name == name)
            {
                return i;
            }
            
        }
        return -1;
    }
}