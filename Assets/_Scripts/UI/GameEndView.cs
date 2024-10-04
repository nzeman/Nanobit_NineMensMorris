using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This view opens when the game ends.
/// </summary>
public class GameEndView : ViewBase
{
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMainMenuButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text winnerPlayerText;
    [SerializeField] private TMP_Text winReasonText;

    [Header("Particles")]
    [SerializeField] private List<ParticleSystem> confettis = new List<ParticleSystem>();
    [SerializeField] private ParticleSystem confettiShower;

    [Header("Strings")]
    [SerializeField] private string winStringWinNoValidMoves;
    [SerializeField] private string winStringWinNoPiecesLeft;

    public override void Start()
    {
        restartButton.onClick.AddListener(GameUIManager.Instance.OnRestartButtonClicked);
        returnToMainMenuButton.onClick.AddListener(GameUIManager.Instance.OnReturnToMainMenuClicked);
    }

    public void StartWinAnimation(bool isPlayer1Winner)
    {
        if (isPlayer1Winner)
        {
            winnerPlayerText.text = PlayerProfile.Instance.GetGamePlayerData(true).playerName;
            winnerPlayerText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;
        }
        else
        {
            winnerPlayerText.text = PlayerProfile.Instance.GetGamePlayerData(false).playerName;
            winnerPlayerText.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
        }
        if(GameManager.Instance.GetWinReason() == GameManager.WinReason.LessThan3PiecesLeft)
        {
            winReasonText.text = winStringWinNoPiecesLeft;
        }
        else
        {
            winReasonText.text = winStringWinNoValidMoves;
        }
        StartCoroutine(WinAnimation());
    }

    public IEnumerator WinAnimation()
    {
        yield return new WaitForSecondsRealtime(.5f);
        confettiShower.Play();
        
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onReachGameEndView);
        int i = 0;
        foreach (var confetti in confettis)
        {
            confetti.Play();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().confettiBlast);
            yield return new WaitForSecondsRealtime(.15f * i);
            i++;
        }

    }
}
