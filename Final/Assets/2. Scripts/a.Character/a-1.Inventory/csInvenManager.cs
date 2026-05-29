using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class csInvenManager : MonoBehaviour
{
    public static csInvenManager Instance;

    public GameObject pnlInven;

    [Header("핫바 설정")]
    public csInvSlot[] hotbarSlots = new csInvSlot[4];

    // 선택된 핫바의 번호 기억
    private int _activeHotbarIndex = -1;

    [Header("인벤토리 설정")]
    public GameObject slotPrefab;
    public Transform slotContainer;
    public int totalSlotCount = 20;
    public List<ItemData> startingItems = new List<ItemData>();

    private List<csInvSlot> _inventorySlots = new List<csInvSlot>();
    private csInvSlot _selectedSlot;
    private bool onInven;

    public void OnInventory(InputValue value) { if(value.isPressed) onInven = true; }
    public void OnWeaponChange(InputValue value) {float weaponIndex = value.Get<float>(); Debug.Log(weaponIndex);}
    void ResetTriggers()
    {
        onInven=false;
    }
    [Header("text")]
    [SerializeField] private Text txtWType;
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtSub;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        BuildSlots();
    }

    void Update()
    {
        ToggleInventory();

        if (pnlInven.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) AssignToHotbarDirectly(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) AssignToHotbarDirectly(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) AssignToHotbarDirectly(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) AssignToHotbarDirectly(3);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectActiveHotbar(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectActiveHotbar(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectActiveHotbar(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectActiveHotbar(3);
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            ClearSelection();
        }
        ResetTriggers();
    }

    public void SelectActiveHotbar(int index)
    {
        _activeHotbarIndex = index;

        // 4개의 핫바 슬롯을 모두 검사합니다.
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].SetBorder(i == index);
            }
        }
    }

    private void HandleInventorySlotClicked(csInvSlot clickedSlot)
    {
        if (clickedSlot.IsEmpty) return;

        if (_selectedSlot != null && _selectedSlot != clickedSlot)
        {
            _selectedSlot.SetSelected(false);
        }

        _selectedSlot = clickedSlot;
        _selectedSlot.SetSelected(true);
        UpdateInfoPnl(_selectedSlot.GetData());
    }

    private void AssignToHotbarDirectly(int hotbarIndex)
    {
        if (_selectedSlot == null)
        {
            Debug.Log("인벤토리 무기 선택 먼저");
            return;
        }

        ItemData dataToAssign = _selectedSlot.GetData();

        if (hotbarSlots[hotbarIndex] != null && hotbarSlots[hotbarIndex].CanAcceptItem(dataToAssign))
        {
            hotbarSlots[hotbarIndex].Setup(dataToAssign);

            SelectActiveHotbar(hotbarIndex);

            Debug.Log($"선택한 아이템을 핫바 {hotbarIndex + 1}번에 장착했습니다.");
        }
        else
        {
            Debug.Log("핫바 장착 불가");
        }

        ClearSelection();
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }
        ClearInfoPnl();
    }

    private void BuildSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        _inventorySlots.Clear();
        for (int i = 0; i < totalSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            go.name = $"InvSlot_{i:00}";

            csInvSlot slot = go.GetComponent<csInvSlot>();
            if (slot == null) continue;

            ItemData data = i < startingItems.Count ? startingItems[i] : null;
            slot.Setup(data);
            slot.OnSlotClicked += HandleInventorySlotClicked;
            _inventorySlots.Add(slot);
        }
    }

    void ToggleInventory()
    {
        if (onInven)
        {
            bool isActivating = !pnlInven.activeSelf;
            pnlInven.SetActive(isActivating);

            Cursor.lockState = isActivating ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isActivating;

            if (!csButtonManager.isMultiplayer)
            {
                Time.timeScale = isActivating ? 0f : 1f;
            }

            if (!isActivating)
            {
                ClearSelection();
            }
        }
        ResetTriggers();
    }

    public ItemData GetHotbarItem(int index)
    {
        if (index >= 0 && index < hotbarSlots.Length && hotbarSlots[index] != null)
        {
            return hotbarSlots[index].GetData();
        }
        return null;
    }

    void OnDestroy()
    {
        foreach (var slot in _inventorySlots)
        {
            if (slot != null)
                slot.OnSlotClicked -= HandleInventorySlotClicked;
        }
    }

    void UpdateInfoPnl(ItemData data)
    {
        if (data == null) { ClearInfoPnl(); return; }

        if (txtWType != null) txtWType.text = data.itemType.ToString();
        if (txtTitle != null) txtTitle.text = data.itemName;
        if (txtSub != null) txtSub.text = data.itemDesc;
    }

    void ClearInfoPnl()
    {
        if (txtWType != null) txtWType.text = string.Empty;
        if (txtTitle != null) txtTitle.text = string.Empty;
        if (txtSub != null) txtSub.text = string.Empty;
    }
}