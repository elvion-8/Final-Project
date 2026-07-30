using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 게임패드 스틱으로 화면 상의 마우스 커서를 직접 조종하고, A 버튼/트리거로 클릭할 수 있게 해주는 가상 마우스 컨트롤러
/// </summary>
public class VirtualCursorController : MonoBehaviour
{
    public static VirtualCursorController Instance { get; private set; }

    [Header("커서 속도 설정")]
    [Tooltip("스틱 조작 시 커서 이동 속도 (픽셀/초)")]
    public float cursorSpeed = 500f;

    [Header("패드 버튼 설정")]
    [Tooltip("A 버튼(South) 및 Right Trigger 클릭 허용")]
    public bool enableGamepadClick = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        CleanupDuplicateEventSystems();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        CleanupDuplicateEventSystems();
    }

    public static void CleanupDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        if (eventSystems.Length > 1)
        {
            EventSystem keep = EventSystem.current != null ? EventSystem.current : eventSystems[0];
            for (int i = 0; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != keep)
                {
                    Debug.LogWarning($"[EventSystem Cleanup] 중복된 EventSystem 제거: {eventSystems[i].gameObject.name}");
                    Destroy(eventSystems[i].gameObject);
                }
            }
        }
    }

    private void Update()
    {
        // 1. 커서가 화면에 보이고 활성화된 상태(UI / 메뉴 모드)에서만 스틱 커서 동작
        if (Cursor.lockState != CursorLockMode.None) return;

        Vector2 stickInput = Vector2.zero;

        // 게임패드 스틱 및 D-Pad 입력 감지
        if (Gamepad.current != null)
        {
            stickInput = Gamepad.current.leftStick.ReadValue();
            if (stickInput.sqrMagnitude < 0.05f)
            {
                stickInput = Gamepad.current.dpad.ReadValue();
            }
        }

        // 2. 스틱 입력이 있을 경우 OS/하드웨어 마우스 포인터 좌표 이동 (Warp)
        if (stickInput.sqrMagnitude > 0.01f && Mouse.current != null)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 newPos = currentPos + stickInput * cursorSpeed * Time.unscaledDeltaTime;

            newPos.x = Mathf.Clamp(newPos.x, 0f, Screen.width);
            newPos.y = Mathf.Clamp(newPos.y, 0f, Screen.height);

            Mouse.current.WarpCursorPosition(newPos);
        }

        // 3. 게임패드 A 버튼(South) 또는 Right Trigger 누름 시 마우스 클릭 시뮬레이션
        if (enableGamepadClick && Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                SimulateLeftClick();
            }
        }
    }

    /// <summary>
    /// 현재 마우스 커서 위치의 UI 요소에 마우스 좌클릭 이벤트 발송
    /// </summary>
    public void SimulateLeftClick()
    {
        if (EventSystem.current == null) return;

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width / 2f, Screen.height / 2f);

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePos,
            button = PointerEventData.InputButton.Left
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject target = results[0].gameObject;

            // UI PointerDown, PointerUp, PointerClick 이벤트 실행
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);

            // InputField나 Selectable 요소 선택 활성화
            Selectable selectable = target.GetComponentInParent<Selectable>();
            if (selectable != null)
            {
                selectable.Select();
            }
        }
    }
}
