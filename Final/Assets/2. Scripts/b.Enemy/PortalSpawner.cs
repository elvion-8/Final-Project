using UnityEngine;
using System.Collections;

public class PortalSpawner : Photon.MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Resources 폴더에 있는 포탈 프리팹의 이름")]
    public string portalPrefabName = "PortalPrefab";
    
    [Tooltip("플레이어가 이동할 다음 씬의 이름")]
    public string nextSceneName;

    // EnemyDeath 스크립트에서 호출할 함수
    public void SpawnNetworkPortal()
    {
        if (!PhotonNetwork.isMasterClient) return;
        object[] initData = new object[] { nextSceneName };
        PhotonNetwork.Instantiate(portalPrefabName, transform.position, transform.rotation, 0, initData);
    }
}