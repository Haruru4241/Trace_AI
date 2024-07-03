using UnityEngine;
using System.Collections.Generic;

public class Grid9 : MonoBehaviour
{
    public GameManager gameManager;
    public LayerMask unwalkableMask;
    public LayerMask slowZoneMask;
    public Vector2 gridWorldSize;
    public float nodeRadius = 0.5f;
    public int defaultPenalty = 1;
    public int unwalkablePenalty = 1000000;
    public int slowZonePenalty = 10;

    public Node9[,] grid9;

    public float nodeDiameter = 1f;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        if (gameManager != null)
        {
            Transform referenceObject = gameManager.transform; // 참조 오브젝트
            if (referenceObject != null)
            {
                gridWorldSize = new Vector2(referenceObject.localScale.x, referenceObject.localScale.z);
            }
        }

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }


    void CreateGrid()
    {
        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            Debug.LogError("Grid size is invalid. Ensure gridWorldSize and nodeRadius are set correctly.");
            return;
        }

        grid9 = new Node9[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                int movementPenalty = defaultPenalty;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, unwalkableMask))
                {
                    movementPenalty = unwalkablePenalty;
                }
                else if (Physics.Raycast(ray, out hit, 100, slowZoneMask))
                {
                    movementPenalty = slowZonePenalty;
                }

                grid9[x, y] = new Node9(worldPoint, x, y, movementPenalty);
            }
        }
    }


    public List<Node9> GetNeighbours(Node9 node)
    {
        List<Node9> neighbours = new List<Node9>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid9[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node9 NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid9[x, y];
    }
}
