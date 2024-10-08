using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameUIManager that controls the flow of UI in the gameplay scene. Enables and disables the views.
/// </summary>
public class GameUIManager : CanvasManagerBase
{

    #region Singleton
    private static GameUIManager _Instance;
    public static GameUIManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<GameUIManager>();
            return _Instance;
        }
    }
    #endregion
    
    public GameView gameView;
    public GameEndView endView;
    public GamePauseView pauseView;

    public void Start()
    {
        EnableView(gameView);
    }

    public void OnContinueButtonClicked()
    {
        Debug.Log("OnContinueButtonClicked");
        GameManager.Instance.ResumeGameFromPause();
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("OnRestartButtonClicked");
        Time.timeScale = 1f;
        SceneLoadingManager.Instance.LoadGameplayScene();
    }

    public void OnReturnToMainMenuClicked()
    {
        Debug.Log("OnReturnToMainMenuClicked");
        Time.timeScale = 1f;
        SceneLoadingManager.Instance.LoadMainMenu();
    }
}

