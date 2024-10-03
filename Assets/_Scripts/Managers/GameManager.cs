using DG.Tweening;
using DG.Tweening.Core.Easing;
using NaughtyAttributes;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    #region Singleton
    private static GameManager _Instance;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<GameManager>();
            return _Instance;
        }
    }
    #endregion

    public enum GamePhase { Placing, Moving, MillRemoval, GameEnd }
    public enum WinReason { LessThan3PiecesLeft, NoValidMovesLeft }

    public GamePhase currentPhase = GamePhase.Placing;
    public GamePhase gamePhasePriorToMillRemoval = GamePhase.Placing;

    public WinReason winReason;

    public bool canInteract = true;
    private bool isPlayer1Turn = true;
    public int maxPiecesPerPlayer = 9;
    public int piecesPlacedPlayer1 = 0;
    public int piecesPlacedPlayer2 = 0;
    public bool isGamePaused = false;
    private bool isPlayer1Winner = false;

    [Header("Camera settings")]
    public Camera mainCamera;
    public float defaultCameraSize = 5f;
    public float cameraOrtoSize = 5f;

    [Header("Time settings")]
    public float timeToMovePieceToBoardPositionInMovingPhase = 0.5f;
    public float timeToMovePieceToBoardInPlacingPhase = 0.5f;

    [Header("Text")]
    public GameUITextData textData;

    public void Start()
    {
        BoardManager.Instance.Initialize();

        maxPiecesPerPlayer = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;

        float ortoSizeCamera = defaultCameraSize + (BoardManager.Instance.GetNumberOfRings() * 1.8F);
        cameraOrtoSize = ortoSizeCamera;
        mainCamera.orthographicSize = ortoSizeCamera;

        SetUi();
        GameUIManager.Instance.gameView.SetTurnText();
        PieceManager.Instance.SpawnAllPiecesAtStart();
        PieceManager.Instance.HighlightNextPieceToPlace();
        canInteract = true;

        AudioManager.Instance.PlayGameMusic(AudioManager.Instance.audioClipDataHolder.gameMusic);
        AudioManager.Instance.StopMainMenuMusic();

        PieceManager.Instance.RefreshPiecesLeftUi();

    }

    public void SwitchingTurn()
    {
        Debug.Log("SWITCHING TURN!");
        isPlayer1Turn = !isPlayer1Turn;
        string debugString = isPlayer1Turn ?
            $"Player 1 turn! :: {PlayerProfile.Instance.GetGamePlayerData(true).playerName}" 
            : 
            $"Player 2 turn! :: {PlayerProfile.Instance.GetGamePlayerData(false).playerName}";

        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onTurnChanged);
        Debug.Log(debugString);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPhase == GamePhase.GameEnd) return;
            if (isGamePaused)
            {
                ResumeGameFromPause();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        isGamePaused = true;
        GameUIManager.Instance.EnableView(GameUIManager.Instance.pauseView);
        Time.timeScale = 0.0000001f;
        //mainCamera.transform.DOMoveY(25f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
        mainCamera.DOOrthoSize(0.0001f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    public void ResumeGameFromPause()
    {
        Time.timeScale = 1f;
        GameUIManager.Instance.EnableView(GameUIManager.Instance.gameView);
        isGamePaused = false;
        //mainCamera.transform.DOMoveY(0f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
        mainCamera.DOOrthoSize(cameraOrtoSize, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    public void OnPieceReachedItsPositionOnBoard(bool millFormed)
    {
        if (currentPhase == GamePhase.Placing)
        {
            if (isPlayer1Turn)
                piecesPlacedPlayer1++;
            else
                piecesPlacedPlayer2++;

            // Handle mill formation before any phase transition
            if (millFormed)
            {
                OnMillFormed();
                // Do not switch turn yet; the turn will switch after mill removal
                return; // Exit the method to wait for mill removal
            }

            if (CheckIfAllPiecesHaveBeenPlaced())
            {
                // Transition to Moving Phase without switching turn here
                TransitionToMovingPhase();
                return; // Exit the method as TransitionToMovingPhase handles the rest
            }
            else
            {
                // If still in placing phase, highlight all unoccupied positions
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();

                // Proceed to switch turn
                SwitchingTurn();
                SetUi();
                PieceManager.Instance.HighlightNextPieceToPlace();
                GameUIManager.Instance.gameView.SetTurnText();
            }
        }
        else if (currentPhase == GamePhase.Moving)
        {
            if (millFormed)
            {
                OnMillFormed();
                // Do not switch turn yet; the turn will switch after mill removal
                return; // Exit the method to wait for mill removal
            }

            // Proceed to switch turn
            SwitchingTurn();

            if (IsGameOverByNoValidMoves())
            {
                return; // Game over conditions met, stop further execution
            }
            else
            {
                UponNeedToSelectAPiece();
            }
            GameUIManager.Instance.gameView.SetTurnText();
        }

        canInteract = true;
    }





    public bool CheckIfAllPiecesHaveBeenPlaced()
    {
        // Check if all pieces have been placed
        if (piecesPlacedPlayer1 >= maxPiecesPerPlayer && piecesPlacedPlayer2 >= maxPiecesPerPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TransitionToMovingPhase()
    {
        currentPhase = GamePhase.Moving;
        GameUIManager.Instance.gameView.SetTopText(textData.transitioningToMovePhaseText);
        Debug.Log("GameManager :: Transitioning to Moving Phase");
        GameUIManager.Instance.gameView.ShowBottomText("You are now in the Moving Phase of the game!");
        BoardManager.Instance.HideHightlightsFromBoardPositions();
        PieceManager.Instance.RefreshPiecesLeftUi();

        // Switch turn to the next player
        SwitchingTurn();

        // Check if the next player has valid moves
        if (IsGameOverByNoValidMoves())
        {
            return; // Game over conditions met, stop further execution
        }
        else
        {
            UponNeedToSelectAPiece();
        }

        // Update the UI for the new turn
        GameUIManager.Instance.gameView.SetTurnText();
    }


    public void SavePreviousPhase()
    {
        gamePhasePriorToMillRemoval = currentPhase;
    }

    public void OnMillFormed()
    {
        Debug.Log("GameManager :: Mill formed! Player must remove an opponent's piece.");
        currentPhase = GamePhase.MillRemoval;

        // Ensure player can interact during mill removal
        canInteract = true;

        GameUIManager.Instance.gameView.SetTopText(textData.millFormedText);
        GameUIManager.Instance.gameView.SetTurnText();
        BoardManager.Instance.HideHightlightsFromBoardPositions();

        List<Piece> piecesToHighlight = new List<Piece>();
        bool isPlayer1Turn = IsPlayer1Turn();
        string opponentTag = isPlayer1Turn ? "Player2Piece" : "Player1Piece";
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onMillFormed);
        PieceManager.Instance.ResetAllPieceVisuals();

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag(opponentTag) && piece.boardPosition != null)
            {
                // Only highlight and scale up/down pieces that are NOT in a mill, or if all opponent pieces are in mills
                if (!PieceManager.Instance.IsInMill(piece.boardPosition) || PieceManager.Instance.AllOpponentPiecesInMill())
                {
                    piece.OutlinePiece(true);
                    piece.deleteSprite.gameObject.SetActive(true);
                    piecesToHighlight.Add(piece);
                }
            }
        }
        PieceManager.Instance.ScaleUpDownPiecesForMillOnly(piecesToHighlight);
    }



    // Returns true if it's Player 1's turn, false if it's Player 2's turn
    public bool IsPlayer1Turn()
    {
        return isPlayer1Turn;
    }

    public void PieceRemovedFromBoardByPlayer()
    {
        Debug.Log("GameManager :: PieceRemovedFromBoardByPlayer");
        DOTween.Kill("PiecesScaleUpDown", true);
        canInteract = false;

        if (CheckLossByPieceCount() || IsGameOverByNoValidMoves())
        {
            return;  // Game over conditions met, stop further execution
        }

        StartCoroutine(HandlePieceRemovalDelay());
    }

    private IEnumerator HandlePieceRemovalDelay()
    {
        yield return new WaitForSeconds(PieceManager.Instance.scaleDownDeletedPieceTime);  // Wait for 0.3 seconds

        canInteract = true;
        SwitchingTurn();

        if (CheckLossByPieceCount() || IsGameOverByNoValidMoves())
        {
            yield break;  // Stop if game is over after the turn switch
        }

        // Move to next phase depending on whether all pieces have been placed
        currentPhase = CheckIfAllPiecesHaveBeenPlaced() ? GamePhase.Moving : GamePhase.Placing;
        if (currentPhase == GamePhase.Moving)
        {
            UponNeedToSelectAPiece();
        }
        else
        {
            PieceManager.Instance.HighlightNextPieceToPlace();
        }

        // Update the UI text for the current turn
        GameUIManager.Instance.gameView.SetTurnText();

        // Highlight unoccupied positions during placing phase
        if (currentPhase == GamePhase.Placing)
        {
            BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            GameUIManager.Instance.gameView.SetTopText(textData.placePieceOnBoardText);
        }
    }


    public void UponNeedToSelectAPiece()
    {
        PieceManager.Instance.HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves();
        GameUIManager.Instance.gameView.SetTopText(textData.selectPieceText);
    }


    public void SetUi()
    {
        if (currentPhase == GamePhase.Placing)
        {
            GameUIManager.Instance.gameView.SetTopText(textData.placePieceOnBoardText);
        }
        else if (currentPhase == GamePhase.Moving)
        {
            GameUIManager.Instance.gameView.SetTopText(textData.moveToAdjacentSpotText);
        }
    }

    public bool CheckLossByPieceCount()
    {
        if (currentPhase == GamePhase.GameEnd) return false;

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
            Debug.Log("GameManager :: CheckLossByPieceCount :: Player 1 has les than 3 pieces left");
            DeclareWinner(false, WinReason.LessThan3PiecesLeft);
            return true;
        }
        if (player2Pieces < 3)
        {
            Debug.Log("GameManager :: CheckLossByPieceCount :: Player 2 has les than 3 pieces left");
            DeclareWinner(true, WinReason.LessThan3PiecesLeft);
            return true;
        }

        return false;
    }

    public bool IsGameOverByNoValidMoves()
    {
        if (currentPhase == GamePhase.GameEnd) return false;

        bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
        Debug.Log("GameManager :: Checking if it is game over by no valid moves...");
        string playerTag = isPlayer1Turn ? "Player1Piece" : "Player2Piece";

        //Debug.Log($"Checking for valid moves for: {(isPlayer1Turn ? "Player 1" : "Player 2")}");
        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag(playerTag) && piece.boardPosition != null)
            {
                //Debug.Log($"Checking piece at: {piece.boardPosition.name}");
                if (HasAnyValidMove(piece))
                {
                    //Debug.Log("Player has valid moves.");
                    return false;
                }
            }
        }

        // player 1 pieces have no possible moves
        if (isPlayer1Turn)
        {
            Debug.Log("GameManager ::Player 1 has no valid moves, thus Player 2 wins!");
            DeclareWinner(false, WinReason.NoValidMovesLeft);
        }
        else
        {
            Debug.Log("GameManager ::Player 2 has no valid moves, thus Player 1 wins!");
            DeclareWinner(true, WinReason.NoValidMovesLeft);
        }
        return true;
    }

    private bool HasAnyValidMove(Piece piece)
    {
        // Check if the player is in the flying phase (only 3 pieces left)
        if (PieceManager.Instance.IsFlyingPhaseForCurrentTurnPlayer())
        {
            for (int i = 0; i < BoardManager.Instance.GetAllBoardPositions().Count; i++)
            {
                BoardPosition position = BoardManager.Instance.GetAllBoardPositions()[i];
                if (!position.isOccupied)
                {
                    //Debug.Log("Found a valid position to fly to.");
                    return true; // Found at least one valid move
                }
            }
        }
        else
        {

            // If not in flying phase, check if the piece has any adjacent valid moves
            for (int i = 0; i < piece.boardPosition.adjacentPositions.Count; i++)
            {
                BoardPosition adjacent = piece.boardPosition.adjacentPositions[i];
                //Debug.Log($"Adjacent position: {adjacent.name}, Occupied: {adjacent.isOccupied}");
                if (!adjacent.isOccupied)
                {
                    //Debug.Log($"Found a valid adjacent move to position: {adjacent.name}");
                    return true;
                }
            }
        }
        //Debug.Log("No valid moves for this piece.");
        return false;
    }

    

    public void DeclareWinner(bool player1isWinner, WinReason _winReason)
    {
        if (currentPhase == GamePhase.GameEnd)
            return;

        winReason = _winReason;
        isPlayer1Winner = player1isWinner;
        StartCoroutine(OnGameEnd());
    }

    public IEnumerator OnGameEnd()
    {

        currentPhase = GamePhase.GameEnd;
        canInteract = false;
        PieceManager.Instance.UnhighlightAllPieces();
        BoardManager.Instance.HideHightlightsFromBoardPositions();

        string winner = "";
        if (isPlayer1Winner)
        {
            winner = PlayerProfile.Instance.GetGamePlayerData(true).playerName;
        }
        else
        {
            winner = PlayerProfile.Instance.GetGamePlayerData(false).playerName;
        }

        Debug.Log("GameManager :: GAME OVER! :: " + winner + " wins!");
        //GameUIManager.Instance.gameView.SetTopText(winner + " WINS!");
        GameUIManager.Instance.gameView.SetTopText("");
        GameUIManager.Instance.gameView.HideTurnText();
        GameUIManager.Instance.EnableView(null);
        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.boardPosition == null)
            {
                piece.gameObject.SetActive(false);
            }
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.winnerJingle);
        yield return new WaitForSecondsRealtime(0.5f);
        mainCamera.DOOrthoSize(cameraOrtoSize * 2.25f, 1.5f);

        yield return new WaitForSecondsRealtime(1.4f);
        GameUIManager.Instance.EnableView(GameUIManager.Instance.endView);
        GameUIManager.Instance.endView.StartWinAnimation(isPlayer1Winner);


    }

    [Button]
    public void TestingOnly_OnGameEnd()
    {
        DeclareWinner(true, WinReason.LessThan3PiecesLeft);
    }
}
