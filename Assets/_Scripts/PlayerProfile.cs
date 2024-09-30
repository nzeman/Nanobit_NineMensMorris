using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using NaughtyAttributes;
using Random = System.Random;
using DG.Tweening.Core.Easing;
using UnityEngine.Experimental.Rendering;
using JetBrains.Annotations;

public class PlayerProfile : MonoBehaviour
{
    #region Singleton
    private static PlayerProfile _Instance;
    public static PlayerProfile Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<PlayerProfile>();
            return _Instance;
        }
    }
    #endregion

    [Header("Keys")]
    [NaughtyAttributes.ReadOnly]
    public string profileKey = "playerProfile";

    [Header("Player")]
    public PlayerData playerData; // reference to player data


    private void Awake()
    {
        if (_Instance == null)
        {
            this.transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);
            _Instance = Instance;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        // load or create new player JSON object
        if (!PlayerPrefs.HasKey(profileKey))
        {
            // Saving just dummy on first ever load
            // this would be replaced by UI wizard at game start / account create to collect basic data
            SavePlayerProfile();
        }
        else
        {
            // Mandatory loads
            playerData = LoadPlayerData();// else load profile
        }
    }

    public void SavePlayerProfile()
    {
        DataManager.SaveData(playerData, profileKey); // save initial profile at game first run
    }

    public PlayerData LoadPlayerData()
    {
        return DataManager.LoadData<PlayerData>(profileKey);
    }

    public GamePlayerData GetGamePlayerData(bool isPlayer1)
    {
        if (isPlayer1)
        {
            return playerData.player1GameData;
        }
        else
        {
            return playerData.player2GameData;
        }

    }

}


[System.Serializable]
public class PlayerData
{
    [Header("Basic Data")]
    public string playerId;

    [Header("Settings Data")]
    public SettingsData settingsData;

    public GamePlayerData player1GameData;
    public GamePlayerData player2GameData;
}

[System.Serializable]
public class SettingsData
{
    public bool isSoundOn = true;
    public bool isVibrationsOn = true;
}

[System.Serializable]
public class GamePlayerData
{
    public string playerName;
    public string colorId;
}


