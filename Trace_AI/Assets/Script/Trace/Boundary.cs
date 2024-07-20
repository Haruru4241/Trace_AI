using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProceduralMap;

public class Boundary : MoveBase
{
    private Vector3 targetPosition;
    GameManager gameManager;
    BlockType[,] mapBlocksList;
    Dictionary<string, float[,]> weightTables;
    Vector2Int currentPosition;
    float[,] finalWeightTable;
    Vector2Int lastPosition;

    public override void Initialize()
    {
        base.Initialize();
        GameObject gameManagerObject = GameObject.Find("GameManager");
        gameManager = gameManagerObject.GetComponent<GameManager>();
        mapBlocksList = gameManager.getMapBlocksList();
        InitializeWeightTable();
        currentPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
    }

    public override void Enter()
    {
        lastPosition = new Vector2Int((int)ai.targetList.First().Key.position.x, (int)ai.targetList.First().Key.position.y);
        ArriveTargetPosition();
        Debug.Log($"{transform.name} 경계 상태 진입, 목표: {targetPosition}");
    }

    public override void Execute()
    {
        Vector2Int newPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        if (newPosition != currentPosition)
        {
            UpdateWeightMap("aiVisitedWeightTable", newPosition, 5, -1, 0);
            currentPosition = newPosition;
        }
        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.patrolThreshold)
        {
            Exit();
        }
    }

    private void UpdateFinalWeightTable()
    {
        int width = mapBlocksList.GetLength(0);
        int height = mapBlocksList.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                finalWeightTable[x, y] = 0;
                foreach (var table in weightTables.Values)
                {
                    finalWeightTable[x, y] += table[x, y];
                }
            }
        }
    }

    public override void Exit()
    {
        fsm.SetState<Patrol>();
        Debug.Log($"{transform.name} 경계 상태 탈출");
    }

    public override Vector3 ArriveTargetPosition()
    {
        Vector2Int currentPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        List<Vector2Int> endPoints = FindEndPointsBFS(currentPosition, 5);
        UpdateWeightMap("playerWeightTable", lastPosition, 5, 3, 0);
        UpdateFinalWeightTable();
        Vector2Int bestEndPoint = FindBestEndPoint(endPoints);
        targetPosition = new Vector3(bestEndPoint.x, transform.position.y, bestEndPoint.y);
        int width = mapBlocksList.GetLength(0);
        int height = mapBlocksList.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                weightTables["playerWeightTable"][x, y] = 0;
                weightTables["aiVisitedWeightTable"][x, y] = 0;
            }
        }
        Debug.Log($"{transform.name} 경계 목표 재설정: {targetPosition}");
        return targetPosition;
    }

    public override Vector3 TraceTargetPosition()
    {
        return targetPosition;
    }

    private void InitializeWeightTable()
    {
        int width = mapBlocksList.GetLength(0);
        int height = mapBlocksList.GetLength(1);
        weightTables = new Dictionary<string, float[,]>
        {
            { "playerWeightTable", new float[width, height] },
            { "aiVisitedWeightTable", new float[width, height] },
            { "mapWeightTable", new float[width, height] }
        };
        finalWeightTable = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                weightTables["playerWeightTable"][x, y] = 0;
                weightTables["aiVisitedWeightTable"][x, y] = 0;
                weightTables["mapWeightTable"][x, y] = GetInitialMapWeight(mapBlocksList[x, y]);
            }
        }
    }

    private float GetInitialMapWeight(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Floor:
                return 1;
            case BlockType.Door:
                return 5;
            default:
                return 0;
        }
    }

    private void UpdateWeightMap(string tableName, Vector2Int newPosition, int count, float weight, float decreasePerMove)
    {
        int[,] visited = new int[mapBlocksList.GetLength(0), mapBlocksList.GetLength(1)];
        Queue<(Vector2Int position, int movesLeft)> queue = new Queue<(Vector2Int, int)>();
        queue.Enqueue((newPosition, count));
        visited[newPosition.x, newPosition.y] = count;

        while (queue.Count > 0)
        {
            var (currentPosition, movesLeft) = queue.Dequeue();
            float weightChange = weight - (count - movesLeft) * decreasePerMove;
            weightTables[tableName][currentPosition.x, currentPosition.y] += weightChange;

            if (movesLeft > 0)
            {
                List<Vector2Int> neighbors = GetValidNeighbors(currentPosition);
                foreach (var neighbor in neighbors)
                {
                    if (visited[neighbor.x, neighbor.y] < movesLeft - 1)
                    {
                        visited[neighbor.x, neighbor.y] = movesLeft - 1;
                        queue.Enqueue((neighbor, movesLeft - 1));
                    }
                }
            }
        }
    }

    private List<Vector2Int> FindEndPointsBFS(Vector2Int start, int initialMoves)
    {
        Queue<(Vector2Int position, int movesLeft)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> endPoints = new List<Vector2Int>();

        queue.Enqueue((start, initialMoves));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (currentPosition, movesLeft) = queue.Dequeue();

            if (movesLeft <= 0)
            {
                endPoints.Add(currentPosition);
                continue;
            }

            List<Vector2Int> neighbors = GetValidNeighbors(currentPosition);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    int cost = GetMovementCost(mapBlocksList[neighbor.x, neighbor.y]);
                    int newMovesLeft = movesLeft - cost;
                    if (newMovesLeft >= 0)
                    {
                        queue.Enqueue((neighbor, newMovesLeft));
                        visited.Add(neighbor);
                    }
                    else
                    {
                        endPoints.Add(neighbor);
                    }
                }
            }
        }

        return endPoints;
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x, position.y + 1),
            new Vector2Int(position.x, position.y - 1)
        };

        neighbors.RemoveAll(neighbor => !IsPassable(neighbor));
        return neighbors;
    }

    private int GetMovementCost(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Floor:
                return 1;
            case BlockType.Door:
                return 10;
            default:
                return 1; // Default movement cost
        }
    }


    private bool IsPassable(Vector2Int position)
    {
        return IsWithinBounds(position.x, position.y)
            && mapBlocksList[position.x, position.y] != BlockType.Wall;
    }

    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < mapBlocksList.GetLength(0) && y < mapBlocksList.GetLength(1);
    }

    public Vector2Int FindBestEndPoint(List<Vector2Int> endPoints)
    {
        List<Vector2Int> bestPoints = new List<Vector2Int>();
        float highestWeight = float.MinValue;

        foreach (var point in endPoints)
        {
            float weight = finalWeightTable[point.x, point.y];
            if (weight > highestWeight)
            {
                highestWeight = weight;
                bestPoints.Clear();
                bestPoints.Add(point);
            }
            else if (weight == highestWeight)
            {
                bestPoints.Add(point);
            }
        }

        if (bestPoints.Count == 0)
        {
            return endPoints[0]; // 기본값
        }

        System.Random rand = new System.Random();
        return bestPoints[rand.Next(bestPoints.Count)];
    }
}