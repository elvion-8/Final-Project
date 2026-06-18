using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터의 정신력(Sanity)에 따라 초상화 UI에 시각 효과를 적용하는 컴포넌트.
/// SanityUI_Shader(Shader Graph)로 만든 머티리얼과 짝을 이루어 동작한다.
/// - 정신력이 낮을수록 : 화면 흔들림 ↑, 붉은 색조 ↑, 지직거림(스태틱) ↑
/// </summary>
public class SanityVisualEffect : MonoBehaviour
{
    [Header("UI References")]
    public MeshRenderer portraitRenderer;       // 효과를 입힐 초상화
    public Material sanityMaterial;    // SanityUI_Shader로 만든 "원본" 머티리얼 에셋

    [Header("Shader Properties (최대치 설정)")]
    // 아래 값들은 게임 도중 거의 변하지 않는 "효과 강도 상한값" 성격의 변수들이다.
    [Tooltip("흔들림/깜빡임 속도")] public float shakeSpeed = 10f;
    [Tooltip("흔들림 강도 (0.05~0.1)")] public float shakeAmp = 0.05f;
    [Tooltip("지직거림 선명도")] public float fizzIntensity = 1f;
    [Tooltip("지직거림 노이즈 크기")] public float fizzCrackleScale = 20f;

    [Header("디버그용 (정신력 붙이면 나중에 끄기)")]
    [Range(0f, 1f)] public float debugSanity = 1f;
    public bool useDebugSanity = true;

    // 원본 머티리얼(sanityMaterial)을 직접 수정하면 프로젝트의 에셋 파일까지 바뀌고,
    // 같은 머티리얼을 쓰는 다른 오브젝트에도 영향이 간다.
    // 따라서 런타임에 복제본을 만들어 "이 컴포넌트 전용 인스턴스"에만 효과를 적용한다.
    private Material instancedMaterial;

    // 셰이더 프로퍼티를 이름(string)으로 매번 찾으면 내부 해싱 비용이 발생한다.
    // 시작 시 정수 ID로 한 번만 변환해 캐싱해두면 SetFloat/SetTexture 호출이 더 빠르다.
    private int sanityID;
    private int shakeSpeedID;
    private int shakeAmpID;
    private int fizzIntensityID;
    private int fizzCrackleScaleID;
    private int portraitTexID;        // 캐릭터별 초상화 텍스처 교체용 ID

    void Start()
    {
        // 1) 셰이더 블랙보드에 정의된 프로퍼티 이름을 정수 ID로 변환해 캐싱한다.
        //    여기 적는 문자열은 Shader Graph Blackboard의 "Reference" 이름과 정확히 일치해야 한다.
        sanityID           = Shader.PropertyToID("_Sanity");
        shakeSpeedID       = Shader.PropertyToID("_ShakeSpeed");
        shakeAmpID         = Shader.PropertyToID("_ShakeAmp");
        fizzIntensityID    = Shader.PropertyToID("_FizzIntensity");
        fizzCrackleScaleID = Shader.PropertyToID("_FizzCrackleScale");
        portraitTexID      = Shader.PropertyToID("_PortraitTex");

        // 2) 원본 머티리얼을 복제해 이 컴포넌트 전용 인스턴스로 만든다.
        if (sanityMaterial != null)
        {
            instancedMaterial = new Material(sanityMaterial);

            if (portraitRenderer != null)
            portraitRenderer.material = instancedMaterial;

            // 3) 인스펙터에서 설정한 기본 효과 강도 값들을 셰이더로 한 번 전달한다.
            ApplyBaseShaderProperties();
        }
    }

    void Update()
    {
        if (useDebugSanity && instancedMaterial != null)
            instancedMaterial.SetFloat(sanityID, debugSanity);
    }

    /// <summary>
    /// 인스펙터에 입력된 효과 강도(흔들림/지직거림 관련)를 머티리얼에 적용한다.
    /// "상한값" 성격이라 매 프레임이 아니라 필요할 때만 호출하면 된다.
    /// </summary>
    private void ApplyBaseShaderProperties()
    {
        if (instancedMaterial == null) return;

        instancedMaterial.SetFloat(shakeSpeedID, shakeSpeed);
        instancedMaterial.SetFloat(shakeAmpID, shakeAmp);
        instancedMaterial.SetFloat(fizzIntensityID, fizzIntensity);
        instancedMaterial.SetFloat(fizzCrackleScaleID, fizzCrackleScale);
    }

    /// <summary>
    /// [에디터] 플레이 중 인스펙터 값을 바꾸면 즉시 화면에 반영해준다.
    /// 효과 강도를 눈으로 보며 튜닝할 때 유용하다. (빌드 동작에는 영향 없음)
    /// 주의: instancedMaterial은 Start에서 만들어지므로, 에디트 모드에서 호출되면
    ///       null일 수 있다. ApplyBaseShaderProperties 내부의 null 체크가 이를 막아준다.
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyBaseShaderProperties();
        }
    }

    /// <summary>
    /// 외부(정신력 관리 시스템 등)에서 호출해 현재 정신력 상태를 셰이더에 반영한다.
    /// 정신력 값이 바뀌는 순간에만 호출하면 충분하다.
    /// </summary>
    /// <param name="currentSanity">현재 정신력</param>
    /// <param name="maxSanity">최대 정신력</param>
    public void UpdateSanity(float currentSanity, float maxSanity)
    {
        if (instancedMaterial == null) return;

        // 0으로 나누는 것을 방지 (maxSanity가 0이면 NaN/Infinity가 발생할 수 있다).
        if (maxSanity <= 0f) return;

        // 0~1 범위로 정규화한다. (0 = 정신력 바닥 / 1 = 멀쩡한 상태)
        float sanityRatio = Mathf.Clamp01(currentSanity / maxSanity);

        // 셰이더의 _Sanity 값을 갱신한다.
        // 셰이더 내부에서는 (1 - _Sanity)를 흔들림/붉은 색조에 곱하므로
        // _Sanity가 낮을수록 흔들림과 붉은 기운이 강해지고, 지직거림도 함께 심해진다.
        instancedMaterial.SetFloat(sanityID, sanityRatio);
    }

    /// <summary>
    /// (추가) 캐릭터별로 초상화를 바꿀 때 사용한다.
    /// 이 셰이더는 Image.sprite가 아니라 셰이더의 _PortraitTex를 샘플링하므로,
    /// 초상화를 코드로 교체하려면 Image.sprite가 아니라 이 함수로 텍스처를 넣어야 한다.
    /// </summary>
    public void SetPortrait(Texture portrait)
    {
        if (instancedMaterial == null) return;
        instancedMaterial.SetTexture(portraitTexID, portrait);
    }

    /// <summary>
    /// 런타임에 new Material()로 만든 인스턴스는 자동으로 회수되지 않는다.
    /// 오브젝트가 파괴될 때 명시적으로 Destroy해 메모리 누수를 막는다.
    /// </summary>
    void OnDestroy()
    {
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}
