using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public GameObject piecePrefabPlayer1;
    public GameObject piecePrefabPlayer2;
    public LayerMask boardLayer;
    public LayerMask pieceLayer;

    private GameManager gameManager;
    [SerializeField] private BoardPosition selectedPiecePosition;
    public List<BoardPosition> allBoardPositions;
    public List<Piece> allPieces;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleBoardPointClick(mousePosition);
        }
    }

    public void HandleBoardPointClick(Vector2 mousePosition)
    {
        RaycastHit2D hitPiece = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, pieceLayer);
        RaycastHit2D hitBoard = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, boardLayer);

        if (gameManager.currentPhase == GameManager.GamePhase.MillRemoval && hitPiece.collider != null)
        {
            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();
            HandleMillRemoval(boardPosition);
        }
        else if (hitBoard.collider != null)
        {
            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();

            if (gameManager.currentPhase == GameManager.GamePhase.Placing)
            {
                HandlePlacingPhase(boardPosition);
            }
            else if (gameManager.currentPhase == GameManager.GamePhase.Moving)
            {
                HandleMovingPhase(boardPosition);
            }
        }
    }

    void HandlePlacingPhase(BoardPosition position)
    {
        if (!position.isOccupied)
        {
            PlacePiece(position, gameManager.IsPlayer1Turn());
            bool millFormed = CheckForMill(position, gameManager.IsPlayer1Turn());
            gameManager.PiecePlacedByPlayer(millFormed);
        }
    }

    void HandleMovingPhase(BoardPosition position)
    {
        // there is nothing selected currently
        if (selectedPiecePosition == null && position.isOccupied)
        {
            if (position.occupyingPiece.CompareTag(gameManager.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece"))
            {
                SelectPiece(position);
            }
        }
        else if (selectedPiecePosition != null && !position.isOccupied)
        {
            if (selectedPiecePosition.IsAdjacent(position))
            {
                MovePiece(selectedPiecePosition, position);
                bool millFormed = CheckForMill(position, gameManager.IsPlayer1Turn());
                gameManager.PiecePlacedByPlayer(millFormed);
            }
            else
            {
                Debug.Log("Invalid move: Not adjacent");
            }
        }
        else if (selectedPiecePosition != null && position.isOccupied)
        {
            if (position.occupyingPiece.CompareTag(gameManager.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece"))
            {
                SelectPiece(position);
            }
        }
    }

    void PlacePiece(BoardPosition position, bool isPlayer1Turn)
    {
        GameObject piecePrefab = isPlayer1Turn ? piecePrefabPlayer1 : piecePrefabPlayer2;
        GameObject piece = Instantiate(piecePrefab, position.transform.position, Quaternion.identity);
        Piece p = piece.GetComponent<Piece>();
        position.OccupyPosition(p);
        allPieces.Add(p);
    }

    void MovePiece(BoardPosition from, BoardPosition to)
    {
        Piece piece = from.occupyingPiece;
        piece.HighlightPiece(false);
        from.ClearPosition();
        to.OccupyPosition(piece);
        piece.transform.position = to.transform.position;
        selectedPiecePosition = null;
    }

    void SelectPiece(BoardPosition position)
    {
        DeselectAllPieces();
        selectedPiecePosition = position;
        position.occupyingPiece.HighlightPiece(true);
        Debug.Log("Selected piece at: " + position.name);
    }

    void DeselectAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.HighlightPiece(false);
        }
    }

    bool CheckForMill(BoardPosition position, bool isPlayer1Turn)
    {
        string playerTag = isPlayer1Turn ? "Player1Piece" : "Player2Piece";
        return CheckMillInLine(position, playerTag, true) || CheckMillInLine(position, playerTag, false);
    }

    bool CheckMillInLine(BoardPosition position, string playerTag, bool checkHorizontal)
    {
        List<BoardPosition> linePositions = new List<BoardPosition> { position };

        foreach (var adjacent in position.adjacentPositions)
        {
            if (IsAlignedInLine(position, adjacent, checkHorizontal) && adjacent.isOccupied && adjacent.occupyingPiece.CompareTag(playerTag))
            {
                linePositions.Add(adjacent);

                foreach (var secondAdjacent in adjacent.adjacentPositions)
                {
                    if (secondAdjacent != position && IsAlignedInLine(adjacent, secondAdjacent, checkHorizontal)
                        && secondAdjacent.isOccupied && secondAdjacent.occupyingPiece.CompareTag(playerTag))
                    {
                        linePositions.Add(secondAdjacent);
                    }
                }
            }
        }

        return linePositions.Count == 3;
    }

    bool IsAlignedInLine(BoardPosition pos1, BoardPosition pos2, bool checkHorizontal)
    {
        if (checkHorizontal)
            return Mathf.Approximately(pos1.transform.position.y, pos2.transform.position.y);
        else
            return Mathf.Approximately(pos1.transform.position.x, pos2.transform.position.x);
    }

    void HandleMillRemoval(BoardPosition position)
    {
        if (position == null || position.occupyingPiece == null)
        {
            Debug.LogWarning("Invalid position or empty occupying piece in mill removal.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager is not set! Cannot proceed with mill removal.");
            return;
        }

        if (position.occupyingPiece.CompareTag(gameManager.IsPlayer1Turn() ? "Player2Piece" : "Player1Piece"))
        {
            if (!IsInMill(position) || AllOpponentPiecesInMill())
            {
                allPieces.Remove(position.occupyingPiece);
                //Destroy(position.occupyingPiece);
                Destroy(position.occupyingPiece.gameObject);
                position.ClearPosition();
                gameManager.PieceRemoved();
            }
            else
            {
                Debug.Log("Cannot remove a piece that is in a mill unless all opponent pieces are in mills.");
            }
        }
        else
        {
            Debug.Log("Piece does not belong to the opponent.");
        }
    }

    bool IsInMill(BoardPosition position)
    {
        string playerTag = position.occupyingPiece.CompareTag("Player1Piece") ? "Player1Piece" : "Player2Piece";

        foreach (var adjacent in position.adjacentPositions)
        {
            if (adjacent.isOccupied && adjacent.occupyingPiece.CompareTag(playerTag))
            {
                foreach (var secondAdjacent in adjacent.adjacentPositions)
                {
                    if (secondAdjacent != position && secondAdjacent.isOccupied && secondAdjacent.occupyingPiece.CompareTag(playerTag))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool AllOpponentPiecesInMill()
    {
        string opponentTag = gameManager.IsPlayer1Turn() ? "Player2Piece" : "Player1Piece";
        foreach (var position in allBoardPositions)
        {
            if (position.isOccupied && position.occupyingPiece.CompareTag(opponentTag) && !IsInMill(position))
            {
                return false;
            }
        }
        return true;
    }
}
