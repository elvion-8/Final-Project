using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundClipInfo
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumeMultiplier;
}

[CreateAssetMenu(fileName = "SoundData", menuName = "Managers/SoundData")]
public class SoundData : ScriptableObject
{
    public List<SoundClipInfo> bgmClips = new List<SoundClipInfo>();
    public List<SoundClipInfo> sfxClips = new List<SoundClipInfo>();
}
