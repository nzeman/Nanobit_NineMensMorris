using DG.Tweening;
using UnityEngine;

/// <summary>
/// Represents a game piece in Nine Men's Morris. 
/// Manages its visual state, including scaling, highlighting, and color, 
/// as well as its position on the board and interactions with the player.
/// </summary>
public class Piece : MonoBehaviour
{
    #region Fields

    public BoardPosition boardPosition;

    [Header("Sprite Renderers")]
    public SpriteRenderer mainSprite;
    public SpriteRenderer selectedSprite;
    public SpriteRenderer deleteSprite;

    private bool isScalingUp = false;

    #endregion

    #region Visual Management

    /// <summary>
    /// Toggles the outline of the piece, represented by the selected sprite.
    /// </summary>
    /// <param name="on">True to enable the outline, false to disable it.</param>
    public void OutlinePiece(bool on)
    {
        if (boardPosition == null) return;
        selectedSprite.enabled = on;
    }

    /// <summary>
    /// Scales up the piece for a pulsating effect, or resets its scale.
    /// </summary>
    /// <param name="scale">True to scale up the piece, false to reset it.</param>
    public void ScaleUp(bool scale)
    {
        if (boardPosition == null) return;

        if (scale && !isScalingUp)
        {
            isScalingUp = true;
            transform.DOScale(1.28f, .7f).SetLoops(-1, LoopType.Yoyo)
                .SetId(transform.GetInstanceID())
                .SetEase(Ease.InOutExpo)
                .OnKill(() => isScalingUp = false);
        }
        else if (!scale)
        {
            ResetVisual();
        }
    }

    /// <summary>
    /// Resets the piece's visual state, including size and sprite visibility.
    /// </summary>
    public void ResetVisual()
    {
        if (boardPosition == null) return;
        transform.localScale = Vector3.one;
        deleteSprite.gameObject.SetActive(false);
        selectedSprite.enabled = false;
        DOTween.Kill(transform.GetInstanceID(), true);
    }

    #endregion

    #region Color Management

    /// <summary>
    /// Sets the piece's main sprite color based on the provided color pair.
    /// </summary>
    /// <param name="colorPair">The color pair to use for coloring the piece.</param>
    internal void Color(ColorPair colorPair)
    {
        mainSprite.color = colorPair.color;
    }

    #endregion
}
