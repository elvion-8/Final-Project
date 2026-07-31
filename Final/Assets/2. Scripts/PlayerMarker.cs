using UnityEngine;
using TMPro;

public class PlayerMarker : MonoBehaviour
{
    [Header("Resources 경로 및 오브젝트 설정")]
    public string resourcePath = "PlayerMarker";

    public GameObject playerMarker;

    [Header("닉네임 텍스트 설정")]
    public TextMeshPro nicknameTextMesh;
    public float heightOffset = 2.4f;

    [Tooltip("싱글플레이어 환경 마커 표시 여부")]
    public bool alwaysShowInSingleplayer = false;

    private PhotonView pv;
    private string playerNickname = "";

    void Start()
    {
        pv = GetComponent<PhotonView>();

        InitMarkerFromResources();
        InitNicknameText();
    }

    void InitMarkerFromResources()
    {
        // 인스펙터 할당이 없고 Resources 경로가 입력된 경우 로드
        if (playerMarker == null && !string.IsNullOrEmpty(resourcePath))
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab != null)
            {
                playerMarker = Instantiate(prefab, transform);
                playerMarker.name = "PlayerMarker_Resources";
            }
            else
            {
                Debug.LogWarning($"[PlayerMarker] Resources/{resourcePath} 경로에서 마커 프리팹을 찾지 못했습니다. 인스펙터 할당 또는 Resources 폴더에 해당 프리팹을 생성해주세요.");
            }
        }

        if (playerMarker != null)
        {
            if (playerMarker.GetComponent<Billboard>() == null)
            {
                playerMarker.AddComponent<Billboard>();
            }

            playerMarker.SetActive(false);
        }
    }

    void InitNicknameText()
    {
        if (playerMarker == null) return;

        // 1. 자식 오브젝트에서 TextMesh 자동 탐색
        if (nicknameTextMesh == null)
        {
            nicknameTextMesh = playerMarker.GetComponentInChildren<TextMeshPro>();
        }

        // 2. 포톤 닉네임 연동
        UpdateNicknameString();

        // 3. 텍스트 설정
        if (nicknameTextMesh != null)
        {
            nicknameTextMesh.text = playerNickname;
        }
    }

    /// 네트워크 접속자 닉네임 가져오기
    private void UpdateNicknameString()
    {
        if (pv != null && pv.owner != null && !string.IsNullOrEmpty(pv.owner.NickName))
        {
            playerNickname = pv.owner.NickName;
        }
        else if (PhotonNetwork.inRoom && !string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            playerNickname = PhotonNetwork.playerName;
        }
        else
        {
            playerNickname = "Player";
        }
    }

    void LateUpdate()
    {
        if (playerMarker == null) return;

        bool isMultiplayer = PhotonNetwork.inRoom;
        bool shouldShow = alwaysShowInSingleplayer || isMultiplayer;

        if (shouldShow)
        {
            if (!playerMarker.activeSelf)
            {
                playerMarker.SetActive(true);
                UpdateNicknameString();
                if (nicknameTextMesh != null)
                {
                    nicknameTextMesh.text = playerNickname;
                }
            }
            //마커 높이 설정
            if (playerMarker.transform.parent == transform)
            {
                playerMarker.transform.localPosition = Vector3.up * heightOffset;
            }
            else
            {
                playerMarker.transform.position = transform.position + Vector3.up * heightOffset;
            }
        }
        else
        {
            if (playerMarker.activeSelf)
                playerMarker.SetActive(false);
        }
    }
}
