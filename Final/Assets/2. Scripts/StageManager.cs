using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    //접속된 플레이어 수를 표시할 Text UI 항목 연결 레퍼런스 (Text 컴포넌트 연결 레퍼런스)
    public Text txtConnect;

    //접속 로그를 표시할 Text UI 항목 연결 레퍼런스 선언
    public Text txtLogMsg;
    
    //RPC 호출을 위한 PhotonView 연결 레퍼런스
    PhotonView pv;

    //플레어의 생성 위치 저장 레퍼런스
    private Transform[] playerPos;

    //스폰 장소 
    private Transform[] EnemySpawnPoints;

    //게임 끝
    private bool gameEnd;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        playerPos = GameObject.Find("PlayerSpawnPoint").GetComponentsInChildren<Transform>();

    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //플레이어를 생성하는 함수 호출
        StartCoroutine(this.CreatePlayer());

        //포톤 클라우드로부터 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.isMessageQueueRunning = true;

        //룸에 입장한 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();

        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n\t<color=#00ff00>["
            + PhotonNetwork.player.NickName
            + "] Connected</color>";

        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        yield return new WaitForSeconds(1.0f);

    }

    // 포톤 추가
    // 플레이어를 생성하는 함수
    IEnumerator CreatePlayer()
    {
        // 지금은 테스트를 위하여 플레이어 스폰 포인트가 2개이다 따라서 차후 접속 인원수에 맞게 스폰 포인트와
        // 총 접속인원의 수를 제한

        //현재 입장한 룸 정보를 받아옴(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;

        // 테스트를 위한 object 배열
        object[] ex = new object[3];
        ex[0] = 3;
        ex[1] = 4;
        ex[2] = 5;

        //float pos = Random.Range(-100.0f, 100.0f);
        //포톤네트워크를 이용한 동적 네트워크 객체는 다음과 같이 Resources 폴더 안에 애셋의 이름을 인자로 전달 해야한다. 
        //PhotonNetwork.Instantiate( "MainPlayer", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0 );
        GameObject player = PhotonNetwork.Instantiate("MainPlayer", playerPos[currRoom.PlayerCount].position, playerPos[currRoom.PlayerCount].rotation, 0, ex);

        // 기존 이름으로 변경해야 드럼통 폭파 가능(DestructionRay 스크립트 참조)
        player.name = "Player";

        //PhotonNetwork.InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
        //이 함수도 PhotonNetwork.Instantiate와 마찬가지로 네트워크 상에 프리팹을 동시에 생성시키지만, Master Client 만 생성 및 삭제 가능.
        //생성된 프리팹 오브젝트의 PhotonView 컴포넌트의 Owner는 Scene이 된다.
        yield return null;
    }

    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        //현재 입장한 룸 정보를 받아옴(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;

        //현재 룸의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 다음 Text UI 항목에 출력
        txtConnect.text = currRoom.PlayerCount.ToString()
            + "/"
            + currRoom.MaxPlayers.ToString();
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        // 플레이어 ID (접속 순번), 이름, 커스텀 속성
        Debug.Log(newPlayer.ToStringFull());
        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();
       
    }

    // 포톤 추가
    //네트워크 플레이어가 룸을 나가거나 접속이 끊어졌을 경우 호출되는 콜백 함수
    void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {
        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();
    }

    //룸에서 접속 종료됐을 때 호출되는 콜백 함수 ( (!) 과정 후 포톤이 호출 )
    void OnLeftRoom()
    {
        // 로비로 이동
        SceneManager.LoadScene("scNetLobby");
    }

    [PunRPC]
    void LogMsg(string msg)
    {
        if (txtLogMsg != null)
        {
            //로그 메시지 Text UI에 텍스트를 누적시켜 표시
            txtLogMsg.text = txtLogMsg.text + msg;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    

    void OnConnectionFail(DisconnectCause cause)
    {
        Debug.LogError("연결 실패 원인: " + cause);
    }
}
