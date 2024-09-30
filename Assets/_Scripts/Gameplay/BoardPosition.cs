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

    public int GetIndex()
    {
        return index;
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
    }

    public void OnMouseEnter()
    {
        if (GameManager.Instance.isGamePaused) return;
        if(GameManager.Instance.currentPhase == GameManager.GamePhase.Placing)
        {
            if (!isOccupied)
            {
                Debug.Log("OnMouseEnter" + index);
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
        else if(GameManager.Instance.currentPhase == GameManager.GamePhase.Moving)
        {

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

