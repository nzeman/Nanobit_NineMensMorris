using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Holds Ui references for each player
/// </summary>
public class PlayerUiPanel : MonoBehaviour
{
    public TMP_Text playerNameText;
    public RectTransform piecesSpawnTransform;
    public TMP_Text piecesLeftToPlaceText;
    public bool isPlayer1;
}
