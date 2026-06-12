using UnityEngine;
using UnityEngine.UI;

public class SanityVisualEffect : MonoBehaviour
{
    public Image portraitImage;
    public Material sanityMaterial;
    private int sanityPropertyID;

    void Start()
    {
        // 셰이더 프로퍼티 ID를 미리 캐싱하여 성능 향상
        sanityPropertyID = Shader.PropertyToID("_Sanity");
        
        // 원본 매테리얼을 손상시키지 않도록 인스턴스화하여 적용
        if (sanityMaterial != null)
        {
            portraitImage.material = new Material(sanityMaterial);
        }
    }

    // 정신력 값을 업데이트하는 함수 (예: 데미지를 받거나 특정 기믹에서 호출)
    public void UpdateSanity(float currentSanity, float maxSanity)
    {
        float sanityRatio = Mathf.Clamp01(currentSanity / maxSanity);
        
        // 셰이더의 _Sanity 프로퍼티 값 업데이트
        portraitImage.material.SetFloat(sanityPropertyID, sanityRatio);
    }
}