using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the game board, including initialization, handling mills, and highlighting positions.
/// </summary>
public class BoardManager : MonoBehaviour
{
    #region Singleton
    private static BoardManager _Instance;
    /// <summary>
    /// Gets the singleton instance of the BoardManager.
    /// </summary>
    public static BoardManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<BoardManager>();
            return _Instance;
        }
    }
    #endregion

    #region Fields
    [SerializeField] private GameObject pointPrefab;
    private int numberOfRings;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private float lineWidth = 0.1f;

    private List<BoardPosition> allBoardPositions = new List<BoardPosition>();
    private List<List<BoardPosition>> ringPoints = new List<List<BoardPosition>>();

    [SerializeField] private Material normalLineMaterial;
    [SerializeField] private Material millLineMaterial;
    private List<LineRenderer> lines = new List<LineRenderer>();

    private List<List<BoardPosition>> activeMills = new List<List<BoardPosition>>();

    [SerializeField] private Sprite boardSprite;
    #endregion

    #region Initialization Methods
    public void Initialize()
    {
        // Get the number of rings from the player's game rules data
        numberOfRings = PlayerProfile.Instance.playerData.gameRulesData.numberOfRings;
        activeMills = new List<List<BoardPosition>>();
        InitializeBoard();
        SetAdjacentPositions();
        DrawLinesBetweenPoints();
        HighlightAllUnoccupiedBoardPositions();
        CreateBoardBackground();
    }

    /// <summary>
    /// Initializes the board by creating points for each ring.
    /// </summary>
    void InitializeBoard()
    {
        for (int ring = 0; ring < numberOfRings; ring++)
        {
            float squareSize = spacing * (ring + 1);
            CreateRingPoints(squareSize, ring);
        }
    }

    /// <summary>
    /// Creates the background sprite for the board.
    /// </summary>
    void CreateBoardBackground()
    {
        GameObject background = new GameObject("BoardBackground");
        SpriteRenderer spriteRenderer = background.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = boardSprite;

        float scale = 0.8f + 0.6f * (numberOfRings - 1);

        background.transform.localScale = new Vector3(scale, scale, 1);
        background.transform.position = new Vector3(0, 0, -1);
    }
    #endregion

    #region Board Setup Methods
    void CreateRingPoints(float squareSize, int ringIndex)
    {
        // Positions for the points in the ring
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-squareSize, squareSize),    // Top-left corner
            new Vector2(0, squareSize),              // Top-center
            new Vector2(squareSize, squareSize),     // Top-right corner
            new Vector2(squareSize, 0),              // Right-center
            new Vector2(squareSize, -squareSize),    // Bottom-right corner
            new Vector2(0, -squareSize),             // Bottom-center
            new Vector2(-squareSize, -squareSize),   // Bottom-left corner
            new Vector2(-squareSize, 0)              // Left-center
        };

        List<BoardPosition> currentRingPoints = new List<BoardPosition>();

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = Instantiate(pointPrefab, positions[i], Quaternion.identity);
            BoardPosition boardPos = point.GetComponent<BoardPosition>();
            boardPos.transform.SetParent(transform);
            boardPos.SetIndex(ringIndex * positions.Length + i); // Assign a unique index
            currentRingPoints.Add(boardPos);
            allBoardPositions.Add(boardPos);
        }

        ringPoints.Add(currentRingPoints);
    }

    /// <summary>
    /// Sets adjacent positions for each board point to establish connectivity.
    /// </summary>
    void SetAdjacentPositions()
    {
        for (int ring = 0; ring < ringPoints.Count; ring++)
        {
            List<BoardPosition> currentRing = ringPoints[ring];

            for (int i = 0; i < currentRing.Count; i++)
            {
                BoardPosition currentPosition = currentRing[i];
                BoardPosition nextPosition = currentRing[(i + 1) % currentRing.Count];
                BoardPosition prevPosition = currentRing[(i - 1 + currentRing.Count) % currentRing.Count];

                currentPosition.adjacentPositions.Add(nextPosition);
                currentPosition.adjacentPositions.Add(prevPosition);
            }

            if (ring > 0)
            {
                List<BoardPosition> previousRing = ringPoints[ring - 1];

                for (int i = 1; i < currentRing.Count; i += 2)
                {
                    BoardPosition currentMidpoint = currentRing[i];
                    BoardPosition previousMidpoint = previousRing[i];

                    currentMidpoint.adjacentPositions.Add(previousMidpoint);
                    previousMidpoint.adjacentPositions.Add(currentMidpoint);
                }
            }
        }
    }

    void DrawLinesBetweenPoints()
    {
        for (int ring = 0; ring < ringPoints.Count; ring++)
        {
            List<BoardPosition> currentRing = ringPoints[ring];

            for (int i = 0; i < currentRing.Count; i++)
            {
                int nextIndex = (i + 1) % currentRing.Count;
                CreateLine(currentRing[i].transform.position, currentRing[nextIndex].transform.position);
            }

            if (ring > 0)
            {
                List<BoardPosition> previousRing = ringPoints[ring - 1];

                for (int i = 1; i < currentRing.Count; i += 2)
                {
                    CreateLine(currentRing[i].transform.position, previousRing[i].transform.position);
                }
            }
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(transform);
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        lineRenderer.material = normalLineMaterial;
        lineRenderer.startWidth = lineWidth; // Default line width
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingLayerID = SortingLayer.NameToID("BoardLines");
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lines.Add(lineRenderer);
    }
    #endregion

    #region Mill Handling Methods

    public void HighlightMills(List<List<BoardPosition>> mills)
    {
        foreach (var millPositions in mills)
        {
            HighlightMillLine(millPositions);
        }
    }


    /// <summary>
    /// Highlights the lines forming a mill.
    /// </summary>
    /// <param name="millPositions">List of board positions forming the mill.</param>
    public void HighlightMillLine(List<BoardPosition> millPositions)
    {
        if (millPositions == null || millPositions.Count != 3)
            return;

        // Add the mill to the list of active mills if it's not already there
        bool millExists = activeMills.Any(existingMill => AreMillsEqual(existingMill, millPositions));
        if (!millExists)
        {
            activeMills.Add(millPositions);
        }

        // Determine if the mill is horizontal or vertical
        bool isHorizontal = Mathf.Abs(millPositions[0].transform.position.y - millPositions[1].transform.position.y) < 0.01f;

        // Sort mill positions to ensure correct order
        if (isHorizontal)
        {
            millPositions = millPositions.OrderBy(pos => pos.transform.position.x).ToList();
        }
        else
        {
            millPositions = millPositions.OrderBy(pos => pos.transform.position.y).ToList();
        }

        // Determine the target color based on the current player
        Color targetColor = GameManager.Instance.IsPlayer1Turn()
            ? Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId).color
            : Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId).color;

        float initialLineWidth = 0.3f;
        float reducedLineWidth = 0.15f;

        for (int i = 0; i < millPositions.Count - 1; i++)
        {
            BoardPosition start = millPositions[i];
            BoardPosition end = millPositions[i + 1];

            // Highlight the line between start and end
            HighlightLineBetweenPositions(start, end, targetColor, initialLineWidth, reducedLineWidth);
        }
    }


    private void HighlightLineBetweenPositions(BoardPosition start, BoardPosition end, Color color, float initialWidth, float reducedWidth)
    {
        foreach (var line in lines)
        {
            if (IsLineConnectingPositions(line, start.transform.position, end.transform.position))
            {
                // Set the line color
                line.material.color = color;

                // Set the line width to the initial thicker value
                line.startWidth = initialWidth;
                line.endWidth = initialWidth;

                // Animate the line width to reduce it over time
                DOVirtual.Float(initialWidth, reducedWidth, 0.5f, value =>
                {
                    line.startWidth = value;
                    line.endWidth = value;
                }).SetDelay(0.5f);

                // Break after finding the correct line
                break;
            }
        }
    }




    public void ResetMillLines(List<BoardPosition> millPositions)
    {
        if (millPositions == null || millPositions.Count != 3)
            return;

        // Remove the mill from active mills
        activeMills.RemoveAll(existingMill => AreMillsEqual(existingMill, millPositions));

        // Determine if the mill is horizontal or vertical
        bool isHorizontal = Mathf.Abs(millPositions[0].transform.position.y - millPositions[1].transform.position.y) < 0.01f;

        // Sort mill positions to ensure correct order
        if (isHorizontal)
        {
            millPositions = millPositions.OrderBy(pos => pos.transform.position.x).ToList();
        }
        else
        {
            millPositions = millPositions.OrderBy(pos => pos.transform.position.y).ToList();
        }

        // Reset the lines between positions in the mill
        for (int i = 0; i < millPositions.Count - 1; i++)
        {
            BoardPosition start = millPositions[i];
            BoardPosition end = millPositions[i + 1];

            // Reset the line between start and end
            ResetLineBetweenPositions(start, end);
        }
    }

    public void RemoveMills(List<List<BoardPosition>> millsToRemove)
    {
        foreach (var mill in millsToRemove)
        {
            // Remove the mill from the active mills list
            activeMills.RemoveAll(existingMill => AreMillsEqual(existingMill, mill));
        }
    }


    private void ResetLineBetweenPositions(BoardPosition start, BoardPosition end)
    {
        foreach (var line in lines)
        {
            if (IsLineConnectingPositions(line, start.transform.position, end.transform.position))
            {
                // Reset the line to default color and width
                line.material = normalLineMaterial;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;

                // Break after resetting the correct line
                break;
            }
        }
    }



    /// <summary>
    /// Checks if two mills are equal based on their positions.
    /// </summary>
    /// <param name="mill1">First mill to compare.</param>
    /// <param name="mill2">Second mill to compare.</param>
    /// <returns>True if mills are equal, false otherwise.</returns>
    public bool AreMillsEqual(List<BoardPosition> mill1, List<BoardPosition> mill2)
    {
        if (mill1.Count != mill2.Count)
            return false;

        // Compare the positions in the mills
        return mill1.OrderBy(pos => pos.name).SequenceEqual(mill2.OrderBy(pos => pos.name));
    }

    #endregion

    #region Board Position Highlight Methods
    public void HighlightAllUnoccupiedBoardPositions()
    {
        foreach (BoardPosition position in allBoardPositions)
        {
            position.HighlightBoardPosition(!position.isOccupied);
        }
    }

    public void HideHightlightsFromBoardPositions()
    {
        foreach (BoardPosition position in allBoardPositions)
        {
            position.HighlightBoardPosition(false);
        }
    }
    #endregion

    #region Utility Methods
    private bool IsLinePartOfMill(LineRenderer line, List<BoardPosition> millPositions)
    {
        // Check if the line connects any two positions in the mill
        for (int i = 0; i < millPositions.Count; i++)
        {
            BoardPosition start = millPositions[i];
            BoardPosition end = millPositions[(i + 1) % millPositions.Count];

            if (IsLineConnectingPositions(line, start.transform.position, end.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsLineConnectingPositions(LineRenderer line, Vector3 pos1, Vector3 pos2)
    {
        float tolerance = 0.01f;
        return (Vector3.Distance(line.GetPosition(0), pos1) < tolerance && Vector3.Distance(line.GetPosition(1), pos2) < tolerance) ||
               (Vector3.Distance(line.GetPosition(0), pos2) < tolerance && Vector3.Distance(line.GetPosition(1), pos1) < tolerance);
    }
    #endregion

    #region Public Getter Methods
    /// <summary>
    /// Gets the list of all board positions.
    /// </summary>
    public List<BoardPosition> GetAllBoardPositions()
    {
        return allBoardPositions;
    }

    /// <summary>
    /// Gets the number of rings on the board.
    /// </summary>
    public int GetNumberOfRings()
    {
        return numberOfRings;
    }

    /// <summary>
    /// Gets the list of active mills.
    /// </summary>
    public List<List<BoardPosition>> GetActiveMills()
    {
        return activeMills;
    }
    #endregion
}
