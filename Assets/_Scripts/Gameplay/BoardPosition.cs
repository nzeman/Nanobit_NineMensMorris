using System.Collections.Generic;
using UnityEngine;

public class BoardPosition : MonoBehaviour
{
    private int index; // Unique index of this board position
    public bool isOccupied = false;
    public Piece occupyingPiece; // Reference to the piece occupying this position

    // Store adjacent board positions for move validation
    public List<BoardPosition> adjacentPositions = new List<BoardPosition>();

    public SpriteRenderer highlightSpriteRenderer;

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
}
