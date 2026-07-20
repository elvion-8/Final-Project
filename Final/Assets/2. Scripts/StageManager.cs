using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    //���ӵ� �÷��̾� ���� ǥ���� Text UI �׸� ���� ���۷��� (Text ������Ʈ ���� ���۷���)
    public Text txtConnect;

    //���� �α׸� ǥ���� Text UI �׸� ���� ���۷��� ����
    public Text txtLogMsg;

    //채팅을 표시할 Text UI 항목 연결 레퍼런스

    public InputField chatInputField;

    private bool isChatting = false; // 현재 채팅 입력 중인지 체크

    //RPC ȣ���� ���� PhotonView ���� ���۷���
    PhotonView pv;

    //�÷����� ���� ��ġ ���� ���۷���
    private Transform[] playerPos;

    //���� ��� 
    private Transform[] EnemySpawnPoints;

    //���� ��
    private bool gameEnd;

    csButtonManager buttonManager;
    private Transform tempPos;
    private EnemyCtrl boss;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        playerPos = GameObject.Find("PlayerSpawnPoint").GetComponentsInChildren<Transform>();
        if (GameObject.Find("tempPos") != null)
        {
            tempPos = GameObject.Find("tempPos").GetComponent<Transform>();
        }
        GameObject bossCtrl = GameObject.Find("TempEnemy");
        if(bossCtrl!=null) {boss=bossCtrl.GetComponent<EnemyCtrl>();}
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        if(canvas != null){
        chatInputField = canvas.transform.Find("LogSystem/InputChat").GetComponent<InputField>();
        txtConnect = canvas.transform.Find("LogSystem/InputChat/Chat").GetComponent<Text>();
        txtLogMsg = canvas.transform.Find("LogSystem/PanelLogMsg/ChatLog").GetComponent<Text>();
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //�÷��̾ �����ϴ� �Լ� ȣ��
        StartCoroutine(this.CreatePlayer());

        //���� Ŭ����κ��� ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.isMessageQueueRunning = true;

        //�뿡 ������ �� ���� ������ ������ ���
        GetConnectPlayerCount();

        // �α� �޽����� ����� ���ڿ� ����
        string msg = "\n\t<color=#00ff00>["
            + PhotonNetwork.player.NickName
            + "] Connected</color>";
        if (PhotonNetwork.inRoom)
        {
            pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);
        }
        else
        {
            LogMsg("offline");
        }

        yield return new WaitForSeconds(1.0f);

        chatInputField.onEndEdit.AddListener(delegate { OnEndEditChat(); });
    }

    // ���� �߰�
    // �÷��̾ �����ϴ� �Լ�
    IEnumerator CreatePlayer()
    {
        // ������ �׽�Ʈ�� ���Ͽ� �÷��̾� ���� ����Ʈ�� 2���̴� ���� ���� ���� �ο����� �°� ���� ����Ʈ��
        // �� �����ο��� ���� ����

        //���� ������ �� ������ �޾ƿ�(���۷��� ����)
        Room currRoom = PhotonNetwork.room;

        // �׽�Ʈ�� ���� object �迭
        object[] ex = new object[3];
        ex[0] = 3;
        ex[1] = 4;
        ex[2] = 5;

        //float pos = Random.Range(-100.0f, 100.0f);
        //�����Ʈ��ũ�� �̿��� ���� ��Ʈ��ũ ��ü�� ������ ���� Resources ���� �ȿ� �ּ��� �̸��� ���ڷ� ���� �ؾ��Ѵ�. 
        //PhotonNetwork.Instantiate( "MainPlayer", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0 );
        GameObject player;
        if (PhotonNetwork.inRoom)
        {
            player = PhotonNetwork.Instantiate("MainPlayer", playerPos[currRoom.PlayerCount].position, playerPos[currRoom.PlayerCount].rotation, 0, ex);

            player.name = "Player";
        }
        else
        {
            Debug.LogWarning("포톤 서버에 접속되어 있지 않습니다. 로컬(유니티 내장) Instantiate를 실행합니다.");

            // Resources 폴더에 있는 "MainPlayer" 프리팹을 로드하여 일반 생성
            GameObject playerPrefab = Resources.Load<GameObject>("MainPlayer");
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, tempPos.position, tempPos.rotation);
                player.name = "Player";
            }


        }
        // ���� �̸����� �����ؾ� �巳�� ���� ����(DestructionRay ��ũ��Ʈ ����)


        //PhotonNetwork.InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
        //�� �Լ��� PhotonNetwork.Instantiate�� ���������� ��Ʈ��ũ �� �������� ���ÿ� ������Ű����, Master Client �� ���� �� ���� ����.
        //������ ������ ������Ʈ�� PhotonView ������Ʈ�� Owner�� Scene�� �ȴ�.
        yield return null;
    }

    //�� ������ ������ ��ȸ�ϴ� �Լ�
    void GetConnectPlayerCount()
    {
        //���� ������ �� ������ �޾ƿ�(���۷��� ����)
        Room currRoom = PhotonNetwork.room;

        if (PhotonNetwork.inRoom)
        {
            txtConnect.text = PhotonNetwork.room.PlayerCount.ToString()
                              + "/"
                              + PhotonNetwork.room.MaxPlayers.ToString();
        }
        else
        {
            txtConnect.text = "0/0 (Offline)";
        }
    }

    // 포톤 추가
    // 룸 나가기 버튼 클릭 이벤트에 연결될 함수
    public void OnClickExitRoom()
    {
        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n\t<color=#ff0000>["
            + PhotonNetwork.player.NickName
            + "] Disconnected</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();

        //(!) 서버에 통보한 후 룸에서 나가려는 클라이언트가 생성한 모든 네트워크 객체및 RPC를 제거하는 과정 진행(포톤 서버에서 진행)
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        // �÷��̾� ID (���� ����), �̸�, Ŀ���� �Ӽ�
        Debug.Log(newPlayer.ToStringFull());
        // �뿡 ���� ������ ������ display
        GetConnectPlayerCount();

    }

    void OnEndEditChat()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Chatting();
        }
    }


    // ���� �߰�
    //��Ʈ��ũ �÷��̾ ���� �����ų� ������ �������� ��� ȣ��Ǵ� �ݹ� �Լ�
    void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {
        // �뿡 ���� ������ ������ display
        GetConnectPlayerCount();
    }

    //�뿡�� ���� ������� �� ȣ��Ǵ� �ݹ� �Լ� ( (!) ���� �� ������ ȣ�� )
    void OnLeftRoom()
    {
        // �κ�� �̵�
        SceneManager.LoadScene("ScStartPoint");
    }

    [PunRPC]
    void LogMsg(string msg)
    {
        if (txtLogMsg != null)
        {
            //�α� �޽��� Text UI�� �ؽ�Ʈ�� �������� ǥ��
            txtLogMsg.text = txtLogMsg.text + msg;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !chatInputField.isFocused)
        {
            chatInputField.ActivateInputField();
        }
    }
    void OnConnectionFail(DisconnectCause cause)
    {
        Debug.LogError("���� ���� ����: " + cause);
    }

    void Chatting()
    {
        // 1. 네트워크 연결 상태 확인
        if (!PhotonNetwork.inRoom)
        {
            Debug.Log("방에 연결되어 있지 않습니다! 로비로 돌아갑니다.");
            return;
        }
        if (string.IsNullOrEmpty(chatInputField.text.Trim())) return;

        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n\t<color=#0000ff>["
                     + PhotonNetwork.player.NickName
                     + ":" + chatInputField.text
                     + "] </color>";

        // [수정] pv.RPC 대신 PhotonNetwork 전체 혹은 권한 체크 후 전송
        if (pv != null)
        {
            // 씬 오브젝트일 경우를 대비해 모두에게 전송 가능하도록 설정되어 있는지 확인
            pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);
        }

        chatInputField.text = "";
        chatInputField.DeactivateInputField(); // 커서가 다시 깜빡이게 만듦
    }
    void OnBossDie()
    {
        if(boss.hp<=0) Managers.loadingManager.LoadScene("ScStage2", LoadingType.GameToGame);
    }
}
