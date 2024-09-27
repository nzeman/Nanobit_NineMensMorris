using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject pointPrefab;
    public Material lineMaterial;
    public int numberOfRings = 3;
    public float spacing = 2f;
    public float lineWidth = 0.1f;

    public List<BoardPosition> allBoardPositions = new List<BoardPosition>();
    private List<List<BoardPosition>> ringPoints = new List<List<BoardPosition>>();
    private List<GameObject> lines = new List<GameObject>();

    void Start()
    {
        InitializeBoard();
        SetAdjacentPositions(); // Set adjacent positions after initializing the board
        DrawLinesBetweenPoints();
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
        // Loop through each ring and set adjacent points
        for (int ring = 0; ring < ringPoints.Count; ring++)
        {
            List<BoardPosition> currentRing = ringPoints[ring];

            // Set adjacent positions within the same ring (circular adjacency)
            for (int i = 0; i < currentRing.Count; i++)
            {
                BoardPosition currentPosition = currentRing[i];
                BoardPosition nextPosition = currentRing[(i + 1) % currentRing.Count];
                BoardPosition prevPosition = currentRing[(i - 1 + currentRing.Count) % currentRing.Count];

                currentPosition.adjacentPositions.Add(nextPosition);
                currentPosition.adjacentPositions.Add(prevPosition);
            }

            // Set adjacent positions between rings (midpoints)
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

    // Draw lines connecting points in the same ring and between rings
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

    // Create a line between two points
    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lines.Add(lineObj);
    }
}
