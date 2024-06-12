using UnityEngine;
using System.Collections.Generic;

public class Grid5 : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public LayerMask slowZoneMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public int defaultPenalty = 1;
    public int unwalkablePenalty = 1000000; // 매우 높은 가중치로 설정
    public int slowZonePenalty = 10;

    public Node5[,] grid5;

    public float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid5();
    }

    void CreateGrid5()
    {
        grid5 = new Node5[gridSizeX, gridSizeY];
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

                grid5[x, y] = new Node5(worldPoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node5> GetNeighbours(Node5 node5)
    {
        List<Node5> neighbours = new List<Node5>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node5.gridX + x;
                int checkY = node5.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid5[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node5 NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid5[x, y];
    }

    public float GetNodeDiameter()
    {
        return nodeDiameter;
    }
}
