using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    //public Image gameOver;
    public PlayerCtrl player;
    public GameObject gameOverPnl;
    public Image gameOverImg;
    public float alpha = 0f;
    public GameObject reStartGO;
    public GameObject menuGO;
    private Button reStartBtn;
    private Button menuBtn;

    void Awake()
    {


    }
    void Start()
    {
        // GameObject playerOb = GameObject.FindGameObjectWithTag("Player");
        // if (playerOb != null) player = playerOb.GetComponent<PlayerCtrl>();
    }

    void Update()
    {
        // if (player.hp <= 0)
        // {
        //     gameOverPnl.SetActive(true);
        //     StartCoroutine(GameOverView());
        // }
    }
    IEnumerator GameOverView()
    {
        // 시작할 때 알파값을 0으로 초기화 (원하는 시작 값이 있다면 생략 가능)
        float alpha = 0f;
        Color currentColor = gameOverImg.color;
        currentColor.a = alpha;
        gameOverImg.color = currentColor;

        // 알파가 1이 될 때까지 반복
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
        Managers.loadingManager.LoadScene("ScStartPoint", LoadingType.GameToGame);
    }
    public void Menu()
    {
        Managers.loadingManager.LoadScene("ScOpen", LoadingType.MenuToGame);
    }
    public void Init()
    {
        GameObject playerOb = GameObject.FindGameObjectWithTag("Player");
        if (playerOb != null) player = playerOb.GetComponent<PlayerCtrl>();
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
}
