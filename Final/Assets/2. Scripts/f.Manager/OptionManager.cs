using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 옵션 패널 UI에서 사운드(SoundManager) 및 카메라(CameraMove) 설정을 조절하는 전용 매니저
/// Inspector에서 public 슬라이더/토글/텍스트 요소를 직접 연결하여 동작합니다.
/// </summary>
public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; }

    [Header("사운드 UI 연결 (Inspector 직접 드래그)")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;

    [Header("사운드 텍스트 표시 (선택 사항)")]
    public Text masterVolText;
    public Text bgmVolText;
    public Text sfxVolText;

    [Header("카메라 UI 연결 (Inspector 직접 드래그)")]
    public Slider cameraSensitivitySlider;
    public Text cameraSensitivityText;
    public Slider fovSlider;
    public Text fovText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        RefreshUIValues();
        BindUIEvents();
    }

    private void OnDisable()
    {
        UnbindUIEvents();
    }

    private SoundManager GetSoundManager()
    {
        SoundManager sound = FindObjectOfType<SoundManager>();
        if (sound == null && Managers.Instance != null)
        {
            sound = Managers.Sound;
        }
        return sound;
    }

    /// <summary>
    /// 현재 SoundManager 및 CameraMove/PlayerPrefs의 설정 값으로 UI 슬라이더 및 텍스트 갱신
    /// </summary>
    public void RefreshUIValues()
    {
        SoundManager sound = GetSoundManager();

        if (sound != null)
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = sound.masterVol;
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = sound.bgmVol;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sound.sfxVol;
            if (muteToggle != null) muteToggle.isOn = sound.isMuted;

            UpdateSoundTexts(sound.masterVol, sound.bgmVol, sound.sfxVol);
        }

        CameraMove cam = CameraMove.Instance ?? FindObjectOfType<CameraMove>();
        float sensitivity = cam != null ? cam.mouseSensitivity : PlayerPrefs.GetFloat("mouseSensitivity", PlayerPrefs.GetFloat("Camera_Sensitivity", 65.0f));
        sensitivity = Mathf.Clamp(sensitivity, 30f, 100f);

        if (cameraSensitivitySlider != null)
        {
            float normSens = Mathf.InverseLerp(30f, 100f, sensitivity);
            cameraSensitivitySlider.value = normSens;
        }
        UpdateCameraSensitivityText(sensitivity);

        float currentFov = cam != null ? cam.fov : PlayerPrefs.GetFloat("Camera_FOV", 60.0f);
        if (currentFov < 35f) currentFov = 60.0f;

        if (fovSlider != null)
        {
            float normFov = Mathf.InverseLerp(40f, 100f, currentFov);
            fovSlider.value = normFov;
        }
        UpdateFovText(currentFov);
    }

    /// <summary>
    /// 슬라이더 및 토글 이벤트 동적 바인딩
    /// </summary>
    private void BindUIEvents()
    {
        UnbindUIEvents();

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        if (muteToggle != null) muteToggle.onValueChanged.AddListener(SetMute);
        if (cameraSensitivitySlider != null) cameraSensitivitySlider.onValueChanged.AddListener(SetCameraSensitivity);
        if (fovSlider != null) fovSlider.onValueChanged.AddListener(SetFOV);
    }

    private void UnbindUIEvents()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(SetBgmVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        if (muteToggle != null) muteToggle.onValueChanged.RemoveListener(SetMute);
        if (cameraSensitivitySlider != null) cameraSensitivitySlider.onValueChanged.RemoveListener(SetCameraSensitivity);
        if (fovSlider != null) fovSlider.onValueChanged.RemoveListener(SetFOV);
    }

    #region Public UI Callbacks (Inspector OnValueChanged 이벤트에서 직접 호출 가능)

    /// <summary>
    /// 마스터 볼륨 설정 (0.0 ~ 1.0)
    /// </summary>
    public void SetMasterVolume(float val)
    {
        SoundManager sound = GetSoundManager();
        if (sound != null)
        {
            sound.SetMasterVolume(val);
        }
        if (masterVolText != null)
        {
            masterVolText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }
    }

    /// <summary>
    /// BGM 볼륨 설정 (0.0 ~ 1.0)
    /// </summary>
    public void SetBgmVolume(float val)
    {
        SoundManager sound = GetSoundManager();
        if (sound != null)
        {
            sound.SetBgmVolume(val);
        }
        if (bgmVolText != null)
        {
            bgmVolText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }
    }

    /// <summary>
    /// SFX 볼륨 설정 (0.0 ~ 1.0)
    /// </summary>
    public void SetSfxVolume(float val)
    {
        SoundManager sound = GetSoundManager();
        if (sound != null)
        {
            sound.SetSfxVolume(val);
        }
        if (sfxVolText != null)
        {
            sfxVolText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }
    }

    /// <summary>
    /// 음소거(Mute) 설정
    /// </summary>
    public void SetMute(bool isMuted)
    {
        SoundManager sound = GetSoundManager();
        if (sound != null)
        {
            sound.SetMute(isMuted);
        }
    }

    /// <summary>
    /// 카메라 마우스/스틱 감도 설정 (0.0 ~ 1.0 슬라이더 입력 ➔ 30 ~ 100 감도 변환)
    /// </summary>
    public void SetCameraSensitivity(float val)
    {
        float realSensitivity = (val <= 1.0f) ? Mathf.Lerp(30f, 100f, val) : Mathf.Clamp(val, 30f, 100f);

        CameraMove cam = CameraMove.Instance ?? FindObjectOfType<CameraMove>();
        if (cam != null)
        {
            cam.SetSensitivity(realSensitivity);
        }
        else
        {
            PlayerPrefs.SetFloat("mouseSensitivity", realSensitivity);
            PlayerPrefs.SetFloat("Camera_Sensitivity", realSensitivity);
            PlayerPrefs.Save();
        }

        UpdateCameraSensitivityText(realSensitivity);
    }

    /// <summary>
    /// 카메라 FOV(화각) 설정 (0.0 ~ 1.0 슬라이더 입력 ➔ 40 ~ 100도 화각 변환)
    /// </summary>
    public void SetFOV(float val)
    {
        float realFov = (val <= 1.0f) ? Mathf.Lerp(40f, 100f, val) : Mathf.Clamp(val, 35f, 110f);

        CameraMove cam = CameraMove.Instance ?? FindObjectOfType<CameraMove>();
        if (cam != null)
        {
            cam.ApplyFOV(realFov);
        }
        else
        {
            if (Camera.main != null) Camera.main.fieldOfView = realFov;
            PlayerPrefs.SetFloat("Camera_FOV", realFov);
            PlayerPrefs.Save();
        }

        UpdateFovText(realFov);
    }

    #endregion

    private void UpdateSoundTexts(float master, float bgm, float sfx)
    {
        if (masterVolText != null) masterVolText.text = $"{Mathf.RoundToInt(master * 100)}%";
        if (bgmVolText != null) bgmVolText.text = $"{Mathf.RoundToInt(bgm * 100)}%";
        if (sfxVolText != null) sfxVolText.text = $"{Mathf.RoundToInt(sfx * 100)}%";
    }

    private void UpdateCameraSensitivityText(float sensitivity)
    {
        if (cameraSensitivityText != null)
        {
            cameraSensitivityText.text = $"{Mathf.RoundToInt(sensitivity)}";
        }
    }

    private void UpdateFovText(float fovVal)
    {
        if (fovText != null)
        {
            fovText.text = $"{Mathf.RoundToInt(fovVal)}°";
        }
    }
}
