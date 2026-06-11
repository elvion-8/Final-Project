using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    public Image portraitImage;
    private Material portraitMaterial;

    void Start()
    {
        // 원본 매테리얼이 변경되지 않도록 인스턴스화
        portraitMaterial = new Material(portraitImage.material);
        portraitImage.material = portraitMaterial;
    }

    // 정신력이 변경될 때마다 호출되는 함수
    public void UpdateSanityEffect(float currentSanity, float maxSanity)
    {
        float sanityRatio = currentSanity / maxSanity;
        portraitMaterial.SetFloat("_Sanity", sanityRatio);
    }
}