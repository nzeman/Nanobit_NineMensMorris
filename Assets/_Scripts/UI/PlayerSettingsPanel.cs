using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages player settings such as name input and color selection.
/// </summary>
public class PlayerSettingsPanel : MonoBehaviour
{
    public List<PlayerColorPicker> colorPickerButtons = new List<PlayerColorPicker>();
    public TMP_Text playerNameText;

    public string selectedColorId;
    public TMP_InputField nameInputField;

    public bool isPlayer1 = true;
    public PlayerSettingsPanel otherPlayerSettingsPanel;

    private string nameBeforeEdit;
    [SerializeField] private TMP_Text duplicateNamesWarningText;

    [SerializeField] private Image editNameImage;

    void Start()
    {
        nameInputField.text = PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName;
        OnColorSelected(PlayerProfile.Instance.GetGamePlayerData(isPlayer1).colorId);
        nameInputField.onEndEdit.AddListener(delegate { OnEndEditName(); });
        nameInputField.onSelect.AddListener(delegate { OnBeginEditName(); });
        duplicateNamesWarningText.gameObject.SetActive(false);
        editNameImage.gameObject.SetActive(true);
    }

    public void OnBeginEditName()
    {
        nameBeforeEdit = nameInputField.text;
        editNameImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles name editing and prevents duplicate player names.
    /// </summary>
    public void OnEndEditName()
    {
        editNameImage.gameObject.SetActive(true);
        if (nameInputField.text == PlayerProfile.Instance.GetGamePlayerData(!isPlayer1).playerName)
        {
            Debug.Log("Players cannot have the same name!");
            nameInputField.text = nameBeforeEdit;
            duplicateNamesWarningText.gameObject.SetActive(true);
            StartCoroutine(HideWarningAfterDelay());
        }
        else
        {
            PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName = nameInputField.text;
            PlayerProfile.Instance.SavePlayerProfile();
        }
    }

    private IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);
        duplicateNamesWarningText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles color selection for the player and updates the UI accordingly.
    /// </summary>
    public void OnColorSelected(string _colorId)
    {
        foreach (var item in otherPlayerSettingsPanel.colorPickerButtons)
        {
            item.button.interactable = true;
            item.cannotSelectImage.gameObject.SetActive(false);
        }

        selectedColorId = _colorId;
        PlayerColorPicker pcp = colorPickerButtons.Find(x => x.colorId == selectedColorId);
        playerNameText.color = Colors.Instance.GetColorById(pcp.colorId).color;
        foreach (var item in colorPickerButtons)
        {
            item.checkmarkImage.gameObject.SetActive(false);
        }
        pcp.checkmarkImage.gameObject.SetActive(true);

        PlayerProfile.Instance.GetGamePlayerData(isPlayer1).colorId = _colorId;
        PlayerProfile.Instance.SavePlayerProfile();

        PlayerColorPicker pcpUsed = otherPlayerSettingsPanel.colorPickerButtons.Find(x => x.colorId == _colorId);
        pcpUsed.button.interactable = false;
        pcpUsed.cannotSelectImage.gameObject.SetActive(true);
    }
}
