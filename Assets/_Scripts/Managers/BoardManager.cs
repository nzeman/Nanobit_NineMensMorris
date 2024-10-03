using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    #region Singleton
    private static BoardManager _Instance;
    public static BoardManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<BoardManager>();
            return _Instance;
        }
    }
    #endregion

    public GameObject pointPrefab;
    public int numberOfRings = 3;
    public float spacing = 2f;
    public float lineWidth = 0.1f;

    public List<BoardPosition> allBoardPositions = new List<BoardPosition>();
    private List<List<BoardPosition>> ringPoints = new List<List<BoardPosition>>();

    public Material normalLineMaterial;
    public Material millLineMaterial;  
    private List<LineRenderer> lines = new List<LineRenderer>();

    public List<List<BoardPosition>> activeMills = new List<List<BoardPosition>>();


    public Sprite boardSprite;

    void Start()
    {
        numberOfRings = PlayerProfile.Instance.playerData.gameRulesData.numberOfRings;
        activeMills = new List<List<BoardPosition>>();
        InitializeBoard();
        SetAdjacentPositions(); // Set adjacent positions after initializing the board
        DrawLinesBetweenPoints();
        HighlightAllUnoccupiedBoardPositions();
        CreateBoardBackground();
    }

    // Create points for each ring dynamically
    void InitializeBoard()
    {
        for (int ring = 0; ring < numberOfRings; ring++)
        {
            float squareSize = spacing * (ring + 1);
            CreateRingPoints(squareSize, ring);
        }
    }

    void CreateBoardBackground()
    {
        GameObject background = new GameObject("BoardBackground");
        SpriteRenderer spriteRenderer = background.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = boardSprite;

        float scale = 0.8f + 0.6f * (numberOfRings - 1);

        background.transform.localScale = new Vector3(scale, scale, 1);
        background.transform.position = new Vector3(0, 0, -1);
    }





    // Instantiate points at corners and midpoints for each square
    void CreateRingPoints(float squareSize, int ringIndex)
    {
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

    // Set adjacent positions for each board point
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
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingLayerID = SortingLayer.NameToID("BoardLines");
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lines.Add(lineRenderer);
    }

    public void HighlightMillLine(List<BoardPosition> millPositions)
    {
        if (millPositions == null || millPositions.Count != 3)
            return;

        bool millExists = BoardManager.Instance.activeMills.Any(existingMill => AreMillsEqual(existingMill, millPositions));
        if (!millExists)
        {
            BoardManager.Instance.activeMills.Add(millPositions);
        }


        // Highlight the mill line
        Color targetColor = GameManager.Instance.IsPlayer1Turn()
            ? Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(true).colorId).color
            : Colors.Instance.GetColorById(PlayerProfile.Instance.GetGamePlayerData(false).colorId).color;

        for (int i = 0; i < millPositions.Count; i++)
        {
            BoardPosition start = millPositions[i];
            BoardPosition end = millPositions[(i + 1) % millPositions.Count];

            foreach (var line in lines)
            {
                if (IsLineConnectingPositions(line, start.transform.position, end.transform.position))
                {
                    line.startWidth = 0.3f;
                    line.endWidth = 0.3f;
                    line.material.color = targetColor; // Set the color statically
                }
            }
        }
    }

    bool AreMillsEqual(List<BoardPosition> mill1, List<BoardPosition> mill2)
    {
        return mill1.OrderBy(pos => pos.name).SequenceEqual(mill2.OrderBy(pos => pos.name));
    }

    public void ResetMillLines(List<BoardPosition> brokenMill)
    {
        // Remove the broken mill from the active mills list
        BoardPosition[] brokenMillArray = brokenMill.ToArray();
        activeMills.RemoveAll(mill => AreMillsEqual(mill, brokenMillArray.ToList()));

        // Reset the visual of the broken mill
        foreach (var line in lines)
        {
            if (IsLinePartOfMill(line, brokenMill))
            {
                line.material = normalLineMaterial;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
            }
        }
    }




    // Helper method to check if a line is part of the broken mill
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


    public void HighlightAllUnoccupiedBoardPositions()
    {
        //Debug.Log("HighlightAllUnoccupiedBoardPositions");
        foreach (BoardPosition position in allBoardPositions)
        {
            if (!position.isOccupied)
            {
                position.HighlightBoardPosition(true);
            }
            else
            {
                position.HighlightBoardPosition(false);
            }
        }

    }

    public void HideHightlightsFromBoardPositions()
    {
        //Debug.Log("HideHightlightsFromBoardPositions");
        foreach (BoardPosition position in allBoardPositions)
        {
            position.HighlightBoardPosition(false);
        }
    }
}
