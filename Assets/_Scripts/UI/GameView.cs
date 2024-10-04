using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents the player's main game view during active gameplay, displaying player names, turn info, and game status.
/// </summary>
public class GameView : ViewBase
{
    #region Fields

    [Header("Text")]
    [SerializeField] private TMP_Text topText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text bottomText;

    [Header("Player UI Panels")]
    public PlayerUiPanel player1UiPanel;
    public PlayerUiPanel player2UiPanel;

    [Header("Bools")]
    private bool isShowingBottomText = false;

    #endregion

    #region Initialization

    public override void Start()
    {
        if (PlayerProfile.Instance == null) return;

        player1UiPanel.playerNameText.text = PlayerProfile.Instance.GetGamePlayerData(true).playerName;
        player1UiPanel.playerNameText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;

        player2UiPanel.playerNameText.text = PlayerProfile.Instance.GetGamePlayerData(false).playerName;
        player2UiPanel.playerNameText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Sets the text at the top of the game view.
    /// </summary>
    public void SetTopText(string topString)
    {
        topText.text = topString.ToUpperInvariant();
    }

    /// <summary>
    /// Displays the bottom text briefly with a fade effect.
    /// </summary>
    public void ShowBottomText(string textToSet)
    {
        bottomText.text = textToSet;

        if (!isShowingBottomText)
        {
            StartCoroutine(ShowBottomTextCoroutine(textToSet));
        }
    }

    private IEnumerator ShowBottomTextCoroutine(string textToSet)
    {
        bottomText.gameObject.SetActive(true);
        isShowingBottomText = true;
        yield return new WaitForSecondsRealtime(2.25f);
        bottomText.gameObject.SetActive(false);
        isShowingBottomText = false;
    }

    /// <summary>
    /// Updates the text displaying the current player's turn.
    /// </summary>
    public void SetTurnText()
    {
        DOTween.Kill(GetInstanceID());
        if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.GameEnd) return;

        turnText.gameObject.SetActive(true);
        var isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
        turnText.text = PlayerProfile.Instance.GetGamePlayerData(isPlayer1Turn).playerName;
        turnText.color = Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(isPlayer1Turn).colorId).color;

        turnText.transform.localScale = Vector3.one;
        turnText.transform.DOPunchScale(new Vector3(.15f, .15f, .15f), .3f, 0, 1f).SetId(GetInstanceID());
    }

    /// <summary>
    /// Hides the turn text from the game view.
    /// </summary>
    public void HideTurnText()
    {
        turnText.gameObject.SetActive(false);
    }

    #endregion
}
