using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuCanvasManager : MonoBehaviour
{

    #region Singleton
    private static MainMenuCanvasManager _Instance;
    public static MainMenuCanvasManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<MainMenuCanvasManager>();
            return _Instance;
        }
    }
    #endregion

    public MainMenuView mainMenuView;
    public SettingsView settingsView;
}
