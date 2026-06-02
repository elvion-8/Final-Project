using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private SoundData _soundData;

    [Range(0f, 1f)]
    public float masterVol = 1.0f;
    [Range(0f, 1f)]
    public float bgmVol = 1.0f;
    [Range(0f, 1f)]
    public float sfxVol = 1.0f;
    
    public bool isMuted = false;

    private Dictionary<string, SoundClipInfo> _bgmClips = new Dictionary<string, SoundClipInfo>();
    private Dictionary<string, SoundClipInfo> _sfxClips = new Dictionary<string, SoundClipInfo>();
    private Coroutine _bgmFadeCoroutine;

    public void Init()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            _bgmSource = sources[0];
            _sfxSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            _bgmSource = sources[0];
            _sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _sfxSource = gameObject.AddComponent<AudioSource>();
        }

        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        _soundData = Resources.Load<SoundData>("SoundData");
        if (_soundData != null)
        {
            Debug.Log($"[사운드 체크] BGM 개수: {_soundData.bgmClips.Count}, SFX 개수: {_soundData.sfxClips.Count}");
            _bgmClips.Clear();
            foreach (var clipInfo in _soundData.bgmClips)
            {
                if (!string.IsNullOrEmpty(clipInfo.name))
                    _bgmClips[clipInfo.name] = clipInfo;
            }

            _sfxClips.Clear();
            foreach (var clipInfo in _soundData.sfxClips)
            {
                if (!string.IsNullOrEmpty(clipInfo.name))
                    _sfxClips[clipInfo.name] = clipInfo;
            }
        }
        else
        {
            Debug.LogWarning("sound error");
        }

        LoadVolumeSettings();
        UpdateAudioSourceVolumes();
    }

    private void LoadVolumeSettings()
    {
        masterVol = PlayerPrefs.GetFloat("Sound_MasterVolume", 1.0f);
        bgmVol = PlayerPrefs.GetFloat("Sound_BGMVolume", 1.0f);
        sfxVol = PlayerPrefs.GetFloat("Sound_SFXVolume", 1.0f);
        isMuted = PlayerPrefs.GetInt("Sound_IsMuted", 0) == 1;
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("Sound_MasterVolume", masterVol);
        PlayerPrefs.SetFloat("Sound_BGMVolume", bgmVol);
        PlayerPrefs.SetFloat("Sound_SFXVolume", sfxVol);
        PlayerPrefs.SetInt("Sound_IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateAudioSourceVolumes()
    {
        if (isMuted)
        {
            if (_bgmSource != null) _bgmSource.volume = 0f;
            if (_sfxSource != null) _sfxSource.volume = 0f;
        }
        else
        {
            if (_bgmSource != null)
            {
                float clipMultiplier = 1f;
                if (_bgmSource.clip != null)
                {
                    foreach (var info in _bgmClips.Values)
                    {
                        if (info.clip == _bgmSource.clip)
                        {
                            clipMultiplier = info.volumeMultiplier;
                            break;
                        }
                    }
                }
                _bgmSource.volume = masterVol * bgmVol * clipMultiplier;
            }

            if (_sfxSource != null)
            {
                _sfxSource.volume = masterVol * sfxVol;
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (isMuted) return;

        float playVolume = masterVol * sfxVol;
        _sfxSource.PlayOneShot(clip, playVolume);
    }

    public void PlaySFX(string soundName)
    {
        if (isMuted) return;
        if (_sfxClips.TryGetValue(soundName, out SoundClipInfo info))
        {
            if (info.clip != null)
            {
                float playVolume = masterVol * sfxVol * info.volumeMultiplier;
                _sfxSource.PlayOneShot(info.clip, playVolume);
            }
        }
        else
        {
            Debug.LogWarning($"SFX clip not found: {soundName}");
        }
    }

    public void PlayBGM(string soundName, float fadeDuration = 0.5f)
    {
        if (_bgmClips.TryGetValue(soundName, out SoundClipInfo info))
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }
            _bgmFadeCoroutine = StartCoroutine(CoFadePlayBGM(info.clip, info.volumeMultiplier, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"BGM clip not found: {soundName}");
        }
    }

    public void StopBGM(float fadeDuration = 0.5f)
    {
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
        }
        _bgmFadeCoroutine = StartCoroutine(CoFadePlayBGM(null, 1.0f, fadeDuration));
    }

    private System.Collections.IEnumerator CoFadePlayBGM(AudioClip newClip, float targetVolumeMultiplier, float duration)
    {
        float targetVol = isMuted ? 0f : (masterVol * bgmVol * targetVolumeMultiplier);

        // Fade out
        if (_bgmSource.isPlaying && _bgmSource.clip != null)
        {
            float startVol = _bgmSource.volume;
            if (duration > 0f)
            {
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    _bgmSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                    yield return null;
                }
            }
            _bgmSource.Stop();
        }

        _bgmSource.clip = newClip;

        // Fade in
        if (newClip != null)
        {
            _bgmSource.Play();
            if (duration > 0f)
            {
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    _bgmSource.volume = Mathf.Lerp(0f, targetVol, t / duration);
                    yield return null;
                }
            }
            _bgmSource.volume = targetVol;
        }
        _bgmFadeCoroutine = null;
    }

    public void SetMasterVolume(float vol)
    {
        masterVol = Mathf.Clamp01(vol);
        UpdateAudioSourceVolumes();
        SaveVolumeSettings();
    }

    public void SetBgmVolume(float vol)
    {
        bgmVol = Mathf.Clamp01(vol);
        UpdateAudioSourceVolumes();
        SaveVolumeSettings();
    }

    public void SetSfxVolume(float vol)
    {
        sfxVol = Mathf.Clamp01(vol);
        UpdateAudioSourceVolumes();
        SaveVolumeSettings();
    }

    public void MasterVolUp()
    {
        SetMasterVolume(masterVol + 0.1f);
    }

    public void MasterVolDown()
    {
        SetMasterVolume(masterVol - 0.1f);
    }

    public void sfxVolUp()
    {
        SetSfxVolume(sfxVol + 0.1f);
    }

    public void SfxVolDown()
    {
        SetSfxVolume(sfxVol - 0.1f);
    }

    public void BgmVolUp()
    {
        SetBgmVolume(bgmVol + 0.1f);
    }

    public void BgmVolDown()
    {
        SetBgmVolume(bgmVol - 0.1f);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        UpdateAudioSourceVolumes();
        SaveVolumeSettings();
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        UpdateAudioSourceVolumes();
        SaveVolumeSettings();
    }
}
