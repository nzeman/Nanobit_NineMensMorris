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

    [Button]
    // Start is called before the first frame update
    void Start()
    {
        //nameInputField.gameObject.SetActive(false);

        nameInputField.text = PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName;
        OnColorSelected(PlayerProfile.Instance.GetGamePlayerData(isPlayer1).colorId);
        nameInputField.onEndEdit.AddListener(delegate { OnEndEditName(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEndEditName()
    {
        PlayerProfile.Instance.GetGamePlayerData(isPlayer1).playerName = nameInputField.text;
        PlayerProfile.Instance.SavePlayerProfile();
    }

    public void OnColorSelected(string _colorId)
    {
        selectedColorId = _colorId;
        PlayerColorPicker pcp = colorPickerButtons.Find(x => x.colorId == selectedColorId);
        playerNameText.color = pcp.color;
        foreach (var item in colorPickerButtons)
        {
            item.checkmarkImage.gameObject.SetActive(false);
        }
        pcp.checkmarkImage.gameObject.SetActive(true);

        PlayerProfile.Instance.GetGamePlayerData(isPlayer1).colorId = _colorId;
        PlayerProfile.Instance.SavePlayerProfile();
    }
}
