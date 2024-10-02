using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipDataHolder", menuName = "Audio/AudioClipDataHolder", order = 1)]
public class AudioClipDataHolder : ScriptableObject
{
    public AudioClipData mainMenuMusic;
    public AudioClipData gameMusic;
    public AudioClipData onPieceCliked;
}
