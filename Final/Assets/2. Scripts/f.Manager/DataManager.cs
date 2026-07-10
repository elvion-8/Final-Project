using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    public scPlayerStat stat { get; private set; } = new scPlayerStat();

    // 로그인 성공 후 서버에서 받아와 채워질 유저 고유 ID 번호
    private int currentUserId = -1;
    private string serverUrl = "http://192.168.0.44:8080/";

    // 인게임에서 데이터를 싱글톤 변수에 주입하는 유일한 통로
    public void SetUserData(int userId, scPlayerStat incomingStats)
    {
        currentUserId = userId;
        stat = incomingStats; // 서버에서 받아온 스탯을 내 진짜 스탯 변수에 대입!

        Debug.Log($"[DataManager] 유저 ID {currentUserId}번의 서버 데이터를 정상적으로 로드했습니다.");
    }

    public void SaveGame()
    {
        if (currentUserId == -1)
        {
            Debug.LogError("[SaveGame 실패] 로그인된 유저 ID가 없습니다. 오프라인 상태이거나 로그인을 안 했습니다.");
            return;
        }

        // 서버로 보낼 때 사용할 임시 보따리 데이터 구조 생성
        SavePayload payload = new SavePayload
        {
            userId = currentUserId,
            stats = stat
        };

        string json = JsonUtility.ToJson(payload);

        // 코루틴 실행을 위해 임시로 MonoBehaviour의 StartCoroutine을 활용하거나,
        // 이 스크립트가 붙은 오브젝트 컴포넌트 자체에서 코루틴을 돌립니다.
        StartCoroutine(SaveRequestRoutine(json));
    }

    private IEnumerator SaveRequestRoutine(string jsonPayload)
    {
        Debug.Log("[SaveGame] 서버로 데이터를 저장하는 중...");

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "save.php", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 앞서 작성한 가위질 방어 코드를 똑같이 적용합니다.
                string rawText = request.downloadHandler.text;
                if (rawText.Contains("{"))
                {
                    rawText = rawText.Substring(rawText.IndexOf('{')).Trim();
                }

                // 성공 메시지 확인용 임시 파싱
                PhpResponse response = JsonUtility.FromJson<PhpResponse>(rawText);
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

    public void LoadGame(scPlayerStat incomingStats)
    {
        if (incomingStats != null)
        {
            stat = incomingStats;
            Debug.Log("[DataManager] 로그인 성공으로 서버 스탯 로드 완료!");
        }
        else
        {
            Debug.LogError("[DataManager] 서버로부터 전달받은 스탯 데이터가 null입니다!");
        }
    }
}

[System.Serializable]
public class SavePayload
{
    public int userId;
    public scPlayerStat stats;
}