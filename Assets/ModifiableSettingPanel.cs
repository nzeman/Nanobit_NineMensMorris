using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ModifiableSetting { NumberOfRings, PiecesPerPlayer }

public class ModifiableSettingPanel : MonoBehaviour
{
    public ModifiableSetting setting;

    [SerializeField] private Button plusOneButton;
    [SerializeField] private Button deductOneButton;
    [SerializeField] private TMP_Text amountText;

    public int currentAmount = 0;
    public int minAmount = 0;
    public int maxAmount = 0;

    [SerializeField] private ModifiableSettingPanel piecesPerPlayerPanel; 

    void Start()
    {
        plusOneButton.onClick.AddListener(() => ModifyAmount(1));
        deductOneButton.onClick.AddListener(() => ModifyAmount(-1));

        if (setting == ModifiableSetting.NumberOfRings)
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfRings;
            UpdateMaxPiecesPerPlayer(); 
        }
        else
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;
        }
        ModifyAmount(0); // Initial update
    }

    public void ModifyAmount(int amount)
    {
        currentAmount += amount;
        currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        amountText.text = currentAmount.ToString();
        ApplyValues();
        UpdateButtonInteractability();

        if (setting == ModifiableSetting.NumberOfRings)
        {
            UpdateMaxPiecesPerPlayer();
        }
    }

    private void UpdateButtonInteractability()
    {
        plusOneButton.interactable = currentAmount < maxAmount;
        UpdateButtonAlpha(plusOneButton, plusOneButton.interactable);

        deductOneButton.interactable = currentAmount > minAmount;
        UpdateButtonAlpha(deductOneButton, deductOneButton.interactable);
    }

    private void UpdateButtonAlpha(Button button, bool interactable)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            Color textColor = buttonText.color;
            textColor.a = interactable ? 1f : 0.1f;
            buttonText.color = textColor;
        }
    }

    private void UpdateMaxPiecesPerPlayer()
    {
        int totalPositions = CalculateTotalPositions(currentAmount);
        int maxPiecesPerPlayer = totalPositions / 2;

        if (piecesPerPlayerPanel != null)
        {
            piecesPerPlayerPanel.minAmount = 3; 
            piecesPerPlayerPanel.maxAmount = maxPiecesPerPlayer; 
            piecesPerPlayerPanel.currentAmount = Mathf.Clamp(piecesPerPlayerPanel.currentAmount, piecesPerPlayerPanel.minAmount, piecesPerPlayerPanel.maxAmount);
            piecesPerPlayerPanel.amountText.text = piecesPerPlayerPanel.currentAmount.ToString();
            piecesPerPlayerPanel.ApplyValues();
            piecesPerPlayerPanel.UpdateButtonInteractability();
        }
        Debug.Log($"Max pieces per player: {maxPiecesPerPlayer}");
    }

    private int CalculateTotalPositions(int rings)
    {
        int positions = 8; // One ring has 8 positions
        for (int i = 2; i <= rings; i++)
        {
            positions += 8; // Each additional ring adds 8 more positions
        }
        return positions;
    }

    public void ApplyValues()
    {
        if (setting == ModifiableSetting.NumberOfRings)
        {
            PlayerProfile.Instance.playerData.gameRulesData.numberOfRings = currentAmount;
        }
        else
        {
            PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer = currentAmount;
        }
        PlayerProfile.Instance.SavePlayerProfile();
    }
}
