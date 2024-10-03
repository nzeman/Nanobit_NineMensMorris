using UnityEngine;

[CreateAssetMenu(fileName = "GameUITextData", menuName = "ScriptableObjects/GameUITextData", order = 1)]
public class GameUITextData : ScriptableObject
{
    [TextArea] public string transitioningToMovePhaseText;
    [TextArea] public string millFormedText;
    [TextArea] public string placePieceOnBoardText;
    [TextArea] public string selectPieceText;
    [TextArea] public string formMillText;
    [TextArea] public string flyingPhaseText;
    [TextArea] public string moveToAdjacentSpotText;
    [TextArea] public string moveToHighlightedSpotText;
}
