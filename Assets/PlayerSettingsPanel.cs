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

    // Start is called before the first frame update
    void Start()
    {
        //nameInputField.gameObject.SetActive(false);
        nameInputField.text = "PLAYER 1";
        OnColorSelected("Red");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnColorSelected(string colorId)
    {
        selectedColorId = colorId;
        PlayerColorPicker pcp = colorPickerButtons.Find(x => x.colorId == selectedColorId);
        playerNameText.color = pcp.color;
        foreach (var item in colorPickerButtons)
        {
            item.checkmarkImage.gameObject.SetActive(false);
        }
        pcp.checkmarkImage.gameObject.SetActive(true);
        
    }
}
