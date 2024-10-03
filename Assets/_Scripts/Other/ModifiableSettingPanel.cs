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
            UpdateMaxPiecesPerPlayer(currentAmount);
        }
        else if (setting == ModifiableSetting.PiecesPerPlayer)
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;
        }

        RefreshWithoutModidfying();
        UpdateButtonInteractability();
    }

    public void RefreshWithoutModidfying()
    {
        amountText.text = currentAmount.ToString();
    }

    public void ModifyAmount(int amount)
    {
        currentAmount += amount;

        if (setting == ModifiableSetting.NumberOfRings)
        {
            currentAmount = Mathf.Clamp(currentAmount, 1, 6);
        }
        else
        {
            currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        }

        RefreshWithoutModidfying();
        ApplyValues();
        UpdateButtonInteractability();

        if (setting == ModifiableSetting.NumberOfRings && piecesPerPlayerPanel != null)
        {
            piecesPerPlayerPanel.UpdateMaxPiecesPerPlayer(currentAmount);
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

    public void UpdateMaxPiecesPerPlayer(int numberOfRings)
    {
        int totalPositions = CalculateTotalPositions(numberOfRings);
        int maxPiecesPerPlayer = Mathf.Max(3, totalPositions / 2);

        if (setting == ModifiableSetting.PiecesPerPlayer)
        {
            minAmount = 3;
            maxAmount = maxPiecesPerPlayer;

            currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
            amountText.text = currentAmount.ToString();
            ApplyValues();
            UpdateButtonInteractability();
        }
        //Debug.Log($"Max pieces per player: {maxPiecesPerPlayer} based on {numberOfRings} rings.");
    }

    private int CalculateTotalPositions(int rings)
    {
        int positions = 8;
        for (int i = 2; i <= rings; i++)
        {
            positions += 8;
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
