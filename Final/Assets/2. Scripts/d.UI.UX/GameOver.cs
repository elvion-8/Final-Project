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
    public GameObject reStartBtn;
    public GameObject menuBtn;

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
            // 중요: 값을 더해줍니다(+=). 
            // 0.1f를 곱하면 대략 10초 동안 서서히 밝아집니다. (속도를 높이려면 숫자를 키우세요. 예: * 2.0f)
            alpha += Time.deltaTime * 1.0f;

            // 1.0f을 넘지 않도록 방지
            if (alpha > 1f) alpha = 1f;

            // 알파값 적용
            currentColor.a = alpha;
            gameOverImg.color = currentColor;

            // 핵심: 한 프레임을 쉬어갑니다. 화면이 갱신되면서 애니메이션이 연출됩니다.
            yield return null;
        }

        // 페이드 인이 완전히 끝난 후 버튼들을 활성화
        reStartBtn.SetActive(true);
        menuBtn.SetActive(true);
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
        Managers.loadingManager.LoadScene(("ScStartPoint"), LoadingType.GameToGame);
    }
    public void Menu()
    {
        Managers.loadingManager.LoadScene(("ScOpen"), LoadingType.MenuToGame);
    }
    public void Init()
    {
        GameObject playerOb = GameObject.FindGameObjectWithTag("Player");
        if (playerOb != null) player = playerOb.GetComponent<PlayerCtrl>();
        gameOverPnl = GameObject.FindGameObjectWithTag("GameOver").transform.Find("GameOverPnl").gameObject;
        if (gameOverPnl != null)
        {
            gameOverImg = gameOverPnl.GetComponent<Image>();
        }
        reStartBtn = GameObject.FindGameObjectWithTag("GameOver").transform.Find("ReStartBtn").gameObject;
        menuBtn = GameObject.FindGameObjectWithTag("GameOver").transform.Find("MenuBtn").gameObject;
    }
}
