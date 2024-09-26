using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject pointPrefab;
    public Material lineMaterial;
    public int numberOfRings = 3;
    public float spacing = 2f;
    public float lineWidth = 0.1f;

    private List<List<BoardPosition>> ringPoints = new List<List<BoardPosition>>();
    private List<GameObject> lines = new List<GameObject>();

    void Start()
    {
        InitializeBoard();
        DrawLinesBetweenPoints();
    }

    // Create points for each ring dynamically
    void InitializeBoard()
    {
        for (int ring = 0; ring < numberOfRings; ring++)
        {
            float squareSize = spacing * (ring + 1);
            CreateRingPoints(squareSize);
        }
    }

    // Instantiate points at corners and midpoints for each square
    void CreateRingPoints(float squareSize)
    {
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-squareSize, squareSize),
            new Vector2(0, squareSize),
            new Vector2(squareSize, squareSize),
            new Vector2(squareSize, 0),
            new Vector2(squareSize, -squareSize),
            new Vector2(0, -squareSize),
            new Vector2(-squareSize, -squareSize),
            new Vector2(-squareSize, 0)
        };

        List<BoardPosition> currentRingPoints = new List<BoardPosition>();

        foreach (var position in positions)
        {
            GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
            BoardPosition boardPos = point.GetComponent<BoardPosition>();
            currentRingPoints.Add(boardPos);
        }

        ringPoints.Add(currentRingPoints);
    }

    // Draw lines connecting points in the same ring and between rings
    void DrawLinesBetweenPoints()
    {
        for (int ring = 0; ring < ringPoints.Count; ring++)
        {
            List<BoardPosition> currentRing = ringPoints[ring];

            // Connect points within the same ring
            for (int i = 0; i < currentRing.Count; i++)
            {
                int nextIndex = (i + 1) % currentRing.Count;
                CreateLine(currentRing[i].transform.position, currentRing[nextIndex].transform.position);
            }

            // Connect midpoints between this ring and the previous ring
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
