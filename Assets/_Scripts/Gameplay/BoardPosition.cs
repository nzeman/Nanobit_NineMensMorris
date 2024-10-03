using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class BoardPosition : MonoBehaviour
{
    private int index; // Unique index of this board position
    public bool isOccupied = false;
    public Piece occupyingPiece; // Reference to the piece occupying this position

    public List<BoardPosition> adjacentPositions = new List<BoardPosition>();

    public SpriteRenderer highlightSpriteRenderer;
    public SpriteRenderer onHoveredSpriteRenderer;

    public void SetIndex(int i)
    {
        index = i;
        name = i.ToString();
    }

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

    public void OnMouseOver()
    {
        if (GameManager.Instance.isGamePaused) return;
        ChangeVisualsOnMouseOver();
    }

    public void OnMouseEnter()
    {
        if (GameManager.Instance.isGamePaused) return;
        ChangeVisualsOnMouseOver();
    }

    public void ChangeVisualsOnMouseOver()
    {
        if (GameManager.Instance.canInteract == false)
        {
            ResetVisual();
            return;
        }

        if (GameManager.Instance.currentPhase == GameManager.GamePhase.Placing)
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
        else if (GameManager.Instance.currentPhase == GameManager.GamePhase.Moving)
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


    public void OnMouseExit()
    {
        ResetVisual();
    }

    public void ResetVisual()
    {
        transform.localScale = Vector3.one;
        onHoveredSpriteRenderer.enabled = false;
        highlightSpriteRenderer.color = Color.white;
    }
}

