using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the player's color selection in the settings panel.
/// </summary>
public class PlayerColorPicker : MonoBehaviour
{
    public PlayerSettingsPanel playerSettingsPanel;
    public Button button;
    public string colorId;
    public Image checkmarkImage;
    public Image cannotSelectImage;

    void Start()
    {
        button.onClick.AddListener(OnPlayerColorButtonClicked);
        button.image.color = Colors.Instance.GetColorById(colorId).color;
    }

    /// <summary>
    /// Called when the player selects a color button.
    /// </summary>
    public void OnPlayerColorButtonClicked()
    {
        playerSettingsPanel.OnColorSelected(colorId);
    }
}
