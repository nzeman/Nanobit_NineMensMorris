using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        plusOneButton.onClick.AddListener(() =>
        {
            ModifyAmount(1);
        });

        deductOneButton.onClick.AddListener(() =>
        {
            ModifyAmount(-1);
        });

        if (setting == ModifiableSetting.NumberOfRings)
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfRings;
        }
        else
        {
            currentAmount = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;
        }

        ModifyAmount(0);
    }

    public void ModifyAmount(int amount)
    {
        currentAmount += amount;
        if (amount < 0)
        {
            if (currentAmount <= minAmount)
            {
                currentAmount = minAmount;
            }
        }
        else
        {
            if (currentAmount >= maxAmount)
            {
                currentAmount = maxAmount;
            }
        }
        amountText.text = currentAmount.ToString();
        ApplyValues();
    }

    public void ApplyValues()
    {
        if(setting == ModifiableSetting.NumberOfRings)
        {
            PlayerProfile.Instance.playerData.gameRulesData.numberOfRings = currentAmount;
        }
        else
        {
            PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer = currentAmount;
        }
        PlayerProfile.Instance.SavePlayerProfile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
