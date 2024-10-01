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

    [SerializeField] private ModifiableSettingPanel numberOfRingsPanel; // Link the number of rings panel

    void Start()
    {
        plusOneButton.onClick.AddListener(() => ModifyAmount(1));
        deductOneButton.onClick.AddListener(() => ModifyAmount(-1));

        // Initialize currentAmount based on the setting type
        if (setting == ModifiableSetting.NumberOfRings)
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfRings;
            UpdateMaxPiecesPerPlayer();
        }
        else if (setting == ModifiableSetting.PiecesPerPlayer)
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;
            UpdateMaxPiecesPerPlayer(); // Ensure max pieces are updated
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

        // If this is the rings panel, update the pieces per player panel based on the new rings value
        if (setting == ModifiableSetting.NumberOfRings)
        {
            UpdateMaxPiecesPerPlayer(); // This updates the piecesPerPlayerPanel
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
        // Get the current number of rings from the rings panel
        int numberOfRings = (numberOfRingsPanel != null) ? numberOfRingsPanel.currentAmount : 3; // Default to 3 rings if not available
        int totalPositions = CalculateTotalPositions(numberOfRings);
        int maxPiecesPerPlayer = totalPositions / 2;

        if (setting == ModifiableSetting.PiecesPerPlayer)
        {
            minAmount = 3; // Set the minimum number of pieces to 3
            maxAmount = maxPiecesPerPlayer; // Set the maximum pieces based on the number of rings

            currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
            amountText.text = currentAmount.ToString();
            ApplyValues();
            UpdateButtonInteractability();
        }

        Debug.Log($"Max pieces per player: {maxPiecesPerPlayer} based on {numberOfRings} rings.");
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
        else if (setting == ModifiableSetting.PiecesPerPlayer)
        {
            PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer = currentAmount;
        }

        PlayerProfile.Instance.SavePlayerProfile();
    }
}
