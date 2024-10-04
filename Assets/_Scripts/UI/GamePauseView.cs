using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This view opens when the player pauses the game.
/// </summary>
public class GamePauseView : ViewBase
{

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMainMenuButton;

    public override void Start()
    {
        continueButton.onClick.AddListener(GameUIManager.Instance.OnContinueButtonClicked);
        restartButton.onClick.AddListener(GameUIManager.Instance.OnRestartButtonClicked);
        returnToMainMenuButton.onClick.AddListener(GameUIManager.Instance.OnReturnToMainMenuClicked);
    }
}
