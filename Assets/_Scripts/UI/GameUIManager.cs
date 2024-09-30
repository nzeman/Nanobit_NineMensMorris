using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : CanvasManagerBase
{

    #region Singleton
    private static GameUIManager _Instance;
    public static GameUIManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<GameUIManager>();
            return _Instance;
        }
    }
    #endregion
    
    public GameView gameView;
    public GameEndView endView;

}

