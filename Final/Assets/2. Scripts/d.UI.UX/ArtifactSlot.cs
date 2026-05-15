using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 인벤토리 슬롯 1개를 담당하는 컴포넌트
/// ─ 슬롯 Button 오브젝트에 부착
/// ─ 선택 / 비선택 상태에 따라 아이콘·테두리 Sprite 교체
/// </summary>
public class ArtifactSlot : MonoBehaviour, IPointerClickHandler
{
    /* ─────────────── Inspector ─────────────── */
    [Header("슬롯 UI 참조")]
    [SerializeField] private Image iconImage;       // 아이콘 Image 컴포넌트
    [SerializeField] private Image borderImage;     // 테두리 Image 컴포넌트

    [Header("기본(공통) 아이콘 스프라이트")]
    [SerializeField] private Sprite defaultIconNormal;  // 비선택 상태 기본 아이콘
    [SerializeField] private Sprite defaultIconLit;     // 선택 상태 기본 아이콘

    [Header("기본(공통) 테두리 스프라이트")]
    [SerializeField] private Sprite defaultBorderNormal;
    [SerializeField] private Sprite defaultBorderLit;

    [Header("빈 슬롯 표시용 스프라이트 (선택사항)")]
    [SerializeField] private Sprite emptySlotSprite;

    /* ─────────────── Runtime ─────────────── */
    private ArtifactData _data;
    private bool _isEmpty = true;
    private bool _isSelected = false;

    /// <summary>슬롯이 클릭됐을 때 외부(InventoryUI)에 알리는 이벤트</summary>
    public event Action<ArtifactSlot> OnSlotClicked;

    // ──────────────────────────────────────────

    /// <summary>데이터 주입 (InventoryUI에서 호출)</summary>
    public void Setup(ArtifactData data)
    {
        _data = data;
        _isEmpty = (data == null);
        SetSelected(false);
    }

    /// <summary>선택 상태 전환 (외부에서 호출)</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        RefreshVisuals();
    }

    public ArtifactData GetData() => _data;
    public bool IsEmpty => _isEmpty;

    // ──────────────────────────────────────────

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isEmpty) return;
        OnSlotClicked?.Invoke(this);
    }

    // ──────────────────────────────────────────

    private void RefreshVisuals()
    {
        if (_isEmpty)
        {
            // 빈 슬롯: 공백 처리
            if (iconImage != null)
            {
                iconImage.sprite = emptySlotSprite;
                iconImage.enabled = emptySlotSprite != null;
            }
            SetBorder(false);
            return;
        }

        // 아이콘
        SetIcon(_isSelected);

        // 테두리
        SetBorder(_isSelected);
    }

    /// <summary>
    /// 아이콘 스프라이트 교체
    /// 우선순위: ArtifactData 개별 스프라이트 > 슬롯 공통 기본 스프라이트
    /// </summary>
    private void SetIcon(bool lit)
    {
        if (iconImage == null) return;

        iconImage.enabled = true;

        Sprite litSprite    = (_data != null && _data.iconLit    != null) ? _data.iconLit    : defaultIconLit;
        Sprite normalSprite = (_data != null && _data.iconNormal != null) ? _data.iconNormal : defaultIconNormal;

        iconImage.sprite = lit ? litSprite : normalSprite;
    }

    /// <summary>
    /// 테두리 스프라이트 교체
    /// 우선순위: ArtifactData 개별 스프라이트 > 슬롯 공통 기본 스프라이트
    /// </summary>
    private void SetBorder(bool lit)
    {
        if (borderImage == null) return;

        Sprite litSprite    = (_data != null && _data.borderLit    != null) ? _data.borderLit    : defaultBorderLit;
        Sprite normalSprite = (_data != null && _data.borderNormal != null) ? _data.borderNormal : defaultBorderNormal;

        borderImage.sprite = lit ? litSprite : normalSprite;
    }
}
