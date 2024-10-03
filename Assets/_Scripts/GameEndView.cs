using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndView : ViewBase
{

    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMainMenuButton;
    [SerializeField] private TMP_Text winnerPlayerText;


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
        StartCoroutine(WinAnimation());
    }

    public IEnumerator WinAnimation()
    {
        yield return new WaitForSecondsRealtime(.3f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onReachGameEndView);
    }
}
