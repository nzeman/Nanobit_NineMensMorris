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
            bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();

            GameManager.Instance.SavePreviousPhase();
            GameManager.Instance.canInteract = false;
            BoardManager.Instance.HideHightlightsFromBoardPositions();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPiecePlacedClick);

            Piece pieceToPlace = isPlayer1Turn ? player1PiecesQueue.Dequeue() : player2PiecesQueue.Dequeue();
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

    public void HighlightPiecesByPlayerWhichHeCanSelectAndThatHaveValidMoves()
    {
        //return;
        string playerTag = GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece";
        foreach (var piece in allPieces)
        {
            if (piece.CompareTag(playerTag))
            {
                bool hasValidMove = piece.boardPosition.adjacentPositions.Any(x => !x.isOccupied);
                piece.ScaleUp(hasValidMove);
            }
            else
            {
                piece.ResetVisual();
            }
        }
    }





    public bool IsFlyingPhaseForCurrentTurnPlayer()
    {
        return GetPieceCountForPlayer(GameManager.Instance.IsPlayer1Turn()) == 3;
    }

    void HandleMovingPhase(BoardPosition position)
    {
        if (selectedPiecePosition == null && position.isOccupied)
        {
            if (position.occupyingPiece.CompareTag(GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece"))
            {
                BoardManager.Instance.HideHightlightsFromBoardPositions();
                SelectPiece(position);

            }
        }
        else if (selectedPiecePosition != null && !position.isOccupied)
        {
            bool isFlyingPhase = GetPieceCountForPlayer(GameManager.Instance.IsPlayer1Turn()) == 3;

            if (isFlyingPhase || selectedPiecePosition.IsAdjacent(position))
            {
                GameUIManager.Instance.gameView.SetTopText("");
                GameManager.Instance.canInteract = false;
                MovePiece(selectedPiecePosition, position);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceMove);
                DOVirtual.DelayedCall(GameManager.Instance.timeToMovePieceToBoardPositionInMovingPhase, () =>
                {
                    StartCoroutine(OnPieceReachPositionInMovingPhase(position));
                    
                }).SetUpdate(true);
            }
            else
            {
                Debug.Log("Invalid move: Not adjacent and not in flying phase.");
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
            }
        }
        else if (selectedPiecePosition != null && position.isOccupied)
        {
            if (position.occupyingPiece.CompareTag(GameManager.Instance.IsPlayer1Turn() ? "Player1Piece" : "Player2Piece"))
            {
                BoardManager.Instance.HideHightlightsFromBoardPositions();
                SelectPiece(position);
                GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECE BY CLICKING ON AN UNOCCUPIED SPOT!");
            }
        }
    }

    public IEnumerator OnPieceReachPositionInMovingPhase(BoardPosition position)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceReachedPositionWhenPlacing);
        yield return new WaitForSecondsRealtime(0.2f);

        List<BoardPosition> millPositions = GetMillPositions(position, GameManager.Instance.IsPlayer1Turn());
        GameManager.Instance.OnPieceReachedItsPositionOnBoard(millPositions != null);
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





    /*
    void PlacePiece(BoardPosition position, bool isPlayer1Turn)
    {
        GameObject piecePrefab = isPlayer1Turn ? piecePrefabPlayer1 : piecePrefabPlayer2;

        Vector3 spawnPos = new Vector3(0f, 10f, 0f);
        GameObject piece = Instantiate(piecePrefab, spawnPos, Quaternion.identity);
        Piece p = piece.GetComponent<Piece>();

        p.boardPosition = position;

        piece.transform.DOJump(position.transform.position, 5f, 1, .3f);
        position.OccupyPosition(p);
        allPieces.Add(p);
    }*/


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
        if(selectedPiecePosition == position.occupyingPiece)
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
            GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECE BY CLICKING ON AN UNOCCUPIED SPOT!");
            position.occupyingPiece.OutlinePiece(true);
            position.occupyingPiece.ScaleUp(true);
        }
        else
        {

            // no flying
            if (CountPiecesOfAvailableAdjacentSpots(position) == 0)
            {
                Debug.Log("Selected piece has no adjacent position that is not occupied, and there is no flying. You cannot move this piece!");
                GameUIManager.Instance.gameView.ShowBottomText("No possible moves with this piece!");
                // TODO add different outline or something here, so it's more clear that you cannot move it
                selectedPiecePosition.occupyingPiece.ResetVisual();
            }
            else
            {
                //DeselectAllPieces();
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);
                position.occupyingPiece.OutlinePiece(true);
                position.occupyingPiece.ScaleUp(true);
                GameUIManager.Instance.gameView.SetTopText("MOVE YOUR PIECE BY CLICKING ON AN UNOCCUPIED SPOT!");
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
                GameManager.Instance.PieceRemoved();

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
            Debug.Log("Piece does not belong to the opponent.");
            GameUIManager.Instance.gameView.ShowBottomText("Piece does not belong to the opponent!");
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
