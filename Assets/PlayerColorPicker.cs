using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerColorPicker : MonoBehaviour
{

    public PlayerSettingsPanel playerSettingsPanel;
    public Button button;
    public string colorId;
    public Image checkmarkImage;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnPlayerColorButtonClicked);
        button.image.color = Colors.Instance.GetColorById(colorId).color;
    }

    public void OnPlayerColorButtonClicked()
    {
        playerSettingsPanel.OnColorSelected(colorId);
    }
   
}
