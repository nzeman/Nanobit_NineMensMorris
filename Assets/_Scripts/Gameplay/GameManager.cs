using DG.Tweening.Core.Easing;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    #region Singleton
    private static GameManager _Instance;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<GameManager>();
            return _Instance;
        }
    }
    #endregion

    public enum GamePhase { Placing, Moving, MillRemoval }
    public GamePhase currentPhase = GamePhase.Placing;
    public GamePhase gamePhasePriorToMillRemoval = GamePhase.Placing;

    private bool isPlayer1Turn = true; // Track which player's turn it is
    public int maxPiecesPerPlayer = 9;
    private int piecesPlacedPlayer1 = 0;
    private int piecesPlacedPlayer2 = 0;

    public void Start()
    {
        SetUi();
        GameUIManager.Instance.gameView.SetTurnText("PLAYER 1");
       
    }

    // Called when a piece is placed or moved
    public void PiecePlacedByPlayer(bool millFormed)
    {
        if (currentPhase == GamePhase.Placing)
        {
            // Increment pieces placed for the current player
            if (isPlayer1Turn)
                piecesPlacedPlayer1++;
            else
                piecesPlacedPlayer2++;

            // Check if all pieces have been placed
            if (piecesPlacedPlayer1 >= maxPiecesPerPlayer && piecesPlacedPlayer2 >= maxPiecesPerPlayer)
            {
                currentPhase = GamePhase.Moving;
                GameUIManager.Instance.gameView.SetTopText("Transitioning to Moving Phase");
                Debug.Log("Transitioning to Moving Phase");
                BoardManager.Instance.HideHightlightsFromBoardPositions();
            }

            if (currentPhase == GamePhase.Placing)
            {
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            }
        }

        // If a mill was formed, switch to Mill Removal Phase
        if (millFormed)
        {
            OnMillFormed();
        }
        else
        {
            // Switch the turn after each valid piece placement or move
            isPlayer1Turn = !isPlayer1Turn;

            if (isPlayer1Turn)
            {
                GameUIManager.Instance.gameView.SetTurnText("PLAYER 1");
                Debug.Log("Player 1 turn");
            }
            else
            {
                GameUIManager.Instance.gameView.SetTurnText("PLAYER 2");
                Debug.Log("Player 2 turn");
            }
            SetUi();
        }
    }

    public void OnMillFormed()
    {
        Debug.Log("Mill formed! Player must remove an opponents piece.");
        gamePhasePriorToMillRemoval = currentPhase;
        currentPhase = GamePhase.MillRemoval;
        GameUIManager.Instance.gameView.SetTopText("Mill formed! Player must remove an opponent's piece.");
        BoardManager.Instance.HideHightlightsFromBoardPositions();
        if (IsPlayer1Turn())
        {
            foreach (var piece in PieceManager.Instance.allPieces)
            {
                if (piece.CompareTag("Player2Piece"))
                {
                    piece.HighlightPiece(true);
                }

            }
        }
        else
        {
            foreach (var piece in PieceManager.Instance.allPieces)
            {
                if (piece.CompareTag("Player1Piece"))
                {
                    piece.HighlightPiece(true);
                }

            }
        }
    }

    // Returns true if it's Player 1's turn, false if it's Player 2's turn
    public bool IsPlayer1Turn()
    {
        return isPlayer1Turn;
    }

    public void PieceRemoved()
    {
        Debug.Log("Piece removed");

        if (CheckLossByPieceCount() || (CheckLossByNoValidMoves() && currentPhase == GamePhase.Moving))
        {
            DeclareWinner(IsPlayer1Turn());
            return;
        }

        currentPhase = gamePhasePriorToMillRemoval;
        isPlayer1Turn = !isPlayer1Turn; 

        if (isPlayer1Turn)
        {
            GameUIManager.Instance.gameView.SetTurnText("PLAYER 1");
            SetUi();
            Debug.Log("Player 1 turn");
        }
        else
        {
            GameUIManager.Instance.gameView.SetTurnText("PLAYER 2");
            SetUi();
            Debug.Log("Player 2 turn");
        }

        if (currentPhase == GamePhase.Placing)
        {
            BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
        }

        PieceManager.Instance.UnhighlightAllPieces();
    }


    public void SetUi()
    {
        if (currentPhase == GamePhase.Placing)
        {
            GameUIManager.Instance.gameView.SetTopText("PLACE YOUR PIECES");
        }
        else if (currentPhase == GamePhase.Moving)
        {
            GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECES");
        }
    }

    public bool CheckLossByPieceCount()
    {
        int player1Pieces = 0;
        int player2Pieces = 0;

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag("Player1Piece"))
            {
                player1Pieces++;
            }
            else if (piece.CompareTag("Player2Piece"))
            {
                player2Pieces++;
            }
        }

        if (player1Pieces < 3)
        {
            DeclareWinner(isPlayer1Turn: false);
            return true;
        }
        if (player2Pieces < 3)
        {
            DeclareWinner(isPlayer1Turn: true);
            return true;
        }

        return false;
    }

    public bool CheckLossByNoValidMoves()
    {
        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag(GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece"))
            {
                if (HasAnyValidMove(piece))
                {
                    return false;
                }
            }
        }

        DeclareWinner(isPlayer1Turn: !GameManager.Instance.IsPlayer1Turn());
        return true;
    }

    bool HasAnyValidMove(Piece piece)
    {
        if (PieceManager.Instance.IsFlyingPhaseForCurrentTurnPlayer())
        {
            foreach (var position in BoardManager.Instance.allBoardPositions)
            {
                if (!position.isOccupied)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (var adjacent in piece.boardPosition.adjacentPositions)
            {
                if (!adjacent.isOccupied)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void DeclareWinner(bool isPlayer1Turn)
    {
        string winner = isPlayer1Turn ? "Player 1" : "Player 2";
        Debug.Log(winner + " wins!");
        GameUIManager.Instance.gameView.SetTopText(winner + " wins!!!!!!!!!!!!!!!!!!!!!!!");
    }
}
