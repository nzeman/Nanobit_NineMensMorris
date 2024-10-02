using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/AudioClipData", order = 0)]
public class AudioClipData : ScriptableObject
{
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop = false;

    public void ApplyToSource(AudioSource source)
    {
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
    }
}
