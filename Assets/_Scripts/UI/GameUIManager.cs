using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : CanvasManagerBase
{

    #region Singleton
    private static GameUIManager _Instance;
    public static GameUIManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<GameUIManager>();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnReturnToMainMenuClicked()
    {
        Debug.Log("OnReturnToMainMenuClicked");
        SceneManager.LoadScene(0);
    }
}

