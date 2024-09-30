using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public SpriteRenderer mainSprite;
    public BoardPosition boardPosition;
    public SpriteRenderer selectedSprite;

    public void HighlightPiece(bool on)
    {
        if (boardPosition == null) return;
        selectedSprite.enabled = on;
        if (on)
        {
            transform.DOScale(1.2f, .3f);
        }
        else
        {
            ResetVisual();
        }
        
    }

    public void ResetVisual()
    {
        transform.localScale = Vector3.one;
    }

    internal void Color(ColorPair colorPair)
    {
        mainSprite.color = colorPair.color;
    }
}
