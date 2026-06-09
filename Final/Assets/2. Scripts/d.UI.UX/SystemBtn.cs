using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SystemBtn : MonoBehaviour
{
    // ────────────────────────────────────────────
    // 버튼 종류 열거
    // ────────────────────────────────────────────
    public enum PanelType
    {
        Character       = 0,
        Skill    = 1,
        Inventory       = 2,
        Achievement      = 3,
        Community      = 4,
        Obtion   = 5,
        GameExit   = 6
    }

    // ────────────────────────────────────────────
    // 팝업 데이터 구조체
    // ────────────────────────────────────────────
    [System.Serializable]
    public class PopupEntry
    {
        public PanelType panelType;
        public Button    button;  
        public GameObject popup;      // 연결된 팝업 오브젝트
        [HideInInspector] public bool isOpen = false;
    }

    // ────────────────────────────────────────────
    // Inspector 연결
    // ────────────────────────────────────────────
    [Header("팝업 목록 (버튼 순서대로 연결)")]
    public List<PopupEntry> popupEntries = new List<PopupEntry>();

    [Header("옵션")]
    [Tooltip("다른 팝업 클릭 시 기존 팝업 자동 닫기")]
    public bool exclusiveMode = true;

    [Tooltip("같은 버튼 재클릭 시 팝업 닫기")]
    public bool toggleOnReclick = true;

    [Tooltip("ESC 키로 모든 팝업 닫기")]
    public bool closeAllOnEsc = true;

    // 현재 열린 팝업 추적
    private PopupEntry currentOpenEntry = null;
    // ────────────────────────────────────────────
    // InputSystem
    // ────────────────────────────────────────────
    private bool escapeBtn=false;
    public void OnEscape(InputValue value)
    {
        if(value.isPressed) escapeBtn=true;
    }

    // ────────────────────────────────────────────
    // 초기화
    // ────────────────────────────────────────────
    void Start()
    {
        // 모든 팝업 닫힌 상태로 초기화
        foreach (var entry in popupEntries)
        {
            if (entry.popup != null)
                entry.popup.SetActive(false);

            PopupEntry captured = entry;
            entry.button?.onClick.AddListener(() => OnButtonClicked(captured));
        }

        SetGameCursor(false);
    }

    void Update()
    {
        if (closeAllOnEsc && escapeBtn)
        {
            escapeBtn = false;
            if (currentOpenEntry != null)
            {
                CloseAllPopups(); // 패널 닫기
            }
            else
            {
                // 아무 패널도 없으면 커서 토글 (일시정지 메뉴용)
                ToggleCursor();
            }
        }
    }

    private bool _isCursorFree = false;

    private void SetGameCursor(bool free)
    {
        _isCursorFree = free;
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;

        if (!csButtonManager.isMultiplayer)
            Time.timeScale = free ? 0f : 1f;
    }

    private void ToggleCursor()
    {
        SetGameCursor(!_isCursorFree);
    }

    // ────────────────────────────────────────────
    // 버튼 클릭 처리
    // ────────────────────────────────────────────
    private void OnButtonClicked(PopupEntry entry)
    {
        if (entry.popup == null)
        {
            Debug.LogWarning($"[ButtonPnl] {entry.panelType} 팝업이 연결되지 않았습니다.");
            return;
        }

        if (toggleOnReclick && currentOpenEntry == entry)
        {
            ClosePopup(entry);
            return;
        }

        if (exclusiveMode && currentOpenEntry != null && currentOpenEntry != entry)
            ClosePopup(currentOpenEntry);

        OpenPopup(entry);
    }

    // ────────────────────────────────────────────
    // 팝업 열기 / 닫기
    // ────────────────────────────────────────────
    private void OpenPopup(PopupEntry entry)
    {
        if (entry.panelType == PanelType.Inventory)
        {
            csInvenManager.Instance.OpenInventory();
            entry.isOpen = true;
            currentOpenEntry = entry;
        }
        else
        {
            entry.popup.SetActive(true);
            entry.isOpen = true;
            currentOpenEntry = entry;
            entry.popup.transform.SetAsLastSibling();
        }

        // 패널 열릴 때 커서 해제
        SetGameCursor(true);

        Debug.Log($"[ButtonPnl] {entry.panelType} 팝업 열림");
    }

    private void ClosePopup(PopupEntry entry)
    {
        if (entry.panelType == PanelType.Inventory)
        {
            csInvenManager.Instance.CloseInventory();
        }
        else
        {
            entry.popup.SetActive(false);
        }

        entry.isOpen = false;
        if (currentOpenEntry == entry)
            currentOpenEntry = null;

        // 모든 패널 닫혔을 때만 커서 잠금
        bool anyOpen = popupEntries.Exists(e => e.isOpen);
        if (!anyOpen)
            SetGameCursor(false);

        Debug.Log($"[ButtonPnl] {entry.panelType} 팝업 닫힘");
    }

    // ────────────────────────────────────────────
    // 외부 호출용 공개 메서드
    // ────────────────────────────────────────────
    public void OpenPopupByType(PanelType type)
    {
        PopupEntry entry = popupEntries.Find(e => e.panelType == type);
        if (entry != null) OpenPopup(entry);
    }
    public void ClosePopupByType(PanelType type)
    {
        PopupEntry entry = popupEntries.Find(e => e.panelType == type);
        if (entry != null) ClosePopup(entry);
    }
    public void CloseAllPopups()
    {
        foreach (var entry in popupEntries)
        {
            if (entry.isOpen) ClosePopup(entry);
        }
    }
    public PanelType? GetCurrentOpenPanel()
    {
        return currentOpenEntry?.panelType;
    }
}
