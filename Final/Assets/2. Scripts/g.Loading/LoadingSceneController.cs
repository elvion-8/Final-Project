using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    LoadingManager loading;  //loadingManager
    [Header("UI")]
    [SerializeField] private Image backgroundImg;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    private LoadingData loadingData;  //loadingData
    // Start is called before the first frame update
    void Start()
    {
        loading = Managers.loadingManager;
        loadingData = loading.loadingData;
        string targetScene = loading.TargetSceneName;
        if(loadingData != null)
        {
            if(backgroundImg!=null)
            {
                backgroundImg.sprite = loadingData.RandomImg();
            }
            text.text=loadingData.RandomTip();
        }
        loadingBar.value = 0f;

        StartCoroutine(LoadSceneAsyncCoroutine(targetScene));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        canvasGroup.alpha = 0f;
        while(canvasGroup.alpha<1f)
        {
            canvasGroup.alpha += Time.deltaTime * loadingData.fadeSpeed;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        float timer = 0;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(op.progress / 0.9f);
            loadingBar.value = Mathf.MoveTowards(loadingBar.value,progress,Time.deltaTime);

            if(loadingBar.value>=1&&timer >= loadingData.minLoadingTime)
            {
                while(canvasGroup.alpha>0f)
                {
                    canvasGroup.alpha -= Time.deltaTime * loadingData.fadeSpeed;
                    yield return null;
                }
                op.allowSceneActivation = true;
            }
        }
    }
}
