using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 아티팩트 인벤토리 전체를 관리하는 컨트롤러
/// ─ 슬롯 동적 생성 / 가로 스크롤 / 하단 정보 패널 갱신
/// </summary>
public class ArtifactInventoryUI : MonoBehaviour
{
    /* ─────────────── Inspector ─────────────── */
    [Header("슬롯 설정")]
    [SerializeField] private GameObject slotPrefab;     // ArtifactSlot 부착된 프리팹
    [SerializeField] private Transform  slotContainer;  // ScrollRect > Viewport > Content

    [Header("아티팩트 데이터 목록")]
    [SerializeField] private List<ArtifactData> artifactList = new();

    [Header("하단 정보 패널")]
    [SerializeField] private Text weaponTypeText;
    [SerializeField] private Text artifactNameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image     previewImage;

    [Header("기타")]
    [SerializeField] private Button    exitButton;
    [SerializeField] private int       totalSlotCount = 8;   // 표시할 전체 슬롯 수

    /* ─────────────── Runtime ─────────────── */
    private readonly List<ArtifactSlot> _slots = new();
    private ArtifactSlot _selectedSlot;

    // ──────────────────────────────────────────

    private void Awake()
    {
        BuildSlots();

        if (exitButton != null)
            exitButton.onClick.AddListener(CloseInventory);
    }

    private void Start()
    {
        // 시작 시 첫 번째 슬롯 자동 선택 (있을 경우)
        if (_slots.Count > 0 && !_slots[0].IsEmpty)
            SelectSlot(_slots[0]);
        else
            ClearInfoPanel();
    }

    // ──────────────────────────────────────────
    // 슬롯 생성
    // ──────────────────────────────────────────

    private void BuildSlots()
    {
        // 기존 슬롯 제거 (에디터 재시작 등 방어 코드)
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        _slots.Clear();

        for (int i = 0; i < totalSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            go.name = $"Slot_{i:00}";

            var slot = go.GetComponent<ArtifactSlot>();
            if (slot == null)
            {
                Debug.LogError("[InventoryUI] slotPrefab에 ArtifactSlot 컴포넌트가 없습니다.");
                continue;
            }

            // 데이터 배열 범위 내면 아이템, 초과하면 빈 슬롯
            ArtifactData data = i < artifactList.Count ? artifactList[i] : null;
            slot.Setup(data);
            slot.OnSlotClicked += SelectSlot;

            _slots.Add(slot);
        }
    }

    // ──────────────────────────────────────────
    // 슬롯 선택 처리
    // ──────────────────────────────────────────

    private void SelectSlot(ArtifactSlot clickedSlot)
    {
        if (clickedSlot == _selectedSlot) return;

        // 이전 선택 해제
        _selectedSlot?.SetSelected(false);

        _selectedSlot = clickedSlot;
        _selectedSlot.SetSelected(true);

        UpdateInfoPanel(_selectedSlot.GetData());
    }

    // ──────────────────────────────────────────
    // 하단 정보 패널 갱신
    // ──────────────────────────────────────────

    private void UpdateInfoPanel(ArtifactData data)
    {
        if (data == null) { ClearInfoPanel(); return; }

        if (weaponTypeText   != null) weaponTypeText.text   = data.weaponType;
        if (artifactNameText != null) artifactNameText.text = data.artifactName;
        if (descriptionText  != null) descriptionText.text  = data.description;

        if (previewImage != null)
        {
            previewImage.sprite  = data.iconLit != null ? data.iconLit : data.iconNormal;
            previewImage.enabled = previewImage.sprite != null;
        }
    }

    private void ClearInfoPanel()
    {
        if (weaponTypeText   != null) weaponTypeText.text   = string.Empty;
        if (artifactNameText != null) artifactNameText.text = string.Empty;
        if (descriptionText  != null) descriptionText.text  = string.Empty;
        if (previewImage     != null) previewImage.enabled  = false;
    }

    // ──────────────────────────────────────────
    // 외부 API
    // ──────────────────────────────────────────

    /// <summary>런타임에 아이템 추가</summary>
    public void AddArtifact(ArtifactData data)
    {
        // 빈 슬롯 중 첫 번째에 삽입
        var emptySlot = _slots.Find(s => s.IsEmpty);
        if (emptySlot == null) { Debug.LogWarning("빈 슬롯이 없습니다."); return; }
        emptySlot.Setup(data);
    }

    /// <summary>선택된 아이템 제거</summary>
    public void RemoveSelected()
    {
        if (_selectedSlot == null) return;
        _selectedSlot.Setup(null);
        _selectedSlot = null;
        ClearInfoPanel();
    }

    /// <summary>인벤토리 창 닫기</summary>
    public void CloseInventory()
    {
        gameObject.SetActive(false);
    }

    /// <summary>인벤토리 창 열기</summary>
    public void OpenInventory()
    {
        gameObject.SetActive(true);
    }
}

