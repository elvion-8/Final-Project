using UnityEngine;
using System.Collections;

public class Portal : Photon.MonoBehaviour
{
    [Header("Portal Animation")]
    [Tooltip("포탈이 커지는 데 걸리는 시간")]
    public float scaleDuration = 1.0f;

    // 로딩 씬으로 넘길 다음 씬 데이터
    public static string TargetSceneName { get; private set; }

    // 중복 호출 방지용
    private static bool isTransitioning = false;
    private string myNextScene;

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = photonView.instantiationData;
        if (instantiationData != null && instantiationData.Length > 0)
        {
            myNextScene = (string)instantiationData[0];
        }
    }

    private void Start()
    {
        isTransitioning = false;
        StartCoroutine(ScaleUpRoutine());
    }

    private IEnumerator ScaleUpRoutine()
    {
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 targetScale = Vector3.one * 1.0f;
        
        transform.localScale = startScale;

        float timer = 0f;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / scaleDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PhotonView playerPv) && playerPv.isMine)
            {
                if (!string.IsNullOrEmpty(myNextScene))
                {
                    photonView.RPC("RPC_RequestSceneTransition", PhotonTargets.MasterClient, myNextScene);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_RequestSceneTransition(string sceneName)
    {
        if (PhotonNetwork.isMasterClient && !isTransitioning)
        {
            isTransitioning = true;
            photonView.RPC("RPC_ExecuteSceneTransition", PhotonTargets.All, sceneName);
        }
    }

    [PunRPC]
    private void RPC_ExecuteSceneTransition(string sceneName)
    {
        PhotonNetwork.isMessageQueueRunning = false;
        Managers.loadingManager.LoadScene(sceneName, LoadingType.GameToGame);
    }
}