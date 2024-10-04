using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a position on the game board in Nine Men's Morris. 
/// Manages whether the position is occupied, the piece occupying it, 
/// adjacent positions, and visual effects such as highlighting and hover effects.
/// </summary>
public class BoardPosition : MonoBehaviour
{
    #region Fields

    [SerializeField] private int index; // Unique index of this board position
    public bool isOccupied = false;
    public Piece occupyingPiece; // Reference to the piece occupying this position
    public List<BoardPosition> adjacentPositions = new List<BoardPosition>();
    [SerializeField] private SpriteRenderer highlightSpriteRenderer;
    [SerializeField] private SpriteRenderer onHoveredSpriteRenderer;

    #endregion

    #region Initialization
    public void SetIndex(int i)
    {
        index = i;
        name = i.ToString();
    }

    #endregion

    #region Occupation & Positioning

    /// <summary>
    /// Marks the position as occupied by the specified piece and updates the visuals.
    /// </summary>
    public void OccupyPosition(Piece piece)
    {
        isOccupied = true;
        occupyingPiece = piece;
        ResetVisual();
    }


    public void ClearPosition()
    {
        isOccupied = false;
        occupyingPiece = null;
    }

    public bool IsAdjacent(BoardPosition other)
    {
        return adjacentPositions.Contains(other);
    }

    #endregion

    #region Highlighting & Hover Effects

    /// <summary>
    /// Highlights the board position when enabled, and animates the highlight with a fade effect.
    /// </summary>
    /// <param name="on">Determines whether the position should be highlighted (true) or not (false).</param>
    public void HighlightBoardPosition(bool on)
    {
        highlightSpriteRenderer.enabled = on;
        DOTween.Kill(highlightSpriteRenderer.GetInstanceID(), true);
        if (on)
        {
            highlightSpriteRenderer.color = new Color(1f, 1f, 1f, .2f);
            highlightSpriteRenderer.DOFade(1f, .8f).SetLoops(-1, LoopType.Yoyo).SetId(highlightSpriteRenderer.GetInstanceID()).SetEase(Ease.InOutSine);
        }
    }

    /// <summary>
    /// Changes the visual appearance of the board position when the mouse hovers over it, 
    /// based on the game phase and whether the position is occupied.
    /// </summary>
    public void ChangeVisualsOnMouseOver()
    {
        if (GameManager.Instance.CanPlayerInteract() == false)
        {
            ResetVisual();
            return;
        }

        if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Placing)
        {
            if (!isOccupied)
            {
                transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                onHoveredSpriteRenderer.enabled = true;
                if (GameManager.Instance.IsPlayer1Turn())
                {
                    onHoveredSpriteRenderer.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;
                    highlightSpriteRenderer.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId)).color;

                }
                else
                {
                    onHoveredSpriteRenderer.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
                    highlightSpriteRenderer.color = (Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId)).color;
                }
            }
        }
        else if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Moving)
        {
            if (!isOccupied)
            {
                if (PieceManager.Instance.GetSelectedPiecePosition() != null)
                {
                    if (PieceManager.Instance.GetSelectedPiecePosition().IsAdjacent(this))
                    {
                        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    }
                }
            }
        }
    }

    public void OnMouseOver()
    {
        if (GameManager.Instance.IsGamePaused()) return;
        ChangeVisualsOnMouseOver();
    }

    public void OnMouseEnter()
    {
        if (GameManager.Instance.IsGamePaused()) return;
        ChangeVisualsOnMouseOver();
    }

    public void OnMouseExit()
    {
        ResetVisual();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resets the visual appearance of the board position.
    /// </summary>
    public void ResetVisual()
    {
        transform.localScale = Vector3.one;
        onHoveredSpriteRenderer.enabled = false;
        highlightSpriteRenderer.color = Color.white;
    }

    #endregion
}
