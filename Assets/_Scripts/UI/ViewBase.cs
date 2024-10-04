using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Serves as a base class for all game views, managing shared functionality like canvas group visibility.
/// </summary>
public class ViewBase : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public virtual void Start()
    {
    }

    public virtual void Update()
    {
    }
}
