using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameView : ViewBase
{
    [SerializeField] private TMP_Text topText;
    [SerializeField] private TMP_Text turnText;

    public void SetTopText(string topString)
    {
        topText.text = topString;
    }

    public void SetTurnText(string turnString)
    {
        turnText.text = turnString;
    }
}
