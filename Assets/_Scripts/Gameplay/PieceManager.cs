using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PieceManager : MonoBehaviour
{
    #region Singleton
    private static PieceManager _Instance;
    public static PieceManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<PieceManager>();
            return _Instance;
        }
    }
    #endregion

    public GameObject piecePrefabPlayer1;
    public GameObject piecePrefabPlayer2;
    public LayerMask boardLayer;
    public LayerMask pieceLayer;

    [SerializeField] private BoardPosition selectedPiecePosition;
    public List<Piece> allPieces;

    private Queue<Piece> player1PiecesQueue = new Queue<Piece>();
    private Queue<Piece> player2PiecesQueue = new Queue<Piece>();

    // UI GameObjects for the spawn positions
    public RectTransform player1SpawnUI;
    public RectTransform player2SpawnUI;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.canInteract == false) return;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleBoardPointClick(mousePosition);
        }
    }

    public BoardPosition GetSelectedPiecePosition()
    {
        return selectedPiecePosition;
    }

    public void HandleBoardPointClick(Vector2 mousePosition)
    {
        RaycastHit2D hitPiece = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, pieceLayer);
        RaycastHit2D hitBoard = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, boardLayer);

        // mill removal 
        if (GameManager.Instance.currentPhase == GameManager.GamePhase.MillRemoval && hitPiece.collider != null)
        {
            if (hitBoard.collider == null)
            {
                // the pieces that was not yet placed (for example in placing phase)
                return;
            }

            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();
            HandleMillRemoval(boardPosition);
        }

        else if (hitBoard.collider != null)
        {
            BoardPosition boardPosition = hitBoard.collider.GetComponent<BoardPosition>();
            if (GameManager.Instance.currentPhase == GameManager.GamePhase.Placing)
            {
                HandlePlacingPhase(boardPosition);
            }
            else if (GameManager.Instance.currentPhase == GameManager.GamePhase.Moving)
            {
                HandleMovingPhase(boardPosition);
            }
        }
    }

    public void RefreshPiecesLeftUi()
    {
        int countPlayer1 = GameManager.Instance.maxPiecesPerPlayer - GameManager.Instance.piecesPlacedPlayer1;
        int countPlayer2 = GameManager.Instance.maxPiecesPerPlayer - GameManager.Instance.piecesPlacedPlayer2;
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

    void HandlePlacingPhase(BoardPosition position)
    {
        if (!position.isOccupied)
        {
            GameUIManager.Instance.gameView.SetTopText("");
            bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();
            GameManager.Instance.SavePreviousPhase();
            GameManager.Instance.canInteract = false;
            BoardManager.Instance.HideHightlightsFromBoardPositions();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPiecePlacedClick);
            Piece pieceToPlace = isPlayer1Turn ? player1PiecesQueue.Dequeue() : player2PiecesQueue.Dequeue();

            Debug.Log($"Placing piece on {position.name}...");

            pieceToPlace.transform.DOMove(position.transform.position, GameManager.Instance.timeToMovePieceToBoardInPlacingPhase).OnComplete(() =>
            {
                StartCoroutine(OnPieceReachedPositionInPlacingPhase(pieceToPlace, position, isPlayer1Turn));
            });


        }
    }

    public IEnumerator OnPieceReachedPositionInPlacingPhase(Piece pieceToPlace, BoardPosition position, bool isPlayer1Turn)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceReachedPositionWhenPlacing);
        yield return new WaitForSecondsRealtime(.2f);

        position.OccupyPosition(pieceToPlace);
        pieceToPlace.boardPosition = position;
        Debug.Log($"Piece reached it's new position...");

        bool millFormed = CheckForMill(position, isPlayer1Turn);

        // Highlight the mill if it's formed
        if (millFormed)
        {
            List<BoardPosition> millPositions = GetMillPositions(position, isPlayer1Turn);
            BoardManager.Instance.HighlightMillLine(millPositions);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onMillFormed);
        }
        GameManager.Instance.currentPhase = GameManager.Instance.gamePhasePriorToMillRemoval;
        GameManager.Instance.OnPieceReachedItsPositionOnBoard(millFormed);
        GameManager.Instance.canInteract = true;
        RefreshPiecesLeftUi();
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
                GameUIManager.Instance.gameView.SetTopText("Flying phase! Move your piece to any unoccupied spot.");
                BoardManager.Instance.HighlightAllUnoccupiedBoardPositions();
            }
            else
            {
                // Highlight adjacent valid moves only for the selected piece
                HighlightAdjacentPositions(selectedPiecePosition);
                GameUIManager.Instance.gameView.SetTopText("Move your piece to an adjacent unoccupied spot.");
            }

            // Play sound and highlight the selected piece
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);

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
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
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
        bool isFlyingPhase = GetPieceCountForPlayer(GameManager.Instance.IsPlayer1Turn()) == 3;

        if (isFlyingPhase || selectedPiecePosition.IsAdjacent(targetPosition))
        {
            GameUIManager.Instance.gameView.SetTopText("");
            GameManager.Instance.canInteract = false;

            // Stop scaling and reset visuals for all pieces once a move is confirmed
            ResetAllScalingAndVisuals();

            MovePiece(selectedPiecePosition, targetPosition);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceMove);
            BoardManager.Instance.HideHightlightsFromBoardPositions();

            DOVirtual.DelayedCall(GameManager.Instance.timeToMovePieceToBoardPositionInMovingPhase, () =>
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

        GameUIManager.Instance.gameView.SetTopText("Select a highlighted piece that can move");
        GameUIManager.Instance.gameView.ShowBottomText("This piece cannot be moved!");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);

        BoardManager.Instance.HideHightlightsFromBoardPositions();
        //position.occupyingPiece.ResetVisual();
        PieceManager.Instance.HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves();
        //GameManager.Instance.UponNeedToSelectAPiece();
    }


    private void HandleInvalidMove()
    {
        Debug.Log("Invalid move: Not adjacent and not in flying phase.");
        GameUIManager.Instance.gameView.ShowBottomText("Selected piece cannot go to this position!");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
    }


    public IEnumerator OnPieceReachPositionInMovingPhase(BoardPosition position)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceReachedPositionWhenPlacing);
        yield return new WaitForSecondsRealtime(0.2f);

        bool millFormed = CheckForMill(position, GameManager.Instance.IsPlayer1Turn());
        if (millFormed)
        {
            List<BoardPosition> millPositions = GetMillPositions(position, GameManager.Instance.IsPlayer1Turn());
            BoardManager.Instance.HighlightMillLine(millPositions);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onMillFormed);
        }

        GameManager.Instance.OnPieceReachedItsPositionOnBoard(millFormed);
        BoardManager.Instance.HideHightlightsFromBoardPositions();
    }


    public void SpawnAllPiecesAtStart()
    {
        Vector3 player1StartPosition = Camera.main.ScreenToWorldPoint(player1SpawnUI.position);
        Vector3 player2StartPosition = Camera.main.ScreenToWorldPoint(player2SpawnUI.position);

        player1StartPosition.z = 0;
        player2StartPosition.z = 0;

        float spacing = 0.5f;
        float scaleOfPieces = 0.35f;

        // Spawn pieces for Player 1
        for (int i = GameManager.Instance.maxPiecesPerPlayer - 1; i >= 0; i--)
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
        for (int i = GameManager.Instance.maxPiecesPerPlayer - 1; i >= 0; i--)
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

    void MovePiece(BoardPosition from, BoardPosition to)
    {
        Piece piece = from.occupyingPiece;
        piece.OutlinePiece(false);

        from.ClearPosition();
        piece.boardPosition = to;

        to.OccupyPosition(piece);
        piece.transform.DOMove(to.transform.position, GameManager.Instance.timeToMovePieceToBoardPositionInMovingPhase);

        selectedPiecePosition = null;
    }


    void SelectPiece(BoardPosition position)
    {
        if(selectedPiecePosition == position)
        {
            // do not reselect the same one over again
            return;
        }

        foreach (var piece in allPieces)
        {
            piece.OutlinePiece(false);
        }
        string playerTag = GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece";
        foreach (var piece in allPieces)
        {
            if (piece.CompareTag(playerTag))
            {
                bool hasValidMove = piece.boardPosition.adjacentPositions.Any(x => !x.isOccupied);
                piece.ScaleUp(hasValidMove);
            }
        }

        selectedPiecePosition = position;
        //Debug.Log("Can this piece fly? " + IsFlyingPhaseForCurrentTurnPlayer());
        if (IsFlyingPhaseForCurrentTurnPlayer())
        {
            foreach (var positionOnBoard in BoardManager.Instance.allBoardPositions)
            {
                if (!positionOnBoard.isOccupied)
                {
                    positionOnBoard.HighlightBoardPosition(true);
                }
                else
                {
                    positionOnBoard.HighlightBoardPosition(false);
                }
            }
            //DeselectAllPieces();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);
            GameUIManager.Instance.gameView.SetTopText("Move your piece to an adjacent unoccupied spot!");
            position.occupyingPiece.OutlinePiece(true);
            position.occupyingPiece.ScaleUp(true);
        }
        else
        {

            // no flying
            if (CountPiecesOfAvailableAdjacentSpots(selectedPiecePosition) == 0)
            {
                Debug.Log("Selected piece has no adjacent position that is not occupied, and there is no flying. You cannot move this piece!");
                //GameUIManager.Instance.gameView.SetTopText("NO POSSIBLE MOVES WITH THIS PIECE, SELECT ANOTHER ONE!");
                //GameUIManager.Instance.gameView.ShowBottomText("No possible moves with this piece!");
                ShowNoValidMovesFeedback(selectedPiecePosition);
                // TODO add different outline or something here, so it's more clear that you cannot move it
                selectedPiecePosition.occupyingPiece.ResetVisual();
            }
            else
            {
                //DeselectAllPieces();
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);
                position.occupyingPiece.OutlinePiece(true);
                position.occupyingPiece.ScaleUp(true);
                GameUIManager.Instance.gameView.SetTopText("Move your piece to an adjacent unoccupied spot");
            }
        }
        //Debug.Log("Selected piece at: " + position.name);
    }

    public int CountPiecesOfAvailableAdjacentSpots(BoardPosition position)
    {
        int countAdjacent = 0;
        foreach (var adjacentPosition in position.adjacentPositions)
        {
            if (!adjacentPosition.isOccupied)
            {
                adjacentPosition.HighlightBoardPosition(true);
                countAdjacent++;
            }
            else
            {
                adjacentPosition.HighlightBoardPosition(false);
            }
        }

        return countAdjacent;
    }

    void DeselectAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.ResetVisual();
        }
    }

    List<BoardPosition> GetMillPositions(BoardPosition position, bool isPlayer1Turn)
    {
        string playerTag = isPlayer1Turn ? "Player1Piece" : "Player2Piece";

        List<BoardPosition> horizontalMill = GetMillInLinePositions(position, playerTag, true);
        if (horizontalMill != null)
            return horizontalMill;

        List<BoardPosition> verticalMill = GetMillInLinePositions(position, playerTag, false);
        if (verticalMill != null)
            return verticalMill;

        return null; // No mill formed
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

                // Look for a second adjacent piece in the same line
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

        return linePositions.Count == 3; // Return true if exactly 3 pieces form a mill
    }


    List<BoardPosition> GetMillInLinePositions(BoardPosition position, string playerTag, bool checkHorizontal)
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

        return linePositions.Count == 3 ? linePositions : null; // Return the list of positions if mill is formed, otherwise null
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

        if (position.occupyingPiece.CompareTag(GameManager.Instance.IsPlayer1Turn() ? "Player2Piece" : "Player1Piece"))
        {
            if (!IsInMill(position) || AllOpponentPiecesInMill() || IsEndgameScenario())
            {
                GameManager.Instance.canInteract = false;
                allPieces.Remove(position.occupyingPiece);
                GameObject pieceToDestroy = position.occupyingPiece.gameObject;
                pieceToDestroy.transform.DOScale(0f, .3f).OnComplete(() =>
                {
                    Destroy(pieceToDestroy);
                });

                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceRemovedFromBoardByMill);

                position.ClearPosition();
                BoardManager.Instance.ResetMillLines();
                GameManager.Instance.PieceRemovedFromBoardByPlayer();

                if (GameManager.Instance.IsGameOverByNoValidMoves())
                {
                    //GameManager.Instance.DeclareWinner();
                    return;
                }
            }
            else
            {
                Debug.Log("Cannot remove a piece that is in a mill unless all opponent pieces are in mills.");
                GameUIManager.Instance.gameView.ShowBottomText("Cannot remove a piece that is in a mill unless all opponent pieces are in mills!");
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
            }
        }
        else
        {
            Debug.Log("You cant remove your own piece!");
            GameUIManager.Instance.gameView.ShowBottomText("You can't remove your own piece!");
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
        }
    }




    bool IsEndgameScenario()
    {
        int player1PieceCount = GetPieceCountForPlayer(true);
        int player2PieceCount = GetPieceCountForPlayer(false);
        return player1PieceCount <= 3 || player2PieceCount <= 3;
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
        foreach (var position in BoardManager.Instance.allBoardPositions)
        {
            if (position.isOccupied && position.occupyingPiece.CompareTag(opponentTag) && !IsInMill(position))
            {
                return false;
            }
        }
        return true;
    }

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


    public void UnhighlightAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.ResetVisual();
        }
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

}
