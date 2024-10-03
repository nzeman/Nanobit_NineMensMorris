using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : ViewBase
{
    [SerializeField] private Button backButton;

    public override void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void OnBackButtonClicked() 
    {
        Debug.Log("OnBackButtonClicked");
        MainMenuCanvasManager.Instance.EnableView(MainMenuCanvasManager.Instance.mainMenuView);
    
    }
}
