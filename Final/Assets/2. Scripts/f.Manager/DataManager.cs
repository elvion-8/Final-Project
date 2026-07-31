using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;

[System.Serializable]
public class SavePayload
{
    public int userId;
    public scPlayerStat stats;
}

[System.Serializable]
public class DataManagerPhpResponse
{
    public bool success;
    public string message;
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public scPlayerStat stat { get; private set; } = new scPlayerStat();

    private int currentUserId = -1;
    [SerializeField] private string serverUrl = "http://192.168.0.44:8080/";

    private string localSaveFilePath;

    // 🔑 AES256 키 및 IV 설정 (32바이트 Key, 16바이트 IV)
    // ⚠️ 실제 출시 시에는 고유한 32자/16자 문자열로 변경하여 사용하세요!
    private static readonly string AesKey = "12345678901234567890123456789012"; // 32 characters (256-bit)
    private static readonly string AesIV = "1234567890123456";                 // 16 characters (128-bit)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 💡 실행 파일(.exe)이 있는 Root 폴더 경로를 가져옵니다.
            string exeFolderPath = Directory.GetParent(Application.dataPath).FullName;

            // 실행 파일 바로 옆 'Saves' 라는 폴더 안에 저장하고 싶을 때
            string saveDirectory = Path.Combine(exeFolderPath, "Saves");

            // 폴더가 없으면 자동 생성
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            // 최종 파일 경로 설정
            localSaveFilePath = Path.Combine(saveDirectory, "offline_player_data.sav");
            Debug.Log($"[DataManager] 저장 파일 경로 설정됨: {localSaveFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUserData(int userId, scPlayerStat incomingStats)
    {
        currentUserId = userId;
        stat = incomingStats;
        Debug.Log($"[DataManager] 유저 ID {currentUserId}번의 서버 데이터를 로드했습니다.");
    }

    #region 통합 Save / Load (온라인 / 오프라인 분기)

    public void SaveGame()
    {
        bool isOffline = SessionManager.Instance != null && SessionManager.Instance.IsOfflineMode;

        if (isOffline)
        {
            SaveLocalGame();
        }
        else
        {
            SaveOnlineGame();
        }
    }

    public void LoadGame(scPlayerStat incomingStats = null)
    {
        bool isOffline = SessionManager.Instance != null && SessionManager.Instance.IsOfflineMode;

        if (isOffline)
        {
            LoadLocalGame();
        }
        else
        {
            LoadOnlineGame(incomingStats);
        }
    }

    #endregion

    #region 암호화 오프라인 로컬 저장 / 불러오기

    private void SaveLocalGame()
    {
        try
        {
            // 1. 객체를 JSON 문자열로 변환
            string json = JsonUtility.ToJson(stat, true);

            // 2. JSON 문자열을 AES256으로 암호화
            string encryptedData = EncryptAES256(json);

            // 3. 암호화된 텍스트를 파일로 저장
            File.WriteAllText(localSaveFilePath, encryptedData, Encoding.UTF8);
            Debug.Log($"🔒 [DataManager] 암호화된 오프라인 데이터 로컬 저장 완료: {localSaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [DataManager] 오프라인 저장 실패: {e.Message}");
        }
    }

    private void LoadLocalGame()
    {
        try
        {
            if (File.Exists(localSaveFilePath))
            {
                // 1. 암호화된 파일 텍스트 읽기
                string encryptedData = File.ReadAllText(localSaveFilePath, Encoding.UTF8);

                // 2. AES256 복호화 진행
                string json = DecryptAES256(encryptedData);

                // 3. JSON 복호화 성공 시 클래스로 역직렬화
                if (!string.IsNullOrEmpty(json))
                {
                    stat = JsonUtility.FromJson<scPlayerStat>(json);
                    Debug.Log("🔓 [DataManager] 암호화된 오프라인 로컬 파일 복호화 & 로드 성공!");
                }
                else
                {
                    Debug.LogError("❌ [DataManager] 데이터 복호화 실패! 파일이 변조되었거나 올바르지 않습니다.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ [DataManager] 로컬 저장 파일이 없습니다. 기본 스탯으로 초기화 후 새로 생성합니다.");
                stat = new scPlayerStat();
                SaveLocalGame(); // 최초 초기 저장
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [DataManager] 오프라인 불러오기 및 복호화 실패 (파일 변조 가능성): {e.Message}");
        }
    }

    #endregion

    #region AES256 암호화 / 복호화 핵심 함수

    private string EncryptAES256(string plainText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(AesKey);
        byte[] ivBytes = Encoding.UTF8.GetBytes(AesIV);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(plainBytes, 0, plainBytes.Length);
                    cs.FlushFinalBlock();
                }
                return System.Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string DecryptAES256(string cipherText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(AesKey);
        byte[] ivBytes = Encoding.UTF8.GetBytes(AesIV);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(System.Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }

    #endregion

    #region 온라인 DB 저장 / 불러오기 (PHP 서버)

    private void SaveOnlineGame()
    {
        if (currentUserId == -1)
        {
            Debug.LogError("[SaveGame 실패] 로그인된 유저 ID가 없습니다.");
            return;
        }

        SavePayload payload = new SavePayload
        {
            userId = currentUserId,
            stats = stat
        };

        string json = JsonUtility.ToJson(payload);
        StartCoroutine(SaveRequestRoutine(json));
    }

    private void LoadOnlineGame(scPlayerStat incomingStats)
    {
        if (incomingStats != null)
        {
            stat = incomingStats;
            Debug.Log("[DataManager] 온라인 서버 스탯 로드 완료!");
        }
        else
        {
            Debug.LogError("[DataManager] 서버로부터 전달받은 스탯 데이터가 null입니다!");
        }
    }

    private IEnumerator SaveRequestRoutine(string jsonPayload)
    {
        Debug.Log("🌐 [SaveGame] 서버로 데이터를 저장하는 중...");

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "save.php", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string rawText = request.downloadHandler.text;

                if (!string.IsNullOrEmpty(rawText) && rawText.Contains("{"))
                {
                    int jsonStartIndex = rawText.IndexOf('{');
                    int jsonEndIndex = rawText.LastIndexOf('}');
                    if (jsonEndIndex > jsonStartIndex)
                    {
                        rawText = rawText.Substring(jsonStartIndex, (jsonEndIndex - jsonStartIndex) + 1).Trim();
                    }
                }

                DataManagerPhpResponse response = JsonUtility.FromJson<DataManagerPhpResponse>(rawText);
                if (response != null)
                {
                    Debug.Log($"[서버 응답] {response.message}");
                }
            }
            else
            {
                Debug.LogError("[SaveGame 실패] 연결 오류: " + request.error);
            }
        }
    }

    #endregion
}