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
        gamePhasePriorToMillRemoval = currentPhase;
        currentPhase = GamePhase.MillRemoval;
        Debug.Log("Mill formed! Player must remove an opponent's piece.");
        GameUIManager.Instance.gameView.SetTopText("Mill formed! Player must remove an opponent's piece.");

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

    // Switch back to the normal game phase after a piece is removed
    public void PieceRemoved()
    {
        Debug.Log("Piece removed");
        currentPhase = gamePhasePriorToMillRemoval;
        isPlayer1Turn = !isPlayer1Turn; // Switch turns after removal
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
}
