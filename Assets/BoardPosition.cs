using System.Collections.Generic;
using UnityEngine;

public class BoardPosition : MonoBehaviour
{
    private int index; // Unique index of this board position
    public bool isOccupied = false;
    public GameObject occupyingPiece; // Reference to the piece occupying this position

    // Store adjacent board positions for move validation
    public List<BoardPosition> adjacentPositions = new List<BoardPosition>();

    public void SetIndex(int i)
    {
        index = i;
    }

    public int GetIndex()
    {
        return index;
    }

    public void OccupyPosition(GameObject piece)
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
