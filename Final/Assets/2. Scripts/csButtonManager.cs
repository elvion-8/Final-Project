using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using System;

[System.Serializable]
public class PhpResponse
{
    public bool success;
    public string message;
    public int user_index;
    public string token; // 💡 중복 접속 감시용 토큰 필드 추가
    public scPlayerStat stats;
}

[System.Serializable]
public class AuthPayload
{
    public string user_id;
    public string password;
}

public class csButtonManager : MonoBehaviour
{
    public static bool isMultiplayer = false;

    public GameObject pnlOption;

    [Header("시작(회원가입)용 입력창")]
    public InputField registerIDInputField;
    public InputField registerPWInputField;

    [Header("로그인(기존회원)용 입력창")]
    public InputField loginIDInputField;
    public InputField loginPWInputField;

    [Header("최종 제출용 버튼 2개")]
    public Button btnRegisterSubmit;   // 회원가입 완료 및 시작 버튼
    public Button btnLoginSubmit;      // 로그인 완료 및 시작 버튼

    [Header("실시간 저장 데이터 (확인용)")]
    public string savedRegisterID = "";
    public string savedRegisterPW = "";
    public string savedLoginID = "";
    public string savedLoginPW = "";

    [Header("서버 상태 메시지 출력용 Text")]
    public Text statusText;

    [Header("서버 설정 (Apache 8080 포트)")]
    private string serverBaseUrl = "http://192.168.0.44:8080/";
    private int currentUserId = -1;

    private DataManager dataManager;

    public void OnText()
    {
        if (registerIDInputField != null) savedRegisterID = registerIDInputField.text;
        if (registerPWInputField != null) savedRegisterPW = registerPWInputField.text;
        if (loginIDInputField != null) savedLoginID = loginIDInputField.text;
        if (loginPWInputField != null) savedLoginPW = loginPWInputField.text;

        Debug.Log($"[실시간 수신] 회원가입 ID: {savedRegisterID} | 로그인 ID: {savedLoginID}");
    }

    public void ActivateRegisterID() { if (registerIDInputField != null) registerIDInputField.ActivateInputField(); }
    public void ActivateRegisterPW() { if (registerPWInputField != null) registerPWInputField.ActivateInputField(); }
    public void ActivateLoginID() { if (loginIDInputField != null) loginIDInputField.ActivateInputField(); }
    public void ActivateLoginPW() { if (loginPWInputField != null) loginPWInputField.ActivateInputField(); }

    public void OnClickRegisterSubmit()
    {
        OnText(); // 버튼을 누르는 순간 마지막으로 텍스트 최종 동기화
        StartCoroutine(AuthRequestRoutine("sain.php", savedRegisterID, savedRegisterPW, false));
    }

    public void OnClickLoginSubmit()
    {
        OnText(); // 버튼을 누르는 순간 마지막으로 텍스트 최종 동기화
        StartCoroutine(AuthRequestRoutine("login.php", savedLoginID, savedLoginPW, true));
    }

    public void LoadScene()
    {
        Managers.loadingManager.LoadScene("ScStartPoint", LoadingType.MenuToGame);
    }

    private IEnumerator AuthRequestRoutine(string phpFile, string user_id, string password, bool isLogin)
    {
        LogMessage("서버와 통신 중...");

        if (string.IsNullOrEmpty(user_id) || string.IsNullOrEmpty(password))
        {
            LogMessage("아이디와 비밀번호를 모두 입력해주세요.");
            yield break;
        }

        AuthPayload payload = new AuthPayload { user_id = user_id, password = password };
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(serverBaseUrl + phpFile, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.timeout = 3;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string rawText = request.downloadHandler.text;
                Debug.Log("서버가 보낸 원본 텍스트: " + rawText);

                // 💡 JSON 잘라내기 로직 보강
                if (!string.IsNullOrEmpty(rawText) && rawText.Contains("{"))
                {
                    int jsonStartIndex = rawText.IndexOf('{');
                    int jsonEndIndex = rawText.LastIndexOf('}');

                    if (jsonEndIndex > jsonStartIndex)
                    {
                        rawText = rawText.Substring(jsonStartIndex, (jsonEndIndex - jsonStartIndex) + 1).Trim();
                    }
                }

                Debug.Log("✂️ 가위질 완료된 텍스트: " + rawText);

                // 💡 JSON 파싱 예외 처리 (JSON parse error 방지)
                PhpResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<PhpResponse>(rawText);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSON 파싱 에러] 수신 문자열: {rawText}\n에러 메시지: {ex.Message}");
                    LogMessage("서버 응답 오류! 오프라인 모드로 진입합니다.");
                    StartCoroutine(CoEnterOfflineMode());
                    yield break;
                }

                if (response == null)
                {
                    LogMessage("서버 응답 데이터를 읽을 수 없습니다.");
                    yield break;
                }

                LogMessage(response.message);

                // 온라인 로그인 성공 처리
                if (dataManager == null) dataManager = DataManager.Instance ?? FindFirstObjectByType<DataManager>();

                if (isLogin)
                {
                    if (dataManager != null)
                    {
                        dataManager.SetUserData(response.user_index, response.stats);
                    }

                    if (SessionManager.Instance != null && !string.IsNullOrEmpty(response.token))
                    {
                        SessionManager.Instance.StartSessionCheck(response.user_index, response.token);
                    }

                    LogMessage("온라인 접속 성공! 게임을 로드합니다.");
                    yield return new WaitForSeconds(1.0f);
                    LoadScene();
                }
                else
                {
                    LogMessage("회원가입 완료! 자동으로 로그인 중...");
                    yield return new WaitForSeconds(1.0f);
                    StartCoroutine(AuthRequestRoutine("login.php", user_id, password, true));
                }
            }
            else
            {
                Debug.LogWarning($"[서버 연결 실패] 오류 내용: {request.error}");
                LogMessage("⚠️ 서버와 연결할 수 없습니다. 오프라인 모드로 자동 접속합니다...");

                yield return new WaitForSeconds(1.2f);
                yield return StartCoroutine(CoEnterOfflineMode());
            }
        }
    }


    /// <summary>
    /// 서버 연결 안될 때 자동으로 실행되는 오프라인 접속 코루틴
    /// </summary>
    private IEnumerator CoEnterOfflineMode()
    {
        // 1. SessionManager 오프라인 세션 시작
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.StartOfflineSession();
        }

        // 2. DataManager 로컬 저장 파일 불러오기
        if (DataManager.Instance != null)
        {
            DataManager.Instance.LoadGame();
        }

        LogMessage("오프라인 모드로 접속 중...");
        yield return new WaitForSeconds(0.8f);

        // 3. 게임 씬으로 이동
        LoadScene();
    }

    private void LogMessage(string msg)
    {
        Debug.Log(msg);
        if (statusText != null) statusText.text = msg;
    }

    public void Option()
    {
        if (pnlOption != null)
        {
            pnlOption.SetActive(!pnlOption.activeSelf);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}