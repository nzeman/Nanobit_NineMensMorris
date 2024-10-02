using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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

    void HandlePlacingPhase(BoardPosition position)
    {
        if (!position.isOccupied)
        {
            bool isPlayer1Turn = GameManager.Instance.IsPlayer1Turn();

            GameManager.Instance.SavePreviousPhase();
            GameManager.Instance.canInteract = false;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPiecePlacedClick);

            Piece pieceToPlace = isPlayer1Turn ? player1PiecesQueue.Dequeue() : player2PiecesQueue.Dequeue();
            pieceToPlace.transform.DOMove(position.transform.position, 0.3f).OnComplete(() =>
            {

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
                GameManager.Instance.PiecePlacedByPlayer(millFormed);
                GameManager.Instance.canInteract = true;
            });


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
                GameManager.Instance.canInteract = false;
                MovePiece(selectedPiecePosition, position);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceMove);

                DOVirtual.DelayedCall(0.3f, () =>
                {
                    List<BoardPosition> millPositions = GetMillPositions(position, GameManager.Instance.IsPlayer1Turn());
                    GameManager.Instance.PiecePlacedByPlayer(millPositions != null);

                    BoardManager.Instance.HideHightlightsFromBoardPositions();
                });
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
            }
        }
    }

    public void SpawnAllPiecesAtStart()
    {
        // Convert the UI RectTransform positions (in screen space) to world space
        Vector3 player1StartPosition = Camera.main.ScreenToWorldPoint(player1SpawnUI.position);
        Vector3 player2StartPosition = Camera.main.ScreenToWorldPoint(player2SpawnUI.position);

        // Ensure the Z-axis is properly set for 2D world space (make it 0)
        player1StartPosition.z = 0;
        player2StartPosition.z = 0;

        float spacing = 0.3f; // Adjust the spacing appropriately

        // Spawn pieces for Player 1
        for (int i = GameManager.Instance.maxPiecesPerPlayer - 1; i >= 0; i--)
        {
            Vector3 player1Position = player1StartPosition + new Vector3(i * spacing, 0f, 0f);
            GameObject piecePlayer1 = Instantiate(piecePrefabPlayer1, player1Position, Quaternion.identity);
            piecePlayer1.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            Piece p1 = piecePlayer1.GetComponent<Piece>();
            p1.Color(Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId));
            player1PiecesQueue.Enqueue(p1);
            allPieces.Add(p1);
        }

        // Spawn pieces for Player 2
        for (int i = GameManager.Instance.maxPiecesPerPlayer - 1; i >= 0; i--)
        {
            Vector3 player2Position = player2StartPosition - new Vector3(i * spacing, 0f, 0f);
            GameObject piecePlayer2 = Instantiate(piecePrefabPlayer2, player2Position, Quaternion.identity);
            piecePlayer2.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
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
        piece.HighlightPiece(false);

        from.ClearPosition();
        piece.boardPosition = to;

        to.OccupyPosition(piece);
        piece.transform.DOMove(to.transform.position, .3f);

        selectedPiecePosition = null;
    }


    void SelectPiece(BoardPosition position)
    {
        DeselectAllPieces();
        selectedPiecePosition = position;
        position.occupyingPiece.HighlightPiece(true);

        Debug.Log("Can this piece fly? " + IsFlyingPhaseForCurrentTurnPlayer());
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
            AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);
        }
        else
        {

            // no flying
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

            if (countAdjacent == 0)
            {
                Debug.Log("Selected piece has no adjacent position that is not occupied, and there is no flying. You cannot move this piece!");
                // TODO add different outline or something here, so it's more clear that you cannot move it
                selectedPiecePosition.occupyingPiece.HighlightPiece(false);
            }
            else
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onPieceSelected);
            }
        }
        Debug.Log("Selected piece at: " + position.name);
    }

    void DeselectAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.HighlightPiece(false);
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

                if (GameManager.Instance.CheckLossByNoValidMoves())
                {
                    GameManager.Instance.DeclareWinner(GameManager.Instance.IsPlayer1Turn());
                    return;
                }
            }
            else
            {
                Debug.Log("Cannot remove a piece that is in a mill unless all opponent pieces are in mills.");
                AudioManager.Instance.PlaySFX(AudioManager.Instance.audioClipDataHolder.onIllegalMove);
            }
        }
        else
        {
            Debug.Log("Piece does not belong to the opponent.");
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
            piece.HighlightPiece(false);
        }
    }

    public void ScaleUpDownPieces(List<Piece> piecesAnimation)
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
