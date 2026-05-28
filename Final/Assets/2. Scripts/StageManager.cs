using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    //���ӵ� �÷��̾� ���� ǥ���� Text UI �׸� ���� ���۷��� (Text ������Ʈ ���� ���۷���)
    public Text txtConnect;

    //���� �α׸� ǥ���� Text UI �׸� ���� ���۷��� ����
    public Text txtLogMsg;
    
    //RPC ȣ���� ���� PhotonView ���� ���۷���
    PhotonView pv;

    //�÷����� ���� ��ġ ���� ���۷���
    private Transform[] playerPos;

    //���� ��� 
    private Transform[] EnemySpawnPoints;

    //���� ��
    private bool gameEnd;

    csButtonManager buttonManager;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        playerPos = GameObject.Find("PlayerSpawnPoint").GetComponentsInChildren<Transform>();

       
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

        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        yield return new WaitForSeconds(1.0f);

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
        GameObject player = PhotonNetwork.Instantiate("MainPlayer", playerPos[currRoom.PlayerCount].position, playerPos[currRoom.PlayerCount].rotation, 0, ex);

        // ���� �̸����� �����ؾ� �巳�� ���� ����(DestructionRay ��ũ��Ʈ ����)
        player.name = "Player";

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

        //���� ���� ������ ���� �ִ� ���� ������ ���� ���ڿ��� ������ ���� Text UI �׸� ���
        txtConnect.text = currRoom.PlayerCount.ToString()
            + "/"
            + currRoom.MaxPlayers.ToString();
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

    }

    

    void OnConnectionFail(DisconnectCause cause)
    {
        Debug.LogError("���� ���� ����: " + cause);
    }
}
