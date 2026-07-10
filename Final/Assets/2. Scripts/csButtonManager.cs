using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

[System.Serializable]
public class PhpResponse
{
    public bool success;
    public string message;
    public int userId;
    public scPlayerStat stats;
}

[System.Serializable]
public class AuthPayload
{
    public string username;
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
        Managers.loadingManager.LoadScene("ScStartPoint",LoadingType.MenuToGame);
    }

    private IEnumerator AuthRequestRoutine(string phpFile, string username, string password, bool isLogin)
    {
        LogMessage("서버와 통신 중...");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            LogMessage("아이디와 비밀번호를 모두 입력해주세요.");
            yield break;
        }

        AuthPayload payload = new AuthPayload { username = username, password = password };
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(serverBaseUrl + phpFile, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string rawText = request.downloadHandler.text;
                Debug.Log("서버가 보낸 원본 텍스트: " + rawText);

                // ✨ 앞뒤 공백과 줄바꿈을 제거하고 정확하게 맨 처음 '{' 부터 잘라냅니다.
                if (!string.IsNullOrEmpty(rawText) && rawText.Contains("{"))
                {
                    int jsonStartIndex = rawText.IndexOf('{');
                    rawText = rawText.Substring(jsonStartIndex).Trim();
                }

                Debug.Log("✂️ 가위질 완료된 텍스트: " + rawText);

                // ⚠️ 반드시 request.downloadHandler.text 대신 가위질된 'rawText'를 넣어야 합니다!
                PhpResponse response = JsonUtility.FromJson<PhpResponse>(rawText);
                LogMessage(response.message);

                if (response.success)
                {
                    if (dataManager == null) dataManager = FindFirstObjectByType<DataManager>();

                    if (isLogin)
                    {
                        if (dataManager != null)
                        {
                            dataManager.SetUserData(response.userId, response.stats);
                        }
                        LogMessage("데이터 로드 성공! 게임을 로드합니다.");
                        yield return new WaitForSeconds(1.0f);
                        LoadScene();
                    }
                    else
                    {
                        LogMessage("회원가입 완료! 자동으로 로그인 중...");
                        yield return new WaitForSeconds(1.0f);
                        StartCoroutine(AuthRequestRoutine("login.php", username, password, true));
                    }
                }
            }
            else
            {
                LogMessage("연결 실패: " + request.error);
            }
        }
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
