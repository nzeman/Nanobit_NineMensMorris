using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
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
                _Instance = FindFirstObjectByType<GameManager>();
            return _Instance;
        }
    }
    #endregion

    #region Enums
    public enum GamePhase { Placing, Moving, MillRemoval, GameEnd }
    public enum WinReason { LessThan3PiecesLeft, NoValidMovesLeft }
    #endregion

    #region Game State
    [SerializeField] private GamePhase currentPhase = GamePhase.Placing;
    [SerializeField] private GamePhase gamePhasePriorToMillRemoval;
    [SerializeField] private WinReason winReason;

    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool isPlayer1Turn = true;
    [SerializeField] private int maxPiecesPerPlayer = 9;
    [SerializeField] private int piecesPlacedPlayer1 = 0;
    [SerializeField] private int piecesPlacedPlayer2 = 0;
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isPlayer1Winner = false;
    #endregion

    #region Camera & UI Settings
    [Header("Camera settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float defaultCameraSize = 5f;
    [SerializeField] private float cameraOrtoSize = 5f;

    [Header("Time settings")]
    [SerializeField] private float timeToMovePieceToBoardPositionInMovingPhase = 0.5f;
    [SerializeField] private float timeToMovePieceToBoardInPlacingPhase = 0.5f;

    [Header("Text")]
    [SerializeField] private GameUITextData textData;
    #endregion

    #region Initialization
    private void Start()
    {
        Debug.Log("GameManager :: Initialization...");
        isPlayer1Turn = Random.value > 0.5f;
        BoardManager.Instance.Initialize();
        maxPiecesPerPlayer = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;
        // Adjust the camera size based on board size
        cameraOrtoSize = defaultCameraSize + (BoardManager.Instance.GetNumberOfRings() * 1.8F);
        mainCamera.orthographicSize = cameraOrtoSize;
        SetUi();
        GameUIManager.Instance.gameView.SetTurnText();
        PieceManager.Instance.SpawnAllPiecesAtStart();
        PieceManager.Instance.HighlightNextPieceToPlace();
        AudioManager.Instance.PlayGameMusic(AudioManager.Instance.GetAudioData().gameMusic);
        AudioManager.Instance.StopMainMenuMusic();
        PieceManager.Instance.RefreshPiecesLeftUi();
        canInteract = true;
        Debug.Log("GameManager :: Game started!");
    }
    #endregion

    #region Game Logic

    /// <summary>
    /// Switches the player's turn and updates the UI accordingly.
    /// </summary>
    private void SwitchingTurn()
    {
        Debug.Log("SWITCHING TURN!");
        isPlayer1Turn = !isPlayer1Turn;
        string debugString = isPlayer1Turn ?
            $"Player 1 turn! :: {PlayerProfile.Instance.GetGamePlayerData(true).playerName}" :
            $"Player 2 turn! :: {PlayerProfile.Instance.GetGamePlayerData(false).playerName}";

        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onTurnChanged);
        Debug.Log(debugString);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPhase == GamePhase.GameEnd) return;
            if (SceneLoadingManager.Instance.IsLoadingScene()) return;
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

    /// <summary>
    /// Opens the pause menu and pauses the game by freezing the time scale.
    /// </summary>
    private void OpenPauseMenu()
    {
        

        isGamePaused = true;
        GameUIManager.Instance.EnableView(GameUIManager.Instance.pauseView);
        Time.timeScale = 0.0000001f;
        mainCamera.DOOrthoSize(0.0001f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Resumes the game from the pause menu by resetting the time scale.
    /// </summary>
    public void ResumeGameFromPause()
    {
        Time.timeScale = 1f;
        GameUIManager.Instance.EnableView(GameUIManager.Instance.gameView);
        isGamePaused = false;
        mainCamera.DOOrthoSize(cameraOrtoSize, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Handles piece placement or movement after it has reached its board position.
    /// This includes checking for mill formation and transitioning between phases.
    /// </summary>
    /// <param name="millFormed">Indicates if a mill was formed after the move.</param>
    public void OnPieceReachedItsPositionOnBoard(bool millFormed)
    {
        if (currentPhase == GamePhase.Placing)
        {
            if (isPlayer1Turn)
                piecesPlacedPlayer1++;
            else
                piecesPlacedPlayer2++;

            if (millFormed)
            {
                OnMillFormed();
                return;
            }

            if (CheckIfAllPiecesHaveBeenPlaced())
            {
                TransitionToMovingPhase();
                return;
            }

            BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            SwitchingTurn();
            SetUi();
            PieceManager.Instance.HighlightNextPieceToPlace();
            GameUIManager.Instance.gameView.SetTurnText();
        }
        else if (currentPhase == GamePhase.Moving)
        {
            if (millFormed)
            {
                OnMillFormed();
                return;
            }

            SwitchingTurn();

            if (IsGameOverByNoValidMoves())
                return;

            UponNeedToSelectAPiece();
            GameUIManager.Instance.gameView.SetTurnText();
        }

        canInteract = true;
    }

    /// <summary>
    /// Transitions the game to the Moving phase after all pieces have been placed.
    /// Updates the UI and checks for valid moves for the next player.
    /// </summary>
    private void TransitionToMovingPhase()
    {
        currentPhase = GamePhase.Moving;
        GameUIManager.Instance.gameView.SetTopText(textData.transitioningToMovePhaseText);
        Debug.Log("GameManager :: Transitioning to Moving Phase");
        GameUIManager.Instance.gameView.ShowBottomText("You are now in the Moving Phase of the game!");
        BoardManager.Instance.HideHightlightsFromBoardPositions();
        PieceManager.Instance.RefreshPiecesLeftUi();

        SwitchingTurn();

        if (IsGameOverByNoValidMoves())
            return;

        UponNeedToSelectAPiece();
        GameUIManager.Instance.gameView.SetTurnText();
    }

    /// <summary>
    /// Handles the logic when a piece is removed from the board by a player after forming a mill.
    /// It checks for game-over conditions and transitions to the appropriate game phase.
    /// </summary>
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

    /// <summary>
    /// Waits for a short delay after a piece is removed, then switches turns and transitions to the next game phase.
    /// </summary>
    private IEnumerator HandlePieceRemovalDelay()
    {
        // Wait for the animation or visual effect of the piece removal to complete
        yield return new WaitForSeconds(PieceManager.Instance.scaleDownDeletedPieceTime);  // Wait for 0.3 seconds

        canInteract = true;  // Allow player interactions again
        SwitchingTurn();  // Switch the current player's turn

        // Check for game-ending conditions after the turn switch
        if (CheckLossByPieceCount() || IsGameOverByNoValidMoves())
        {
            yield break;  // Stop execution if the game is over
        }

        // Transition to the appropriate game phase based on piece placement
        currentPhase = CheckIfAllPiecesHaveBeenPlaced() ? GamePhase.Moving : GamePhase.Placing;

        if (currentPhase == GamePhase.Moving)
        {
            UponNeedToSelectAPiece();  // Highlight movable pieces in the Moving phase
        }
        else
        {
            PieceManager.Instance.HighlightNextPieceToPlace();  // Highlight the next piece to be placed in the Placing phase
        }

        // Update the UI to reflect the current turn
        GameUIManager.Instance.gameView.SetTurnText();

        // Highlight unoccupied positions during the Placing phase
        if (currentPhase == GamePhase.Placing)
        {
            BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            GameUIManager.Instance.gameView.SetTopText(textData.placePieceOnBoardText);
        }
    }


    #endregion

    #region Mill Logic

    /// <summary>
    /// Handles the actions to take when a mill is formed.
    /// Highlights opponent pieces that can be removed and transitions to Mill Removal phase.
    /// </summary>
    private void OnMillFormed()
    {
        Debug.Log("GameManager :: Mill formed! Player must remove an opponent's piece.");
        currentPhase = GamePhase.MillRemoval;
        canInteract = true;

        GameUIManager.Instance.gameView.SetTopText(textData.millFormedText);
        GameUIManager.Instance.gameView.SetTurnText();
        BoardManager.Instance.HideHightlightsFromBoardPositions();

        List<Piece> piecesToHighlight = new List<Piece>();
        string opponentTag = isPlayer1Turn ? "Player2Piece" : "Player1Piece";
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onMillFormed);
        PieceManager.Instance.ResetAllPieceVisuals();

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag(opponentTag) && piece.boardPosition != null)
            {
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
    #endregion

    #region Game End

    /// <summary>
    /// Declares the winner of the game and starts the end game sequence.
    /// </summary>
    /// <param name="player1isWinner">True if Player 1 is the winner, false otherwise.</param>
    /// <param name="_winReason">The reason for the win.</param>
    public void DeclareWinner(bool player1isWinner, WinReason _winReason)
    {
        if (currentPhase == GamePhase.GameEnd)
            return;

        winReason = _winReason;
        isPlayer1Winner = player1isWinner;
        StartCoroutine(OnGameEnd());
    }

    /// <summary>
    /// Handles the end game sequence, including zooming the camera out and showing the end game view.
    /// </summary>
    private IEnumerator OnGameEnd()
    {
        currentPhase = GamePhase.GameEnd;
        canInteract = false;
        PieceManager.Instance.UnhighlightAllPieces();
        BoardManager.Instance.HideHightlightsFromBoardPositions();
        string winner = isPlayer1Winner
            ? PlayerProfile.Instance.GetGamePlayerData(true).playerName
            : PlayerProfile.Instance.GetGamePlayerData(false).playerName;

        Debug.Log("GameManager :: GAME OVER! :: " + winner + " wins!");

        GameUIManager.Instance.gameView.SetTopText("");
        GameUIManager.Instance.gameView.HideTurnText();
        GameUIManager.Instance.EnableView(null);

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.boardPosition == null)
                piece.gameObject.SetActive(false);
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().winnerJingle);
        yield return new WaitForSecondsRealtime(0.5f);

        mainCamera.DOOrthoSize(cameraOrtoSize * 2.25f, 1.5f);

        yield return new WaitForSecondsRealtime(1.4f);
        GameUIManager.Instance.EnableView(GameUIManager.Instance.endView);
        GameUIManager.Instance.endView.StartWinAnimation(isPlayer1Winner);
    }
    #endregion

    #region Helper Methods

    private bool CheckIfAllPiecesHaveBeenPlaced()
    {
        return piecesPlacedPlayer1 >= maxPiecesPerPlayer && piecesPlacedPlayer2 >= maxPiecesPerPlayer;
    }

    private void UponNeedToSelectAPiece()
    {
        PieceManager.Instance.HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves();
        GameUIManager.Instance.gameView.SetTopText(textData.selectPieceText);
    }

    private void SetUi()
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

    private bool CheckLossByPieceCount()
    {
        if (currentPhase == GamePhase.GameEnd) return false;

        int player1Pieces = 0;
        int player2Pieces = 0;

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag("Player1Piece"))
                player1Pieces++;
            else if (piece.CompareTag("Player2Piece"))
                player2Pieces++;
        }

        if (player1Pieces < 3)
        {
            Debug.Log("GameManager :: Player 1 has less than 3 pieces left");
            DeclareWinner(false, WinReason.LessThan3PiecesLeft);
            return true;
        }

        if (player2Pieces < 3)
        {
            Debug.Log("GameManager :: Player 2 has less than 3 pieces left");
            DeclareWinner(true, WinReason.LessThan3PiecesLeft);
            return true;
        }

        return false;
    }

    public bool IsGameOverByNoValidMoves()
    {
        if (currentPhase == GamePhase.GameEnd) return false;

        bool isPlayer1Turn = IsPlayer1Turn();
        Debug.Log("GameManager :: Checking if game is over due to no valid moves...");
        string playerTag = isPlayer1Turn ? "Player1Piece" : "Player2Piece";

        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if (piece.CompareTag(playerTag) && piece.boardPosition != null && HasAnyValidMove(piece))
            {
                return false;
            }
        }

        if (isPlayer1Turn)
        {
            Debug.Log("GameManager :: Player 1 has no valid moves, Player 2 wins!");
            DeclareWinner(false, WinReason.NoValidMovesLeft);
        }
        else
        {
            Debug.Log("GameManager :: Player 2 has no valid moves, Player 1 wins!");
            DeclareWinner(true, WinReason.NoValidMovesLeft);
        }

        return true;
    }

    private bool HasAnyValidMove(Piece piece)
    {
        if (PieceManager.Instance.IsFlyingPhaseForCurrentTurnPlayer())
        {
            foreach (var position in BoardManager.Instance.GetAllBoardPositions())
            {
                if (!position.isOccupied)
                    return true;
            }
        }
        else
        {
            foreach (var adjacent in piece.boardPosition.adjacentPositions)
            {
                if (!adjacent.isOccupied)
                    return true;
            }
        }

        return false;
    }

    public bool IsPlayer1Turn()
    {
        return isPlayer1Turn;
    }

    /// <summary>
    /// Saves the current phase before transitioning to Mill Removal.
    /// </summary>
    public void SavePreviousPhase()
    {
        gamePhasePriorToMillRemoval = currentPhase;
    }


    #endregion

    #region Getters

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public void SetCurrentPhase(GamePhase _gamePhase)
    {
        currentPhase = _gamePhase;
    }

    public GamePhase GetGetPhaseBeforeMill()
    {
        return gamePhasePriorToMillRemoval;
    }

    public bool CanPlayerInteract()
    {
        return canInteract;
    }

    public void SetCanPlayerInteract(bool _canInteract)
    {
        canInteract = _canInteract;
    }

    public int GetMaxPiecesByPlayer()
    {
        return maxPiecesPerPlayer;
    }

    public int GetPiecesCountPlacedByPlayer(bool isPlayer1)
    {
        if (isPlayer1)
        {
            return piecesPlacedPlayer1;
        }
        else
        {
            return piecesPlacedPlayer2;
        }
    }

    public WinReason GetWinReason()
    {
        return winReason;
    }

    public float GetTimeToMovePieceToBoardPositionInMovingPhase()
    {
        return timeToMovePieceToBoardPositionInMovingPhase;
    }

    public float GetTimeToMovePieceToBoardPositionInPlacingPhase()
    {
        return timeToMovePieceToBoardInPlacingPhase;
    }

    public GameUITextData GetTextData()
    {
        return textData;
    }

    #endregion
}
