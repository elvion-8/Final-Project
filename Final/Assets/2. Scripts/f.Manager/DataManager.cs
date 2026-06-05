using UnityEngine;

public class DataManager : MonoBehaviour
{
    public scPlayerStat stat {get; private set;} = new scPlayerStat();

    public void Init()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(stat);
        PlayerPrefs.SetString("PlayerSaveData",json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if(PlayerPrefs.HasKey("PlayerSaveData"))
        {
            string json = PlayerPrefs.GetString("PlayerSaveData");
            stat = JsonUtility.FromJson<scPlayerStat>(json);
        }
    }
}
