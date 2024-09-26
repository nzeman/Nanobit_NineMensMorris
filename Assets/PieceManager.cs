using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public GameObject piecePrefabPlayer1;
    public GameObject piecePrefabPlayer2;

    private bool isPlayer1Turn = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.GetComponent<BoardPosition>() != null) 
            {
                BoardPosition position = hit.collider.GetComponent<BoardPosition>();
                if (!position.isOccupied)
                {
                    PlacePiece(position);
                }
            }
        }
    }

    void PlacePiece(BoardPosition position)
    {
        GameObject piecePrefab = isPlayer1Turn ? piecePrefabPlayer1 : piecePrefabPlayer2;
        GameObject piece = Instantiate(piecePrefab, position.transform.position, Quaternion.identity);

        position.OccupyPosition(piece); 
        isPlayer1Turn = !isPlayer1Turn;
    }
}

