using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the pieces in the game, handling placement, movement, mill detection, and interactions.
/// </summary>
public class PieceManager : MonoBehaviour
{
    #region Singleton
    private static PieceManager _Instance;
    public static PieceManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<PieceManager>();
            return _Instance;
        }
    }
    #endregion

    #region Public Variables
    public GameObject piecePrefabPlayer1;
    public GameObject piecePrefabPlayer2;
    public LayerMask boardLayer;
    public LayerMask pieceLayer;

    [SerializeField] private BoardPosition selectedPiecePosition;
    public List<Piece> allPieces;

    public float scaleDownDeletedPieceTime = 0.3f;

    private Queue<Piece> player1PiecesQueue = new Queue<Piece>();
    private Queue<Piece> player2PiecesQueue = new Queue<Piece>();

    // UI GameObjects for the spawn positions
    public RectTransform player1SpawnUI;
    public RectTransform player2SpawnUI;
    #endregion

    #region Unity Methods
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.CanPlayerInteract() == false) return;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleBoardPointClick(mousePosition);
        }
    }
    #endregion

    #region Piece Interaction Methods
    public BoardPosition GetSelectedPiecePosition()
    {
        return selectedPiecePosition;
    }

    /// <summary>
    /// Handles the player's click on the board, determining the game phase and acting accordingly.
    /// </summary>
    /// <param name="mousePosition">The position of the mouse click.</param>
    public void HandleBoardPointClick(Vector2 mousePosition)
    {
        RaycastHit2D hitPiece = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, pieceLayer);
        RaycastHit2D hitBoard = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, boardLayer);

        // Mill removal 
        if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.MillRemoval && hitPiece.collider != null)
        {
            if (hitBoard.collider == null)
            {
                // The pieces that were not yet placed (for example in placing phase)
                return;
            }

            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();
            HandleMillRemoval(boardPosition);
        }
        else if (hitBoard.collider != null)
        {
            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();
            if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Placing)
            {
                HandlePlacingPhase(boardPosition);
            }
            else if (GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Moving)
            {
                HandleMovingPhase(boardPosition);
            }
        }
    }

    public void ResetAllPieceVisuals()
    {
        Debug.Log("ResetAllPieceVisuals");
        foreach (var piece in PieceManager.Instance.allPieces)
        {
            piece.ResetVisual();
        }
    }

    public bool IsFlyingPhaseForCurrentTurnPlayer()
    {
        return GetPieceCountForPlayer(GameManager.Instance.IsPlayer1Turn()) == 3;
    }

    public void UnhighlightAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.ResetVisual();
        }
    }
    #endregion

    #region Placing Phase Methods
    /// <summary>
    /// Handles the placing phase when the player clicks on a board position.
    /// </summary>
    /// <param name="position">The board position clicked.</param>
    void HandlePlacingPhase(BoardPosition position)
    {
        if (!position.isOccupied)
        {
            GameUIManager.Instance.gameView.HideTurnText();
            GameUIManager.Instance.gameView.SetTopText("");
            bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
            GameManager.Instance.SavePreviousPhase();
            GameManager.Instance.SetCanPlayerInteract(false);
            BoardManager.Instance.HideHightlightsFromBoardPositions();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPiecePlacedClick);
            Piece pieceToPlace = isPlayer1Turn ? player1PiecesQueue.Dequeue() : player2PiecesQueue.Dequeue();

            Debug.Log($"Placing piece on {position.name}...");
            pieceToPlace.transform.DOMove(position.transform.position, GameManager.Instance.GetTimeToMovePieceToBoardPositionInPlacingPhase()).OnComplete(() =>
            {
                StartCoroutine(OnPieceReachedPositionInPlacingPhase(pieceToPlace, position, isPlayer1Turn));
            });
        }
    }

    /// <summary>
    /// Coroutine called when a piece reaches its position during the placing phase.
    /// </summary>
    /// <param name="pieceToPlace">The piece being placed.</param>
    /// <param name="position">The target board position.</param>
    /// <param name="isPlayer1Turn">Is it Player 1's turn?</param>
    public IEnumerator OnPieceReachedPositionInPlacingPhase(Piece pieceToPlace, BoardPosition position, bool isPlayer1Turn)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPieceReachedPositionWhenPlacing);
        yield return new WaitForSecondsRealtime(.2f);

        position.OccupyPosition(pieceToPlace);
        pieceToPlace.boardPosition = position;
        Debug.Log($"Piece reached its new position...");

        // First, check and handle any broken mills
        CheckAndHandleBrokenMills();

        // Get all mills involving the position
        List<List<BoardPosition>> millsFormed = GetAllMillsInvolvingPosition(position, isPlayer1Turn ? "Player1Piece" : "Player2Piece");

        // Determine if any mills were formed
        bool millFormed = millsFormed.Count > 0;

        // Highlight the mills if any were formed
        if (millFormed)
        {
            BoardManager.Instance.HighlightMills(millsFormed);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onMillFormed);
        }

        GameManager.Instance.SetCurrentPhase(GameManager.Instance.GetGetPhaseBeforeMill());
        GameManager.Instance.OnPieceReachedItsPositionOnBoard(millFormed);
        GameManager.Instance.SetCanPlayerInteract(true);
        RefreshPiecesLeftUi();
    }

    public void RefreshPiecesLeftUi()
    {
        int countPlayer1 = GameManager.Instance.GetMaxPiecesByPlayer() - GameManager.Instance.GetPiecesCountPlacedByPlayer(true);
        int countPlayer2 = GameManager.Instance.GetMaxPiecesByPlayer() - GameManager.Instance.GetPiecesCountPlacedByPlayer(false);
        GameUIManager.Instance.gameView.player1UiPanel.piecesLeftToPlaceText.text = (countPlayer1).ToString();
        GameUIManager.Instance.gameView.player2UiPanel.piecesLeftToPlaceText.text = (countPlayer2).ToString();
        if (countPlayer1 <= 0)
        {
            GameUIManager.Instance.gameView.player1UiPanel.piecesLeftToPlaceText.gameObject.SetActive(false);
        }
        if (countPlayer2 <= 0)
        {
            GameUIManager.Instance.gameView.player2UiPanel.piecesLeftToPlaceText.gameObject.SetActive(false);
        }
    }

    public void HighlightNextPieceToPlace()
    {
        bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
        Piece nextPieceToPlace = isPlayer1Turn ? player1PiecesQueue.Peek() : player2PiecesQueue.Peek();
        if (isPlayer1Turn)
        {
            nextPieceToPlace.transform.DOMove(nextPieceToPlace.transform.position + new Vector3(1f, 0f, 0f), 0.3f);
            nextPieceToPlace.transform.DOScale(Vector3.one, .3f);
        }
        else
        {
            nextPieceToPlace.transform.DOMove(nextPieceToPlace.transform.position + new Vector3(-1f, 0f, 0f), 0.3f);
            nextPieceToPlace.transform.DOScale(Vector3.one, .3f);
        }
    }
    #endregion

    #region Moving Phase Methods
    /// <summary>
    /// Handles the moving phase when the player clicks on a board position.
    /// </summary>
    /// <param name="position">The board position clicked.</param>
    void HandleMovingPhase(BoardPosition position)
    {
        // If no piece is selected, try to select one
        if (selectedPiecePosition == null)
        {
            TrySelectPiece(position); // Handles both selecting and validating the piece based on playerTag
        }
        // If a piece is already selected, check if the target position is not occupied to move
        else if (!position.isOccupied)
        {
            TryMovePiece(position); // Attempt to move to the target position
        }
        else if (position.isOccupied)
        {
            TrySelectPiece(position); // Handles the logic for selecting another piece
        }
    }

    /// <summary>
    /// Highlights pieces that the current player can select and that have valid moves.
    /// </summary>
    public void HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves()
    {
        string playerTag = GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece";

        foreach (var piece in allPieces)
        {
            if (piece.CompareTag(playerTag))
            {
                bool hasValidMove = piece.boardPosition.adjacentPositions.Any(x => !x.isOccupied) || IsFlyingPhaseForCurrentTurnPlayer();
                if (hasValidMove)
                {
                    if (piece == selectedPiecePosition?.occupyingPiece)
                    {
                        piece.OutlinePiece(true);
                    }
                    else
                    {
                        piece.OutlinePiece(false);
                    }

                    piece.ScaleUp(true);
                }
                else
                {
                    piece.OutlinePiece(false);
                    piece.ResetVisual();
                }
            }
            else
            {
                // Reset opponent's pieces
                piece.ResetVisual();
            }
        }
    }

    private void TrySelectPiece(BoardPosition position)
    {
        string playerTag = GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece";

        if (position.isOccupied && position.occupyingPiece.CompareTag(playerTag))
        {
            // Deselect the previous piece and clear its adjacent highlights
            if (selectedPiecePosition != null)
            {
                BoardManager.Instance.HideHightlightsFromBoardPositions();
                selectedPiecePosition.occupyingPiece.OutlinePiece(false);
            }

            // Select the new piece
            selectedPiecePosition = position;

            bool isFlyingPhase = IsFlyingPhaseForCurrentTurnPlayer();

            // Check if the selected piece has valid moves
            bool hasValidMove = isFlyingPhase || position.adjacentPositions.Any(x => !x.isOccupied);

            if (!hasValidMove)
            {
                // Show feedback for no valid moves but do not apply scaling or outlining
                ShowNoValidMovesFeedback(position);
                selectedPiecePosition = null;  // Clear selection since no valid moves
                return;  // Exit the method without scaling or outlining
            }

            // Highlight valid moves or handle flying phase
            if (isFlyingPhase)
            {
                // Highlight all available board positions if the player is in the flying phase
                GameUIManager.Instance.gameView.SetTopText(GameManager.Instance.GetTextData().flyingPhaseText);
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            }
            else
            {
                // Highlight adjacent valid moves only for the selected piece
                HighlightAdjacentPositions(selectedPiecePosition);
                GameUIManager.Instance.gameView.SetTopText(GameManager.Instance.GetTextData().moveToAdjacentSpotText);
            }

            // Play sound and highlight the selected piece
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPieceSelected);

            // Show outline and scale only for the selected piece that has valid moves
            position.occupyingPiece.OutlinePiece(true);
            position.occupyingPiece.ScaleUp(true);  // Ensure scaling but without restarting the animation
        }
        else
        {
            if (position.isOccupied)
            {
                GameUIManager.Instance.gameView.ShowBottomText("This piece does not belong to you!");
            }
            else
            {
                GameUIManager.Instance.gameView.ShowBottomText("You need to select your piece!");
            }
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onIllegalMove);
        }
    }

    private void HighlightAdjacentPositions(BoardPosition position)
    {
        // Highlight adjacent positions only for the selected piece
        foreach (var adjacentPosition in position.adjacentPositions)
        {
            if (!adjacentPosition.isOccupied) // Only highlight unoccupied positions
            {
                adjacentPosition.HighlightBoardPosition(true);
            }
            else
            {
                adjacentPosition.HighlightBoardPosition(false);
            }
        }
    }

    private void TryMovePiece(BoardPosition targetPosition)
    {
        Debug.Log("TryMovePiece");
        bool isFlyingPhase = GetPieceCountForPlayer(GameManager.Instance.IsPlayer1Turn()) <= 3;

        if (isFlyingPhase || selectedPiecePosition.IsAdjacent(targetPosition))
        {
            GameUIManager.Instance.gameView.SetTopText("");
            GameUIManager.Instance.gameView.HideTurnText();
            GameManager.Instance.SetCanPlayerInteract(false);

            // Stop scaling and reset visuals for all pieces once a move is confirmed
            ResetAllScalingAndVisuals();

            MovePiece(selectedPiecePosition, targetPosition);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPieceMove);
            BoardManager.Instance.HideHightlightsFromBoardPositions();

            DOVirtual.DelayedCall(GameManager.Instance.GetTimeToMovePieceToBoardPositionInMovingPhase(), () =>
            {
                StartCoroutine(OnPieceReachPositionInMovingPhase(targetPosition));
            }).SetUpdate(true);
        }
        else
        {
            HandleInvalidMove();
        }
    }

    private void ResetAllScalingAndVisuals()
    {
        // Stop all scaling and reset visuals for each piece
        foreach (var piece in allPieces)
        {
            piece.ResetVisual(); // Reset each piece to its original state, including stopping scaling
        }
    }

    private void ShowNoValidMovesFeedback(BoardPosition position)
    {
        // Feedback for trying to select a piece without valid moves
        Debug.Log($"Selected piece at {position.name} has no valid moves.");

        GameUIManager.Instance.gameView.SetTopText(GameManager.Instance.GetTextData().selectPieceText);
        GameUIManager.Instance.gameView.ShowBottomText("This piece cannot be moved!");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onIllegalMove);

        BoardManager.Instance.HideHightlightsFromBoardPositions();
        // Reset visuals and highlight selectable pieces
        PieceManager.Instance.HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves();
    }

    private void HandleInvalidMove()
    {
        Debug.Log("Invalid move: Not adjacent and not in flying phase.");
        GameUIManager.Instance.gameView.ShowBottomText("Selected piece cannot go to this position!");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onIllegalMove);
    }

    /// <summary>
    /// Coroutine called when a piece reaches its position during the moving phase.
    /// </summary>
    /// <param name="position">The target board position.</param>
    public IEnumerator OnPieceReachPositionInMovingPhase(BoardPosition position)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPieceReachedPositionWhenPlacing);
        yield return new WaitForSecondsRealtime(0.2f);

        GameManager.Instance.SetCanPlayerInteract(false);

        // First, check and handle any broken mills
        CheckAndHandleBrokenMills();

        // Get all mills involving the position
        List<List<BoardPosition>> millsFormed = GetAllMillsInvolvingPosition(position, GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece");

        // Determine if any mills were formed
        bool millFormed = millsFormed.Count > 0;

        // Highlight the mills if any were formed
        if (millFormed)
        {
            BoardManager.Instance.HighlightMills(millsFormed);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onMillFormed);
        }

        // Proceed with the game flow
        GameManager.Instance.OnPieceReachedItsPositionOnBoard(millFormed);

        BoardManager.Instance.HideHightlightsFromBoardPositions();
    }

    void MovePiece(BoardPosition from, BoardPosition to)
    {
        Piece piece = from.occupyingPiece;
        piece.OutlinePiece(false);

        from.ClearPosition();
        piece.boardPosition = to;

        to.OccupyPosition(piece);
        piece.transform.DOMove(to.transform.position, GameManager.Instance.GetTimeToMovePieceToBoardPositionInMovingPhase());

        selectedPiecePosition = null;
    }
    #endregion

    #region Mill Handling Methods
    /// <summary>
    /// Handles the removal of a piece during the mill removal phase.
    /// </summary>
    /// <param name="position">The board position of the piece to remove.</param>
    void HandleMillRemoval(BoardPosition position)
    {
        if (position == null || position.occupyingPiece == null)
        {
            Debug.LogWarning("Invalid position or empty occupying piece in mill removal.");
            return;
        }

        // Check if the piece belongs to the opponent
        if (position.occupyingPiece.CompareTag(GameManager.Instance.IsPlayer1Turn() ? "Player2Piece" : "Player1Piece"))
        {
            // Check if the piece can be removed
            if (!IsInMill(position) || AllOpponentPiecesInMill())
            {
                GameManager.Instance.SetCanPlayerInteract(false);
                // Store references before clearing the position
                Piece pieceToDestroy = position.occupyingPiece;
                GameObject pieceGameObject = pieceToDestroy.gameObject;
                allPieces.Remove(pieceToDestroy);
                position.ClearPosition();
                pieceGameObject.transform.DOScale(0f, scaleDownDeletedPieceTime).OnComplete(() =>
                {
                    // Destroy();
                    pieceGameObject.SetActive(false);

                    CheckAndHandleBrokenMills();
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onPieceRemovedFromBoardByMill);
                    GameManager.Instance.PieceRemovedFromBoardByPlayer();

                    if (GameManager.Instance.IsGameOverByNoValidMoves())
                    {
                        return;
                    }
                });
            }
            else
            {
                Debug.Log("Cannot remove a piece that is in a mill unless all opponent pieces are in mills.");
                GameUIManager.Instance.gameView.ShowBottomText("Cannot remove a piece that is in a mill unless all opponent pieces are in mills!");
                AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onIllegalMove);
            }
        }
        else
        {
            Debug.Log("You can't remove your own piece!");
            GameUIManager.Instance.gameView.ShowBottomText("You can't remove your own piece!");
            AudioManager.Instance.PlaySFX(AudioManager.Instance.GetAudioData().onIllegalMove);
        }
    }

    void CheckAndHandleBrokenMills()
    {
        List<List<BoardPosition>> millsToRemove = new List<List<BoardPosition>>();

        // Iterate over active mills and check if they are still valid
        foreach (var mill in BoardManager.Instance.GetActiveMills())
        {
            if (!IsMillStillValid(mill))
            {
                millsToRemove.Add(mill);
            }
        }

        // Reset visuals for broken mills
        foreach (var brokenMill in millsToRemove)
        {
            BoardManager.Instance.ResetMillLines(brokenMill);
        }

        // Remove broken mills from active mills
        BoardManager.Instance.RemoveMills(millsToRemove);
    }

    bool IsMillStillValid(List<BoardPosition> mill)
    {
        if (mill == null || mill.Count != 3)
            return false;

        // Check if occupyingPiece is not null
        if (mill[0].occupyingPiece == null)
            return false;

        string playerTag = mill[0].occupyingPiece.tag;

        // Check if all positions are still occupied by the same player's pieces
        foreach (var position in mill)
        {
            if (!position.isOccupied || position.occupyingPiece == null || position.occupyingPiece.tag != playerTag)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsInMill(BoardPosition position)
    {
        string playerTag = position.occupyingPiece.CompareTag("Player1Piece") ? "Player1Piece" : "Player2Piece";

        // Check for mill in horizontal direction
        if (CheckMillInLine(position, playerTag, true))
        {
            return true;
        }

        // Check for mill in vertical direction
        if (CheckMillInLine(position, playerTag, false))
        {
            return true;
        }

        return false;
    }

    public bool AllOpponentPiecesInMill()
    {
        string opponentTag = GameManager.Instance.IsPlayer1Turn() ? "Player2Piece" : "Player1Piece";
        foreach (var position in BoardManager.Instance.GetAllBoardPositions())
        {
            if (position.isOccupied && position.occupyingPiece.CompareTag(opponentTag) && !IsInMill(position))
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Mill Detection Methods
    /// <summary>
    /// Finds all mills in a line involving a given position.
    /// </summary>
    /// <param name="position">The board position to check.</param>
    /// <param name="playerTag">The player's tag.</param>
    /// <param name="checkHorizontal">Check horizontal if true, else vertical.</param>
    /// <returns>List of mills found.</returns>
    List<List<BoardPosition>> FindMillsInLine(BoardPosition position, string playerTag, bool checkHorizontal)
    {
        List<BoardPosition> alignedPositions = CollectAlignedPositions(position, checkHorizontal);

        // Sort positions based on their coordinates to ensure correct sequence
        alignedPositions = alignedPositions.OrderBy(pos =>
            checkHorizontal ? pos.transform.position.x : pos.transform.position.y).ToList();

        List<List<BoardPosition>> mills = new List<List<BoardPosition>>();

        // Check for sequences of three consecutive positions occupied by the player's pieces
        for (int i = 0; i <= alignedPositions.Count - 3; i++)
        {
            if (alignedPositions[i].occupyingPiece != null && alignedPositions[i].occupyingPiece.CompareTag(playerTag) &&
                alignedPositions[i + 1].occupyingPiece != null && alignedPositions[i + 1].occupyingPiece.CompareTag(playerTag) &&
                alignedPositions[i + 2].occupyingPiece != null && alignedPositions[i + 2].occupyingPiece.CompareTag(playerTag))
            {
                List<BoardPosition> mill = new List<BoardPosition>
                {
                    alignedPositions[i],
                    alignedPositions[i + 1],
                    alignedPositions[i + 2]
                };
                mills.Add(mill);
            }
        }
        return mills;
    }

    bool CheckMillInLine(BoardPosition position, string playerTag, bool checkHorizontal)
    {
        List<List<BoardPosition>> mills = FindMillsInLine(position, playerTag, checkHorizontal);
        return mills.Count > 0;
    }

    List<List<BoardPosition>> GetAllMillsInvolvingPosition(BoardPosition position, string playerTag)
    {
        List<List<BoardPosition>> mills = new List<List<BoardPosition>>();

        // Check horizontal and vertical mills
        mills.AddRange(FindMillsInLine(position, playerTag, true));
        mills.AddRange(FindMillsInLine(position, playerTag, false));

        // Filter mills to only those that include the current position
        mills = mills.Where(mill => mill.Contains(position)).ToList();

        return mills;
    }

    List<BoardPosition> CollectAlignedPositions(BoardPosition startPosition, bool checkHorizontal)
    {
        List<BoardPosition> alignedPositions = new List<BoardPosition> { startPosition };

        // Traverse in the negative direction
        TraverseInDirection(startPosition, checkHorizontal, -1, ref alignedPositions);

        // Traverse in the positive direction
        TraverseInDirection(startPosition, checkHorizontal, 1, ref alignedPositions);

        // Sort positions based on their coordinates to ensure correct sequence
        alignedPositions = alignedPositions.OrderBy(pos =>
            checkHorizontal ? pos.transform.position.x : pos.transform.position.y).ToList();

        return alignedPositions;
    }

    void TraverseInDirection(BoardPosition currentPosition, bool checkHorizontal, int direction, ref List<BoardPosition> alignedPositions)
    {
        float tolerance = 0.01f;
        Vector3 currentPos = currentPosition.transform.position;
        foreach (var adjacent in currentPosition.adjacentPositions)
        {
            Vector3 adjacentPos = adjacent.transform.position;
            bool isAligned = checkHorizontal
                ? Mathf.Abs(currentPos.y - adjacentPos.y) < tolerance
                : Mathf.Abs(currentPos.x - adjacentPos.x) < tolerance;

            bool isCorrectDirection = checkHorizontal
                ? Mathf.Sign(adjacentPos.x - currentPos.x) == direction
                : Mathf.Sign(adjacentPos.y - currentPos.y) == direction;

            if (isAligned && isCorrectDirection)
            {
                if (!alignedPositions.Contains(adjacent))
                {
                    alignedPositions.Add(adjacent);
                    TraverseInDirection(adjacent, checkHorizontal, direction, ref alignedPositions);
                }
                break;
            }
        }
    }
    #endregion

    #region Piece Spawn Methods
    /// <summary>
    /// Spawns all pieces for both players at the start of the game.
    /// </summary>
    public void SpawnAllPiecesAtStart()
    {
        Vector3 player1StartPosition = Camera.main.ScreenToWorldPoint(player1SpawnUI.position);
        Vector3 player2StartPosition = Camera.main.ScreenToWorldPoint(player2SpawnUI.position);

        player1StartPosition.z = 0;
        player2StartPosition.z = 0;

        float spacing = 0.5f;
        float scaleOfPieces = 0.35f;

        // Spawn pieces for Player 1
        for (int i = GameManager.Instance.GetMaxPiecesByPlayer() - 1; i >= 0; i--)
        {
            Vector3 player1Position = player1StartPosition + new Vector3(i * spacing, 0f, 0f);
            GameObject piecePlayer1 = Instantiate(piecePrefabPlayer1, player1Position, Quaternion.identity, transform);
            piecePlayer1.transform.localScale = new Vector3(scaleOfPieces, scaleOfPieces, scaleOfPieces);
            Piece p1 = piecePlayer1.GetComponent<Piece>();
            p1.Color(Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId));
            player1PiecesQueue.Enqueue(p1);
            allPieces.Add(p1);
        }

        // Spawn pieces for Player 2
        for (int i = GameManager.Instance.GetMaxPiecesByPlayer() - 1; i >= 0; i--)
        {
            Vector3 player2Position = player2StartPosition - new Vector3(i * spacing, 0f, 0f);
            GameObject piecePlayer2 = Instantiate(piecePrefabPlayer2, player2Position, Quaternion.identity, transform);
            piecePlayer2.transform.localScale = new Vector3(scaleOfPieces, scaleOfPieces, scaleOfPieces);
            Piece p2 = piecePlayer2.GetComponent<Piece>();
            p2.Color(Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId));
            player2PiecesQueue.Enqueue(p2);
            allPieces.Add(p2);
        }
    }
    #endregion

    #region Utility Methods
    int GetPieceCountForPlayer(bool isPlayer1)
    {
        int count = 0;
        foreach (Piece piece in allPieces)
        {
            if (piece.CompareTag(isPlayer1 ? "Player1Piece" : "Player2Piece"))
            {
                count++;
            }
        }
        return count;
    }

    public void ScaleUpDownPiecesForMillOnly(List<Piece> piecesAnimation)
    {
        foreach (var piece in piecesAnimation)
        {
            Sequence seq = DOTween.Sequence();
            seq.SetLoops(-1);
            seq.SetId("PiecesScaleUpDown");
            seq.OnKill(() =>
            {
                piece.ResetVisual();
            });
            seq.Append(piece.transform.DOBlendableScaleBy(new Vector3(.1f, .1f, .1f), .3f));
            seq.Append(piece.transform.DOBlendableScaleBy(new Vector3(-.1f, -.1f, -.1f), .3f));
        }
    }
    #endregion
}
