using DG.Tweening;
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
        topString = topString.ToUpperInvariant();
        topText.text = topString;

    }

    public void SetTurnText()
    {
        DOTween.Kill(GetInstanceID());
        if (GameManager.Instance.currentPhase == GameManager.GamePhase.GameEnd) return;

        if (GameManager.Instance.IsPlayer1Turn())
        {
            turnText.text = "PLAYER 1";
            turnText.color = Color.blue;
        }
        else
        {
            turnText.text = "PLAYER 2";
            turnText.color = Color.red;
        }
        turnText.transform.localScale = Vector3.one;
        turnText.transform.DOPunchScale(new Vector3(.3f, .3f, .3f), .4f, 0, 1f).SetId(GetInstanceID());
    }

    public void HideTurnText()
    {
        turnText.gameObject.SetActive(false);
    }
}
