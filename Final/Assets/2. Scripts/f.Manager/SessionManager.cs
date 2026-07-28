using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;

[System.Serializable]
public class SessionCheckPayload
{
    public int user_index;
    public string token;
}

[System.Serializable]
public class SessionCheckResponse
{
    public bool success;
    public string message;
}

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("서버 설정")]
    [SerializeField] private string serverBaseUrl = "http://192.168.0.44:8080/";

    [Header("UI 연결 (팝업 창)")]
    [SerializeField] private GameObject sessionPopUpPanel; // 팝업 패널
    [SerializeField] private Button confirmButton;        // 팝업 확인 버튼

    private int currentUserId = -1;
    private string currentToken = "";

    // 💡 백그라운드 루프 관리를 위한 Coroutine 변수
    private Coroutine sessionCheckCoroutine = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경 시에도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitUI();
    }

    // UI 연결 초기화
    private void InitUI()
    {
        // 시작 시 팝업창 숨기기
        if (sessionPopUpPanel != null)
        {
            sessionPopUpPanel.SetActive(false);
        }

        // 팝업창 확인 버튼 클릭 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnClickConfirmBtn);
        }
    }

    // 💡 씬이 바뀔 때마다 실행되는 UI 수동 재연결 함수 (필요시 로그인 컨트롤러에서 호출 가능)
    public void RegisterUI(GameObject popupPanel, Button btn)
    {
        sessionPopUpPanel = popupPanel;
        confirmButton = btn;
        InitUI();
    }

    // 로그인 성공 시 호출
    public void StartSessionCheck(int userIndex, string token)
    {
        if (userIndex <= 0 || string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("⚠️ [SessionManager] 유효하지 않은 userIndex 또는 token입니다.");
            return;
        }

        // 💡 1. 새로 로그인할 때 기존 진행 중인 세션 체크 코루틴이 있다면 확실히 중지
        StopSessionCheck();

        currentUserId = userIndex;
        currentToken = token;

        // 💡 2. 새 코루틴 시작 및 변수 할당
        sessionCheckCoroutine = StartCoroutine(CheckSessionRoutine());
    }

    // 세션 검사 완전히 중지하는 함수
    public void StopSessionCheck()
    {
        if (sessionCheckCoroutine != null)
        {
            StopCoroutine(sessionCheckCoroutine);
            sessionCheckCoroutine = null;
        }
        currentUserId = -1;
        currentToken = "";
    }

    private IEnumerator CheckSessionRoutine()
    {
        while (true)
        {
            // 3초마다 세션 유효성 검사
            yield return new WaitForSeconds(3.0f);

            if (currentUserId == -1 || string.IsNullOrEmpty(currentToken))
                break;

            SessionCheckPayload payload = new SessionCheckPayload
            {
                user_index = currentUserId,
                token = currentToken
            };

            string json = JsonUtility.ToJson(payload);

            //디버그용
            Debug.Log($"[SessionManager] 요청 보냄 -> user_index: {currentUserId} | token: {currentToken}");

            using (UnityWebRequest request = new UnityWebRequest(serverBaseUrl + "check_session.php", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string rawText = request.downloadHandler.text;

                    // 🔍 [디버그 로그 2] check_session.php에서 받은 결과 원본 출력
                    Debug.Log($"[SessionManager] 서버 응답 원본: {rawText}");

                    // JSON 영역 정제
                    if (!string.IsNullOrEmpty(rawText) && rawText.Contains("{"))
                    {
                        int jsonStartIndex = rawText.IndexOf('{');
                        int jsonEndIndex = rawText.LastIndexOf('}');
                        if (jsonEndIndex > jsonStartIndex)
                        {
                            rawText = rawText.Substring(jsonStartIndex, (jsonEndIndex - jsonStartIndex) + 1).Trim();
                        }
                    }

                    SessionCheckResponse response = null;
                    try
                    {
                        response = JsonUtility.FromJson<SessionCheckResponse>(rawText);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SessionManager] JSON 파싱 에러: {e.Message}");
                    }

                    // 중복 접속 감지 시 (토큰 불일치)
                    if (response != null && !response.success)
                    {
                        // 🔍 [디버그 로그 3] 실패 사유(message) 출력
                        Debug.LogWarning($"[중복 접속 감지] 서버 메시지: {response.message}");
                        OnSessionExpired();
                        yield break; // 루프 중단
                    }
                }
            }
        }

        sessionCheckCoroutine = null;
    }

    // 세션 만료 시 팝업 띄우기
    private void OnSessionExpired()
    {
        StopSessionCheck();

        // 시간을 정지하여 인게임 조작 및 업데이트 차단
        Time.timeScale = 0f;

        // 팝업 패널 활성화
        if (sessionPopUpPanel != null)
        {
            sessionPopUpPanel.SetActive(true);
        }
        else
        {
            // 연결된 팝업 UI가 없다면 즉시 확인 버튼 동작 수행
            OnClickConfirmBtn();
        }
    }

    // 팝업창의 '확인' 버튼을 눌렀을 때 실행
    public void OnClickConfirmBtn()
    {
        Time.timeScale = 1f; // 시간 정지 해제

        if (sessionPopUpPanel != null)
        {
            sessionPopUpPanel.SetActive(false); // 팝업 닫기
        }

        // 로그인 화면("ScOpen")으로 이동
        if (Managers.loadingManager != null)
        {
            Managers.loadingManager.LoadScene("ScOpen", LoadingType.MenuToGame);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("ScOpen");
        }
    }
}