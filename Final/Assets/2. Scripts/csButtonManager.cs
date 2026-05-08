using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class csButtonManager : MonoBehaviour
{
    public static bool isMultiplayer = false;

    public GameObject pnlOption;
    public void NewGame()
    {
        SceneManager.LoadScene("scstartpoint");
    }

    public void LoadGame()
    {

    }

    public void Option()
    {
        if (pnlOption != null)
        {
            pnlOption.SetActive(!pnlOption.activeSelf);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
