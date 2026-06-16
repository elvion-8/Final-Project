using UnityEngine;

public enum LoadingType
{
    MenuToGame,
    GameToGame
}

[CreateAssetMenu(fileName = "NewLoadingData",menuName = "Managers/Loading Data")]
public class LoadingData : ScriptableObject
{
    [Header("Loading Image")]
    public Sprite[] loadingImg;
    public LoadingType loadingType;

    [Header("Tip&Text")]
    [TextArea(3,5)]
    public string[] loadingTip;

    [Header("Settings")]
    public float minLoadingTime = 2.0f;
    public float fadeSpeed = 1.0f;

    public string RandomTip()
    {
        if(loadingTip == null || loadingTip.Length==0) return "Loading....";
        int index = Random.Range(0,loadingTip.Length);
        return loadingTip[index];
    }

    public Sprite RandomImg()
    {
        if(loadingImg==null || loadingImg.Length==0) return null;
        int index = Random.Range(0,loadingImg.Length);
        return loadingImg[index];
    }
}
