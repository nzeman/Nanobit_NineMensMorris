using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Base class for CanvasManagers
/// </summary>
public class CanvasManagerBase : MonoBehaviour
{
    [Header("All views")]
    [SerializeField] private List<ViewBase> allViews = new List<ViewBase>();

    public void EnableView(ViewBase vb, float duration = 0.3f)
    {
        foreach (ViewBase view in allViews)
        {
            view.canvasGroup.DOFade(0f, duration).SetUpdate(true);
            view.canvasGroup.interactable = false;
            view.canvasGroup.blocksRaycasts = false;
        }
        if(vb != null)
        {
            vb.canvasGroup.DOFade(1f, duration).SetUpdate(true);
            vb.canvasGroup.interactable = true;
            vb.canvasGroup.blocksRaycasts = true;
        }
    }
}
