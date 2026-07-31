using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    private PlayerCtrl _player;
    private GameObject gameOverPnl;
    private Image gameOverImg;
    public float alpha = 0f;
    private GameObject reStartGO;
    private GameObject menuGO;
    private Button reStartBtn;
    private Button menuBtn;

    public void EnsureInitUI()
    {
        GameObject gameOverOb = GameObject.FindGameObjectWithTag("GameOver");
        if (gameOverOb != null)
        {
            Transform pnlTr = gameOverOb.transform.Find("GameOverPnl");
            if (pnlTr != null) gameOverPnl = pnlTr.gameObject;

            Transform reStartTr = gameOverOb.transform.Find("ReStartBtn");
            if (reStartTr != null)
            {
                reStartGO = reStartTr.gameObject;
                reStartBtn = reStartGO.GetComponent<Button>();
            }

            Transform menuTr = gameOverOb.transform.Find("MenuBtn");
            if (menuTr != null)
            {
                menuGO = menuTr.gameObject;
                menuBtn = menuGO.GetComponent<Button>();
            }
        }

        if (gameOverPnl != null && gameOverImg == null)
        {
            gameOverImg = gameOverPnl.GetComponent<Image>();
        }

        if (reStartBtn != null)
        {
            reStartBtn.onClick.RemoveListener(ReStart);
            reStartBtn.onClick.AddListener(ReStart);
        }
        if (menuBtn != null)
        {
            menuBtn.onClick.RemoveListener(Menu);
            menuBtn.onClick.AddListener(Menu);
        }
    }

    IEnumerator GameOverView()
    {
        if (gameOverImg != null)
        {
            float alpha = 0f;
            Color currentColor = gameOverImg.color;
            currentColor.a = alpha;
            gameOverImg.color = currentColor;

            while (alpha < 1f)
            {
                alpha += Time.unscaledDeltaTime * 1.0f;
                if (alpha > 1f) alpha = 1f;

                currentColor.a = alpha;
                gameOverImg.color = currentColor;
                yield return null;
            }
        }
        if (reStartGO != null) reStartGO.SetActive(true);
        if (menuGO != null) menuGO.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnGameOver()
    {
        EnsureInitUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPnl != null)
        {
            gameOverPnl.SetActive(true);
            StartCoroutine(GameOverView());
        }
        else
        {
            Debug.LogWarning("[GameOver] gameOverPnl canvas target not found in current scene.");
        }
    }

    public void ReStart()
    {
        Debug.Log("[GameOver] ReStart Clicked");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        PlayerStatController.ClearRunBuffs();
        Managers.loadingManager.LoadScene("ScStartPoint", LoadingType.GameToGame);
    }

    public void Menu()
    {
        Debug.Log("[GameOver] Menu Clicked");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        PlayerStatController.ClearRunBuffs();
        Managers.loadingManager.LoadScene("ScOpen", LoadingType.MenuToGame);
    }

    public void Init()
    {
        EnsureInitUI();
    }

    public PlayerCtrl player
    {
        get
        {
            if (PlayerCtrl.localPlayer != null)
            {
                _player = PlayerCtrl.localPlayer;
            }
            else if (_player == null)
            {
                foreach (GameObject playerOb in GameObject.FindGameObjectsWithTag("Player"))
                {
                    PhotonView pv = playerOb.GetComponent<PhotonView>();
                    if (!PhotonNetwork.inRoom || (pv != null && pv.isMine))
                    {
                        _player = playerOb.GetComponent<PlayerCtrl>();
                        break;
                    }
                }
            }
            return _player;
        }
    }
}
