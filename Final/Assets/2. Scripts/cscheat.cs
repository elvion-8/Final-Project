using UnityEngine;

public class cscheat : MonoBehaviour
{
    public static cscheat instance { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            InventoryUpgrade();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            HpDown();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            AddCost(100);
        }
    }

    private void InventoryUpgrade()
    {
        csInvenManager inven = csInvenManager.Instance;
        if (inven == null)
        {
            inven = FindObjectOfType<csInvenManager>();
        }

        if (inven != null)
        {
            inven.ExpandInventory(1);
            Debug.Log("[cscheat] 인벤토리 슬롯 1칸 확장 완료");
        }
        else
        {
            Debug.LogWarning("[cscheat] csInvenManager를 찾을 수 없습니다.");
        }
    }

    private void HpDown()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            ITakeDamage target = playerObj.GetComponent<ITakeDamage>();
            if (target != null)
            {
                target.TakeDamage(10);
            }
            else
            {
                playerObj.SendMessage("TakeDamage", 10, SendMessageOptions.DontRequireReceiver);
            }
            Debug.Log("[cscheat] 플레이어 HP 감소 완료 (-10)");
        }
        else
        {
            Debug.LogWarning("[cscheat] Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void AddCost(int amount)
    {
        bool updated = false;

        PlayerStatManage statManage = FindObjectOfType<PlayerStatManage>();
        if (statManage != null && statManage.stat != null)
        {
            statManage.stat.cost += amount;
            updated = true;
        }

        if (Managers.Data != null && Managers.Data.stat != null)
        {
            if (statManage == null || statManage.stat != Managers.Data.stat)
            {
                Managers.Data.stat.cost += amount;
            }
            Managers.Data.SaveGame();
            updated = true;
        }

        PlayerStatController[] activeControllers = FindObjectsOfType<PlayerStatController>();
        foreach (var controller in activeControllers)
        {
            controller.LoadPermanentStats();
        }

        PlayerStatController.OnStatsChanged?.Invoke();

        if (updated)
        {
            Debug.Log($"[cscheat] Cost {amount} 추가 완료");
        }
        else
        {
            Debug.LogWarning("[cscheat] PlayerStatManage 또는 Managers.Data.stat을 찾을 수 없습니다.");
        }
    }
}
