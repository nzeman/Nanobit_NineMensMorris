using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public BoardPosition boardPosition;

    [Header("Sprite Renderers")]
    public SpriteRenderer mainSprite;
    public SpriteRenderer selectedSprite;
    public SpriteRenderer mildOutline;
    public SpriteRenderer deleteSprite;

    public void OutlinePiece(bool on)
    {
        if (boardPosition == null) return;
        if (on)
        {
            selectedSprite.enabled = on;
        }
        else
        {
            ResetVisual();
        }
        
    }


    public void ScaleUp(bool scale)
    {
        if (boardPosition == null) return;
        if (scale)
        {
            transform.DOScale(1.25f, 1f).SetLoops(-1, LoopType.Yoyo).SetId(GetInstanceID()).SetEase(Ease.InOutExpo);
        }
        else
        {
            ResetVisual();
        }
        
    }

    public void ResetVisual()
    {
        if (boardPosition == null) return;
        transform.localScale = Vector3.one;
        deleteSprite.gameObject.SetActive(false);
        selectedSprite.enabled = false;
        DOTween.Kill(GetInstanceID(), true);
    }

    internal void Color(ColorPair colorPair)
    {
        mainSprite.color = colorPair.color;
    }
}
