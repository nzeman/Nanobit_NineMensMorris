using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MainMenuCanvasManager : CanvasManagerBase
{
    #region Singleton
    private static MainMenuCanvasManager _Instance;
    public static MainMenuCanvasManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<MainMenuCanvasManager>();
            return _Instance;
        }
    }
    #endregion

    public MainMenuView mainMenuView;
    public SettingsView settingsView;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        mainMenuView.playButton.onClick.AddListener(OnPlayButtonClicked);
        mainMenuView.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        mainMenuView.exitButton.onClick.AddListener(OnExitButtonClicked);
        EnableView(mainMenuView, 0f);
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("OnPlayButtonClicked");
        SceneLoadingManager.Instance.LoadGameplayScene();
        //SceneManager.LoadScene(1);
    }

    public void OnSettingsButtonClicked()
    {
        Debug.Log("OnSettingsButtonClicked");
        EnableView(settingsView);
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("OnExitButtonClicked");
        Debug.Log("Application Quit is called!");
        Application.Quit();
    }
}
