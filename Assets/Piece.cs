using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public SpriteRenderer selectedSprite;

    public void HighlightPiece(bool on)
    {
        selectedSprite.enabled = on;
    }
}
