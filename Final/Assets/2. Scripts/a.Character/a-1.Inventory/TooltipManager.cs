using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public Text tooltipNameText;
    public Text tooltipDescText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // 툴팁이 활성화되어 있으면 마우스 커서 위치를 따라다니도록 설정
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = mousePos;
        }
    }

    public void ShowTooltip(string itemName, string itemDesc)
    {
        tooltipNameText.text = itemName;
        tooltipDescText.text = itemDesc;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
