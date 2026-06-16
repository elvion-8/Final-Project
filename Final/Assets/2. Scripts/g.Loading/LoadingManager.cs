using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public LoadingData loadingData { get; private set; }
    public string TargetSceneName { get; private set; }

    [SerializeField]
    private LoadingData defaultLoadingData;

    [SerializeField]
    private LoadingData[] datas;

    private Dictionary<LoadingType, LoadingData> _loadingDataDic = new Dictionary<LoadingType, LoadingData>();

    public void Init()
    {
        _loadingDataDic.Clear();
        datas = Resources.LoadAll<LoadingData>("LoadingData");
        Debug.Log($"[LoadingManager] Resources에서 찾은 총 SO 개수: {datas.Length}개");

        foreach (LoadingData data in datas)
        {
            if (_loadingDataDic.ContainsKey(data.loadingType))
            {
                Debug.LogError($"[중복 범인 발견!] 파일 이름: {data.name} | 설정된 타입: {data.loadingType}");
            }
            _loadingDataDic[data.loadingType] = data;
            if (datas.Length > 0)
            {
                defaultLoadingData = datas[0];
            }
        }
    }


    public void LoadScene(string targetScene, LoadingType loadingType)
    {
        TargetSceneName = targetScene;
        if (_loadingDataDic.TryGetValue(loadingType, out LoadingData data))
        {
            loadingData = data;
        }
        else
        {
            Debug.Log("[loadingManager] SO파일이 Resources폴더에 없음");
            loadingData = defaultLoadingData;
        }

        SceneManager.LoadScene("ScLoading");
    }
}
