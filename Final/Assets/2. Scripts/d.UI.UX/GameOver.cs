using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    //public Image gameOver;
    private PlayerCtrl _player;
    private GameObject gameOverPnl;
    private Image gameOverImg;
    public float alpha = 0f;
    private GameObject reStartGO;
    private GameObject menuGO;
    private Button reStartBtn;
    private Button menuBtn;

    void Awake()
    {

    }
    void Start()
    {

    }

    IEnumerator GameOverView()
    {
        float alpha = 0f;
        Color currentColor = gameOverImg.color;
        currentColor.a = alpha;
        gameOverImg.color = currentColor;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 1.0f;

            if (alpha > 1f) alpha = 1f;

            currentColor.a = alpha;
            gameOverImg.color = currentColor;
            yield return null;
        }
        reStartGO.SetActive(true);
        menuGO.SetActive(true);
    }
    public void OnGameOver()
    {
        if (player.hp <= 0)
        {
            gameOverPnl.SetActive(true);
            StartCoroutine(GameOverView());
        }
    }
    public void ReStart()
    {
        PlayerStatController.ClearRunBuffs();
        Managers.loadingManager.LoadScene("ScStartPoint", LoadingType.GameToGame);
    }
    public void Menu()
    {
        PlayerStatController.ClearRunBuffs();
        Managers.loadingManager.LoadScene("ScOpen", LoadingType.MenuToGame);
    }
    public void Init()
    {
        GameObject playerOb = GameObject.FindGameObjectWithTag("Player");
        if (playerOb != null) _player = playerOb.GetComponent<PlayerCtrl>();
        GameObject gameOverOb = GameObject.FindGameObjectWithTag("GameOver");
        if (gameOverOb != null) gameOverPnl = gameOverOb.transform.Find("GameOverPnl").gameObject;
        if (gameOverPnl != null)
        {
            gameOverImg = gameOverPnl.GetComponent<Image>();
            reStartGO = GameObject.FindGameObjectWithTag("GameOver").transform.Find("ReStartBtn").gameObject;
            menuGO = GameObject.FindGameObjectWithTag("GameOver").transform.Find("MenuBtn").gameObject;
            reStartBtn = reStartGO.GetComponent<Button>();
            menuBtn = menuGO.GetComponent<Button>();
        }

        if (reStartBtn != null) reStartBtn.onClick.AddListener(ReStart);
        if (menuBtn != null) menuBtn.onClick.AddListener(Menu);
    }
    public PlayerCtrl player
    {
        get
        {
            if(_player==null)
            {
                GameObject playerOb = GameObject.FindGameObjectWithTag("Player");
                if(playerOb != null)
                {
                    _player = playerOb.GetComponent<PlayerCtrl>();
                }
            }
            return _player;
        }
    }
}
