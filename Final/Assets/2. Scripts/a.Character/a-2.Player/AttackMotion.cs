using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class AttackMotion : MonoBehaviour
{
    public GameObject[] weaponPrefabs;
    public Transform weaponPoint;
    private GameObject currentWeapon;
    public ItemData currentWeaponData;
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

    private int pendingWeaponIndex = -1;
    private ItemData pendingWeaponData;

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

            if (pendingWeaponIndex != -1)
            {
                if (!player.isAttacking)
                {
                    SwapToPendingWeapon();
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("-1");
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
        Debug.Log($"[AttackMotion] OnWeaponChange invoked. pv.isMine: {pv.isMine}, inRoom: {PhotonNetwork.inRoom}");
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            float weaponIndex = value.Get<float>();
            if (weaponIndex == 0) return;
            Debug.Log($"[AttackMotion] Requested weaponIndex: {weaponIndex}");

            ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem((int)weaponIndex - 1);
            if (hotbarItem == null)
            {
                Debug.LogWarning($"[AttackMotion] Hotbar item is null at slot {(int)weaponIndex - 1}");
                return;
            }
            if (hotbarItem.itemPrefab == null)
            {
                Debug.LogWarning($"[AttackMotion] Hotbar item prefab is null at slot {(int)weaponIndex - 1}");
                return;
            }

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
                Debug.LogError($"weaponPrefabs 배열에 등록되지 않은 무기 프리팹입니다! Prefab name: {hotbarItem.itemPrefab.name}");
                return;
            }

            if (player.isAttacking)
            {
                pendingWeaponIndex = prefabIndex;
                pendingWeaponData = hotbarItem;
                Debug.Log($"[AttackMotion] Attack in progress. Weapon swap buffered: {hotbarItem.itemName} (Index: {prefabIndex})");
            }
            else
            {
                pendingWeaponIndex = -1;
                pendingWeaponData = null;

                currentWeaponData = hotbarItem;
                Debug.Log($"[AttackMotion] Not attacking. Swapping immediately to: {hotbarItem.itemName} (Index: {prefabIndex})");

                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("EquipWeapon", PhotonTargets.AllBuffered, prefabIndex);
                }
                else EquipWeapon(prefabIndex);
            }
        }
    }

    private void SwapToPendingWeapon()
    {
        if (pendingWeaponIndex == -1) return;

        currentWeaponData = pendingWeaponData;
        int indexToEquip = pendingWeaponIndex;

        pendingWeaponIndex = -1;
        pendingWeaponData = null;

        Debug.Log($"[AttackMotion] Executing pending weapon swap to index: {indexToEquip}");

        if (PhotonNetwork.inRoom)
        {
            pv.RPC("EquipWeapon", PhotonTargets.AllBuffered, indexToEquip);
        }
        else
        {
            EquipWeapon(indexToEquip);
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
        if (index < 0 || index > weaponPrefabs.Length) return;

        vfxModifier.ChangeActionByIndex(index);

        // 기존 무기 제거
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        SetAnimationLayer(index);

        Transform spawnPoint = weaponPoint != null ? weaponPoint : transform;
        currentWeapon = Instantiate(weaponPrefabs[index], spawnPoint.position, spawnPoint.rotation, spawnPoint);

        trail = currentWeapon.GetComponentInChildren<TrailRenderer>(true).gameObject;
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            if (currentWeaponData != null && sound != null)
            {
                sound.ChangeWeapon(currentWeaponData);
            }
        }
    }

    void SetAnimationLayer(int index)
    {
        for (int i = 3; i <= 8; i++) anim.SetLayerWeight(i, 0f); // 우선 전부 0으로 초기화

        int attackLayerIndex = anim.GetLayerIndex("Attack");
        Debug.Log($"[AttackMotion] GetLayerIndex('Attack') 결과: {attackLayerIndex}");

        if (attackLayerIndex != -1)
        {
            anim.SetLayerWeight(attackLayerIndex, 1f);
            float currentWeight = anim.GetLayerWeight(attackLayerIndex);
            Debug.Log($"[AttackMotion] Attack 레이어 가중치 설정 시도 후 실제 값: {currentWeight}");

            // WeaponType 파라미터 업데이트
            int weaponTypeVal = GetWeaponTypeFromIndex(index);
            anim.SetInteger("WeaponType", weaponTypeVal);
            Debug.Log($"[AttackMotion] 통합 Attack 레이어 활성화 및 WeaponType 설정: {weaponTypeVal}");
        }
        else
        {
            Debug.LogWarning("[AttackMotion] 'Attack' 레이어를 찾을 수 없어 기존 개별 레이어 방식을 사용합니다. 아래는 현재 등록된 레이어 목록입니다:");
            for (int i = 0; i < anim.layerCount; i++)
            {
                Debug.Log($"[AttackMotion] Layer {i}: {anim.GetLayerName(i)}");
            }

            // 기존 방식 유지 (백업용)
            if (index == 0) anim.SetLayerWeight(4, 1f);      // Axe
            else if (index == 1) anim.SetLayerWeight(3, 1f); // Dagger
            else if (index == 2) anim.SetLayerWeight(6, 1f); // Spear
            else if (index == 3) anim.SetLayerWeight(5, 1f); // Sword
            else if (index == 4) anim.SetLayerWeight(7, 1f); // Hammer
            else if (index == 5) anim.SetLayerWeight(8, 1f); // Scythe
        }
    }

    private int GetWeaponTypeFromIndex(int index)
    {
        switch (index)
        {
            case 0: return 2; // Axe (WeaponType.Axe = 2)
            case 1: return 0; // Dagger (WeaponType.Dagger = 0)
            case 2: return 3; // Spear (WeaponType.Spear = 3)
            case 3: return 1; // Sword (WeaponType.Sword = 1)
            case 4: return 5; // Hammer (WeaponType.Hammer = 5)
            case 5: return 4; // Scythe (WeaponType.Scythe = 4)
            default: return 0;
        }
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
            Debug.LogError("현재 장착된 무기가 없어 스킬 실행 불가");
        }
        Debug.Log("스킬사용");
        charCon.enabled = true;
    }

    public int GetCurrentWeaponType()
    {
        if (currentWeaponData != null && currentWeaponData.itemType == ItemType.Weapon)
        {
            return (int)currentWeaponData.weaponType;
        }

        int index = WeaponNume();
        if (index != -1)
        {
            return GetWeaponTypeFromIndex(index);
        }

        return -1;
    }

    public int WeaponNume()
    {
        if (currentWeapon == null) return -1;

        string name = currentWeapon.name;
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            if (name.Contains(weaponPrefabs[i].name))
            {
                return i;
            }
        }
        return -1;
    }
}