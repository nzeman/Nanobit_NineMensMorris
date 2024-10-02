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
                _Instance = FindObjectOfType<GameManager>();
            return _Instance;
        }
    }
    #endregion

    public enum GamePhase { Placing, Moving, MillRemoval, GameEnd }
    public GamePhase currentPhase = GamePhase.Placing;
    public GamePhase gamePhasePriorToMillRemoval = GamePhase.Placing;

    public bool canInteract = true;
    private bool isPlayer1Turn = true;
    public int maxPiecesPerPlayer = 9;
    public int piecesPlacedPlayer1 = 0;
    public int piecesPlacedPlayer2 = 0;
    public bool isGamePaused = false;

    [Header("Camera settings")]
    public Camera camera;
    public float defaultCameraSize = 5f;
    public float cameraOrtoSize = 5f;

    [Header("Time settings")]
    public float timeToMovePieceToBoardPositionInMovingPhase = 0.5f;
    public float timeToMovePieceToBoardInPlacingPhase = 0.5f;

    public void Start()
    {
        maxPiecesPerPlayer = PlayerProfile.Instance.playerData.gameRulesData.numberOfPiecesPerPlayer;

        float ortoSizeCamera = defaultCameraSize + (BoardManager.Instance.numberOfRings * 1.15F);
        cameraOrtoSize = ortoSizeCamera;
        camera.orthographicSize = ortoSizeCamera;

        SetUi();
        GameUIManager.Instance.gameView.SetTurnText();
        PieceManager.Instance.SpawnAllPiecesAtStart();
        PieceManager.Instance.HighlightNextPieceToPlace();
        canInteract = true;

        AudioManager.Instance.PlayGameMusic(AudioManager.Instance.audioClipDataHolder.gameMusic);
        AudioManager.Instance.StopMainMenuMusic();

        PieceManager.Instance.RefreshPiecesLeftUi();

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
        //camera.transform.DOMoveY(25f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
        camera.DOOrthoSize(0.0001f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    public void ResumeGameFromPause()
    {
        Time.timeScale = 1f;
        GameUIManager.Instance.EnableView(GameUIManager.Instance.gameView);
        isGamePaused = false;
        //camera.transform.DOMoveY(0f, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
        camera.DOOrthoSize(cameraOrtoSize, .3f).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    public void PiecePlacedByPlayer(bool millFormed)
    {
        if (currentPhase == GamePhase.Placing)
        {
            if (isPlayer1Turn)
                piecesPlacedPlayer1++;
            else
                piecesPlacedPlayer2++;

            if (CheckIfAllPiecesHaveBeenPlaced())
            {
                TransitionToMovingPhase();
            }
            else
            {
                
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            }

           
        }

        canInteract = true;

        if (millFormed)
        {
            OnMillFormed();
        }
        else
        {

            isPlayer1Turn = !isPlayer1Turn;

            if (currentPhase == GamePhase.Moving)
            {
                // Check if the current player has no valid moves after this placement or move
                if (CheckLossByNoValidMoves())
                {
                    DeclareWinner(!isPlayer1Turn);
                    return;
                }
                else
                {
                    UponNeedToSelectAPiece();
                }
            }


            GameUIManager.Instance.gameView.SetTurnText();

            if (currentPhase == GamePhase.Placing)
            {
            SetUi();
                PieceManager.Instance.HighlightNextPieceToPlace();
            }

        }
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
        GameUIManager.Instance.gameView.SetTopText("Transitioning to Moving Phase");
        Debug.Log("Transitioning to Moving Phase");
        GameUIManager.Instance.gameView.ShowBottomText("You are now in the Moving Phase of the game!");
        BoardManager.Instance.HideHightlightsFromBoardPositions();
        PieceManager.Instance.RefreshPiecesLeftUi();
        if (CheckLossByNoValidMoves())
        {
            DeclareWinner(!isPlayer1Turn);
            return;
        }
    }

    public void SavePreviousPhase()
    {
        gamePhasePriorToMillRemoval = currentPhase;
    }

    public void OnMillFormed()
    {
        Debug.Log("Mill formed! Player must remove an opponent's piece.");

        currentPhase = GamePhase.MillRemoval;
        GameUIManager.Instance.gameView.SetTopText("Mill formed! Player must remove an opponent's piece.");
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

    public void PieceRemoved()
    {
        Debug.Log("Piece removed");
        DOTween.Kill("PiecesScaleUpDown", true);

        canInteract = false;

        if (CheckLossByPieceCount() || (CheckLossByNoValidMoves() /*&& currentPhase == GamePhase.Moving*/))
        {
            DeclareWinner(IsPlayer1Turn());
            return;
        }

        DOVirtual.DelayedCall(.3f, () =>
        {

            canInteract = true;
            isPlayer1Turn = !isPlayer1Turn;
            if (CheckLossByPieceCount() || (CheckLossByNoValidMoves() /*&& currentPhase == GamePhase.Moving*/))
            {
                DeclareWinner(IsPlayer1Turn());
                return;
            }
            if (CheckIfAllPiecesHaveBeenPlaced())
            {
                currentPhase = GamePhase.Moving;

                UponNeedToSelectAPiece();
            }
            else
            {
                currentPhase = GamePhase.Placing;
                PieceManager.Instance.HighlightNextPieceToPlace();
                
            }
            if (isPlayer1Turn)
            {
                GameUIManager.Instance.gameView.SetTurnText();
                
                Debug.Log("Player 1 turn");
            }
            else
            {
                GameUIManager.Instance.gameView.SetTurnText();
                
                Debug.Log("Player 2 turn");
            }
            //SetUi();

            if (currentPhase == GamePhase.Placing)
            {
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
                GameUIManager.Instance.gameView.SetTopText("PLACE YOUR PIECE ON THE BOARD!");
            }
            else
            {
                GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECE BY CLICKING ON AN UNOCCUPIED SPOT!");
            }
        });
        //PieceManager.Instance.UnhighlightAllPieces();
    }

    public void UponNeedToSelectAPiece()
    {
        PieceManager.Instance.HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves();
        GameUIManager.Instance.gameView.SetTopText("SELECT YOUR PIECE BY CLICKING ON IT!");
    }


    public void SetUi()
    {
        if (currentPhase == GamePhase.Placing)
        {
            GameUIManager.Instance.gameView.SetTopText("PLACE YOUR PIECE ON THE BOARD!");
        }
        else if (currentPhase == GamePhase.Moving)
        {
            GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECES AND TRY TO FORM A MILL!");
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
        bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
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

        Debug.Log($"No valid moves for {(isPlayer1Turn ? "Player 1" : "Player 2")}. Declaring other player as winner.");
        DeclareWinner(!isPlayer1Turn);
        return true;
    }




    public bool HasAnyValidMove(Piece piece)
    {
        // Check if the player is in the flying phase (only 3 pieces left)
        if (PieceManager.Instance.IsFlyingPhaseForCurrentTurnPlayer())
        {
            Debug.Log("Player is in the flying phase. Checking for any open positions.");
            for (int i = 0; i < BoardManager.Instance.allBoardPositions.Count; i++)
            {
                BoardPosition position = BoardManager.Instance.allBoardPositions[i];
                if (!position.isOccupied)
                {
                    Debug.Log("Found a valid position to fly to.");
                    return true; // Found at least one valid move
                }
            }
        }
        else
        {
            Debug.Log($"Checking adjacent positions for piece at {piece.boardPosition.name}");

            // Log the number of adjacent positions
            Debug.Log($"Piece at {piece.boardPosition.name} has {piece.boardPosition.adjacentPositions.Count} adjacent positions.");

            // If not in flying phase, check if the piece has any adjacent valid moves
            for (int i = 0; i < piece.boardPosition.adjacentPositions.Count; i++)
            {
                BoardPosition adjacent = piece.boardPosition.adjacentPositions[i];
                Debug.Log($"Adjacent position: {adjacent.name}, Occupied: {adjacent.isOccupied}");

                if (!adjacent.isOccupied)
                {
                    Debug.Log($"Found a valid adjacent move to position: {adjacent.name}");
                    return true;
                }
            }
        }

        Debug.Log("No valid moves for this piece.");
        return false;
    }





    public void DeclareWinner(bool isPlayer1Turn)
    {
        if (currentPhase == GamePhase.GameEnd)
            return;

        StartCoroutine(OnGameEnd());
    }

    public IEnumerator OnGameEnd()
    {

        currentPhase = GamePhase.GameEnd;
        canInteract = false;

        PieceManager.Instance.UnhighlightAllPieces();

        string winner = isPlayer1Turn ? 
            PlayerProfile.Instance.GetGamePlayerData(true).playerName 
            :
            PlayerProfile.Instance.GetGamePlayerData(false).playerName;

        Debug.Log(winner + " wins!");
        //GameUIManager.Instance.gameView.SetTopText(winner + " WINS!");
        GameUIManager.Instance.gameView.SetTopText("");
        GameUIManager.Instance.gameView.HideTurnText();
        GameUIManager.Instance.EnableView(null);
        foreach (var piece in PieceManager.Instance.allPieces)
        {
            if(piece.boardPosition == null)
            {
                piece.gameObject.SetActive(false);
            }
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.winnerJingle);
        yield return new WaitForSecondsRealtime(0.5f);
        camera.DOOrthoSize(cameraOrtoSize * 1.75f, 1.5f);

        yield return new WaitForSecondsRealtime(1.4f);
        GameUIManager.Instance.EnableView(GameUIManager.Instance.endView);
        GameUIManager.Instance.endView.StartWinAnimation(isPlayer1Turn);


    }

    [Button]
    public void TestingOnly_OnGameEnd()
    {
        DeclareWinner(true);
    }
}
