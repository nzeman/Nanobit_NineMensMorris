using DG.Tweening.Core.Easing;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GamePhase { Placing, Moving, MillRemoval }
    public GamePhase currentPhase = GamePhase.Placing;
    public GamePhase gamePhasePriorToMillRemoval = GamePhase.Placing;

    private bool isPlayer1Turn = true; // Track which player's turn it is
    public int maxPiecesPerPlayer = 9;
    private int piecesPlacedPlayer1 = 0;
    private int piecesPlacedPlayer2 = 0;

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
                Debug.Log("Transitioning to Moving Phase.");
            }
        }

        // If a mill was formed, switch to Mill Removal Phase
        if (millFormed)
        {
            gamePhasePriorToMillRemoval = currentPhase;
            currentPhase = GamePhase.MillRemoval;
            Debug.Log("Mill formed! Player must remove an opponent's piece.");
        }
        else
        {
            // Switch the turn after each valid piece placement or move
            isPlayer1Turn = !isPlayer1Turn;
            Debug.Log("Player turn switched. It is now " + (isPlayer1Turn ? "Player 1's" : "Player 2's") + " turn.");
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
        currentPhase = gamePhasePriorToMillRemoval;
        isPlayer1Turn = !isPlayer1Turn; // Switch turns after removal
        Debug.Log("Piece removed. It is now " + (isPlayer1Turn ? "Player 1's" : "Player 2's") + " turn.");
    }
}
