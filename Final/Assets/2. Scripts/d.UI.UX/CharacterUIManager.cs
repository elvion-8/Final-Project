using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    [System.Serializable]
    public class TabData
    {
        public string tabName;
        public Button button;           // 탭 버튼
        public Image activeIndicator;   // 버튼 옆 활성화 표시 이미지
        public GameObject panel;        // 연결된 패널
    }

    [Header("탭 데이터 목록 (위에서부터 순서대로)")]
    public TabData[] tabs;

    private int currentTabIndex = -1;

    void Start()
    {
        // 각 버튼에 클릭 이벤트 등록
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // 클로저 캡처용
            tabs[i].button.onClick.AddListener(() => OnTabSelected(index));
        }

        // 기본적으로 최상단(0번) 탭 선택
        OnTabSelected(0);
    }

    public void OnTabSelected(int selectedIndex)
    {
        if (currentTabIndex == selectedIndex) return;
        currentTabIndex = selectedIndex;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isSelected = (i == selectedIndex);

            // 활성화 표시 이미지 on/off
            if (tabs[i].activeIndicator != null)
                tabs[i].activeIndicator.gameObject.SetActive(isSelected);

            // 패널 on/off
            if (tabs[i].panel != null)
                tabs[i].panel.SetActive(isSelected);
        }
    }
}