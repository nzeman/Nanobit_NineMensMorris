using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameView : ViewBase
{
    [SerializeField] private TMP_Text topText;
    [SerializeField] private TMP_Text turnText;


    public PlayerUiPanel player1UiPanel;
    public PlayerUiPanel player2UiPanel;

    [SerializeField] private TMP_Text bottomText;
    private bool isShowingBottomText = false;

    public override void Start()
    {
        if (PlayerProfile.Instance == null) return;

        player1UiPanel.playerNameText.text = PlayerProfile.Instance.GetGamePlayerData(true).playerName;
        player1UiPanel.playerNameText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;

        player2UiPanel.playerNameText.text = PlayerProfile.Instance.GetGamePlayerData(false).playerName;
        player2UiPanel.playerNameText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
    }

    public void SetTopText(string topString)
    {
        topString = topString.ToUpperInvariant();
        topText.text = topString;
    }

    public void ShowBottomText(string textToSet)
    {
        //if (isShowingBottomText)
            //return;

        StartCoroutine(ShowBottomTextCoroutine(textToSet));
    }

    private IEnumerator ShowBottomTextCoroutine(string textToSet)
    {
        bottomText.gameObject.SetActive(true);
        bottomText.text = textToSet;
        isShowingBottomText = true;
        yield return new WaitForSecondsRealtime(1.5f);
        bottomText.gameObject.SetActive(false);
        isShowingBottomText = false;
    }

    public void SetTurnText()
    {
        DOTween.Kill(GetInstanceID());
        if (GameManager.Instance.currentPhase == GameManager.GamePhase.GameEnd) return;

        if (GameManager.Instance.IsPlayer1Turn())
        {
            turnText.text = PlayerProfile.Instance.GetGamePlayerData(true).playerName;
            turnText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;
        }
        else
        {
            turnText.text = PlayerProfile.Instance.GetGamePlayerData(false).playerName;
            turnText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
        }
        turnText.transform.localScale = Vector3.one;
        turnText.transform.DOPunchScale(new Vector3(.3f, .3f, .3f), .4f, 0, 1f).SetId(GetInstanceID());
    }

    public void HideTurnText()
    {
        turnText.gameObject.SetActive(false);
    }
}
