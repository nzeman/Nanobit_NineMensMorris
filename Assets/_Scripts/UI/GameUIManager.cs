using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
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

    public List<ViewBase> allViews = new List<ViewBase>();
    public GameView gameView;

    public void EnableView(ViewBase vb, float duration = 0.3f)
    {
        foreach (ViewBase view in allViews)
        {
            view.canvasGroup.DOFade(0f, duration).SetUpdate(true);
            view.canvasGroup.interactable = false;
            view.canvasGroup.blocksRaycasts = false;
        }
        vb.canvasGroup.DOFade(1f, duration).SetUpdate(true);
        vb.canvasGroup.interactable = true;
        vb.canvasGroup.blocksRaycasts = true;
    }
  

}

