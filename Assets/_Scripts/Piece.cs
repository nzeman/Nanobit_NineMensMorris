using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public BoardPosition boardPosition;
    public SpriteRenderer selectedSprite;

    public void HighlightPiece(bool on)
    {
        selectedSprite.enabled = on;
    }

    public void ResetVisual()
    {
        transform.localScale = Vector3.one;
    }

   
}
