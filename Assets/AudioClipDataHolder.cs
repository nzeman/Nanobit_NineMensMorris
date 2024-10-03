using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipDataHolder", menuName = "Audio/AudioClipDataHolder", order = 1)]
public class AudioClipDataHolder : ScriptableObject
{
    [Header("Music")]
    public AudioClipData mainMenuMusic;
    public AudioClipData gameMusic;

    [Header("SFX")]
    public AudioClipData onPiecePlacedClick;
    public AudioClipData onPieceMove;
    public AudioClipData onIllegalMove;
    public AudioClipData onPieceSelected;
    public AudioClipData onPieceRemovedFromBoardByMill;
    public AudioClipData onMillFormed;
    public AudioClipData winnerJingle;
    public AudioClipData onReachGameEndView;
    public AudioClipData confettiBlast;
}
