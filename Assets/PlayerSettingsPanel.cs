using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Button]
    // Start is called before the first frame update
    void Start()
    {
        //nameInputField.gameObject.SetActive(false);

        nameInputField.text = PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName;
        OnColorSelected(PlayerProfile.Instance.GetGamePlayerData(isPlayer1).colorId);
        nameInputField.onEndEdit.AddListener(delegate { OnEndEditName(); });
        nameInputField.onSelect.AddListener(delegate { OnBeginEditName(); });
        duplicateNamesWarningText.gameObject.SetActive(false);
        editNameImage.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginEditName()
    {
        nameBeforeEdit = nameInputField.text;
        editNameImage.gameObject.SetActive(false);
    }

    public void OnEndEditName()
    {
        editNameImage.gameObject.SetActive(true);
        if (isPlayer1)
        {
            if(nameInputField.text == PlayerProfile.Instance.GetGamePlayerData(false).playerName)
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
        else
        {
            // is player 2
            if (nameInputField.text == PlayerProfile.Instance.GetGamePlayerData(true).playerName)
            {
                nameInputField.text = nameBeforeEdit;
                Debug.Log("Players cannot have the same name!");
                duplicateNamesWarningText.gameObject.SetActive(true);
                StartCoroutine(HideWarningAfterDelay());
            }
            else
            {
                PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName = nameInputField.text;
                PlayerProfile.Instance.SavePlayerProfile();
            }
        }
    }

    public IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);
        duplicateNamesWarningText.gameObject.SetActive(false);
    }

    public void OnColorSelected(string _colorId)
    {
        foreach (var item in otherPlayerSettingsPanel.colorPickerButtons)
        {
            item.button.interactable = true;
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

        otherPlayerSettingsPanel.colorPickerButtons.Find(x => x.colorId == _colorId).button.interactable = false;
    }
}
