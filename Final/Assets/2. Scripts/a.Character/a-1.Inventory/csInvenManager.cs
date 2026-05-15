using System.Collections.Generic;
using UnityEngine;

public class csInvenManager : MonoBehaviour
{
    public static csInvenManager Instance;

    public GameObject pnlInven;

    [Header("핫바 설정")]

    public csInvSlot[] hotbarSlots = new csInvSlot[4];
    public int selectHotKey = -1;

    [Header("인벤토리 설정")]
    public GameObject slotPrefab;
    public Transform slotContainer;
    public int totalSlotCount = 20;
    public List<ItemData> startingItems = new List<ItemData>();

    private List<csInvSlot> _inventorySlots = new List<csInvSlot>();
    private csInvSlot _selectedSlot;
    //public csInvSlot[] inventorySlots;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        BuildSlots();
    }

    // void Start()
    // {
    //     if (inventorySlots != null)
    //     {
    //         foreach (var slot in inventorySlots)
    //         {
    //             slot.OnSlotClicked += HandleInventorySlotClicked;
    //         }
    //     }
    // }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryActive();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectHotKey = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) selectHotKey = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) selectHotKey = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) selectHotKey = 3;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            selectHotKey = -1;
        }
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
            if (slot == null)
            {
                continue;
            }
            ItemData data = i < startingItems.Count ? startingItems[i] : null;
            slot.Setup(data);
            slot.OnSlotClicked += HandleInventorySlotClicked;
            _inventorySlots.Add(slot);
        }
    }
    private void HandleInventorySlotClicked(csInvSlot clickedSlot)
    {
        if (clickedSlot.IsEmpty) return;

        // [선택 표시 로직] 클릭한 슬롯을 하이라이트(선택) 처리
        if (_selectedSlot != null && _selectedSlot != clickedSlot)
        {
            _selectedSlot.SetSelected(false); // 이전 선택 해제
        }
        _selectedSlot = clickedSlot;
        _selectedSlot.SetSelected(true);      // 새 슬롯 선택

        // 만약 아이템 이름/설명을 띄우는 UI가 있다면 여기서 갱신 함수를 호출하세요.
        // UpdateInfoPanel(_selectedSlot.GetData());

        // [핫바 할당 로직] 숫자키(1~4)가 눌려있는 대기 상태라면 핫바로 데이터 넘기기
        if (selectHotKey != -1)
        {
            ItemData dataToAssign = clickedSlot.GetData();

            if (hotbarSlots[selectHotKey].CanAcceptItem(dataToAssign))
            {
                hotbarSlots[selectHotKey].Setup(dataToAssign);
                Debug.Log($"핫바 {selectHotKey + 1}번에 장착 완료");

                // ※ 기획에 따라 결정: 아이템을 '이동'시킬 거라면 원래 슬롯을 비웁니다. '복사'라면 아래 줄을 지우세요.
                // clickedSlot.Setup(null); 
            }
            else
            {
                Debug.Log("이 아이템은 해당 핫바 슬롯에 장착할 수 없습니다.");
            }

            // 장착이 끝났으므로 상태 초기화
            selectHotKey = -1;
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }
    }

    void InventoryActive()
    {
        if (pnlInven.activeSelf == false)
        {
            pnlInven.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!csButtonManager.isMultiplayer)
            {
                Time.timeScale = 0f;
            }
        }
        else
        {
            pnlInven.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!csButtonManager.isMultiplayer)
            {
                Time.timeScale = 1f;
            }
        }
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
        // 씬 전환/파괴 시 메모리 누수를 막기 위해 이벤트 구독 해제
        foreach (var slot in _inventorySlots)
        {
            if (slot != null)
                slot.OnSlotClicked -= HandleInventorySlotClicked;
        }
    }
}
