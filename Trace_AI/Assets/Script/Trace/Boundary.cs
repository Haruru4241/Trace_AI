using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boundary : MoveBase
{
    private Vector3 targetPosition;
    Vector2Int currentPosition;

    public List<WeightTable> weightTableTypes = new List<WeightTable>();

    public static BlockType[,] mapBlocksList;

    Dictionary<WeightTableType, WeightTable> weightTables;
    WeightTable finalWeightTable;

    public override void Initialize()
    {
        base.Initialize();

        GameObject gameManagerObject = GameObject.Find("GameManager");
        GameManager gameManager = gameManagerObject.GetComponent<GameManager>();
        mapBlocksList = gameManager.mapMaker.mapBlocksList;

        int width = mapBlocksList.GetLength(0);
        int height = mapBlocksList.GetLength(1);

        weightTables = new Dictionary<WeightTableType, WeightTable>
        {
            { WeightTableType.LastPosition, new WeightTable(new float[width, height]) },
            { WeightTableType.Visited, new WeightTable(new float[width, height]) },
            { WeightTableType.Environmental, new WeightTable(new float[width, height]) },
            { WeightTableType.EndPosition, new WeightTable(new float[width, height]) }
        };

        finalWeightTable = new WeightTable(new float[width, height]);
    }

    public override void Enter()
    {
        Debug.Log($"{transform.name} ��� ���� ����, ��ǥ: {targetPosition}");
        Vector2Int lastPosition = new Vector2Int((int)ai.targetList.First().Key.position.x, (int)ai.targetList.First().Key.position.z);
        UpdateWeightMap(WeightTableType.LastPosition, lastPosition, 3, 10, 0);

        Debug.Log(WeightTableType.LastPosition);
        weightTables[WeightTableType.LastPosition].PrintValues();
        ArriveTargetPosition();
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();


        Vector2Int newPosition = CurrentPosition();
        if (newPosition != currentPosition)
        {
            UpdateWeightMap(WeightTableType.Visited, newPosition, 4, -0.3f, 0);
            currentPosition = newPosition;
        }
        foreach (var rule in fsm.FindStatetargetState(this))
        {
            if (rule.ExitCondition.ExitCondition())
            {
                Exit(rule.escapeState);
                break; // 조건이 만족되면 반복 종료
            }
        }

        // if (ai.targetList.Any() && (ai.targetList.First().Value >= fsm.chaseThreshold
        //     || ai.targetList.First().Value <= fsm.patrolThreshold))
        // {
        //     Exit();
        // }
    }

    public override void Exit(MoveBase newState)
    {
        Debug.Log($"{transform.name} ��� ���� Ż��");
        fsm.SetState(newState);
        //fsm.SetState<Patrol>();
    }

    public override void ArriveTargetPosition()
    {
        List<Vector2Int> endPoints = FindEndPointsBFS(CurrentPosition(), 4);
        foreach (var endPoint in endPoints) UpdateWeightMap(WeightTableType.EndPosition, endPoint, 1, 2, 0);

        Debug.Log(WeightTableType.Visited);
        weightTables[WeightTableType.Visited].PrintValues();
        Debug.Log(WeightTableType.EndPosition);
        weightTables[WeightTableType.EndPosition].PrintValues();

        Vector2Int bestEndPoint = FindBestEndPoint();
        targetPosition = new Vector3(bestEndPoint.x, 1, bestEndPoint.y);

        weightTables[WeightTableType.LastPosition].InitializeValues();
        weightTables[WeightTableType.Visited].InitializeValues();
        weightTables[WeightTableType.EndPosition].InitializeValues();

        Debug.Log($"{transform.name} ��� ��ǥ �缳��: {targetPosition}");
    }

    public override Vector3 TraceTargetPosition()
    {
        return targetPosition;
    }

    private void UpdateWeightMap(WeightTableType tableName, Vector2Int newPosition, int count, float weight, float decreasePerMove)
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
                    if (newMovesLeft > 0)
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

    private Vector2Int FindBestEndPoint()
    {
        float highestWeight = float.MinValue;
        List<Vector2Int> bestEndPoints = new List<Vector2Int>();

        int rows = mapBlocksList.GetLength(0);
        int cols = mapBlocksList.GetLength(1);

        finalWeightTable.InitializeValues();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                foreach (var table in weightTables.Values)
                {
                    finalWeightTable[x, y] += table[x, y];
                }

                float weight = finalWeightTable[x, y];
                if (weight > highestWeight)
                {
                    highestWeight = weight;
                    bestEndPoints.Clear(); // �ְ� ����ġ ���� �� ����Ʈ �ʱ�ȭ
                    bestEndPoints.Add(new Vector2Int(x, y));
                }
                else if (weight == highestWeight) bestEndPoints.Add(new Vector2Int(x, y));
            }
        }

        Debug.Log("finalWeightTable");
        finalWeightTable.PrintValues();

        if (bestEndPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, bestEndPoints.Count);

            Debug.Log($"bestEndPoints {bestEndPoints.Count} {randomIndex}");
            return bestEndPoints[randomIndex];
        }
        return Vector2Int.zero;
    }

    private int GetMovementCost(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Floor:
                return 1;
            case BlockType.Door:
                return 10;
            case BlockType.Hallway:
                return 1;
            default:
                return 1; // Default movement cost
        }
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

    private bool IsPassable(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;

        return x >= 0 && y >= 0 && x < mapBlocksList.GetLength(0) && y < mapBlocksList.GetLength(1)
            && mapBlocksList[x, y] != BlockType.Wall;
    }

    private Vector2Int CurrentPosition()
    {
        return new Vector2Int((int)transform.position.x, (int)transform.position.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetPosition, 1f);
    }
}
public enum WeightTableType
{
    LastPosition,
    Visited,
    Environmental,
    EndPosition
}

[System.Serializable]
public class WeightTable
{
    public float[,] Values { get; private set; }
    public string tableName = "Default";
    public int count = 1;
    public float weight = 0;
    public float decreasePerMove = 0;

    public WeightTable(float[,] values)
    {
        Values = values;
    }

    public WeightTable(int width, int height)
    {
        Values = new float[width, height];
        InitializeValues();
    }

    public void InitializeValues()
    {
        for (int i = 0; i < Values.GetLength(0); i++)
        {
            for (int j = 0; j < Values.GetLength(1); j++)
            {
                Values[i, j] = 0f;
            }
        }
    }

    public float this[int x, int y]
    {
        get { return Values[x, y]; }
        set { Values[x, y] = value; }
    }

    public void PrintValues()
    {
        string name = this.ToString();
        string allValues = "";
        for (int i = Values.GetLength(1) - 1; i >= 0; i--) // �Ʒ����� ����
        {
            for (int j = 0; j < Values.GetLength(1); j++) // ���ʿ��� ����������
            {
                string valueStr = Values[j, i].ToString();
                if (Boundary.mapBlocksList[j, i] == BlockType.Empty)
                {
                    valueStr = "=";
                }
                else
                {
                    valueStr = Values[j, i].ToString();
                }
                int padding = 3 - valueStr.Length; // �� 3ĭ�� ���߱� ���� �е� ���
                allValues += valueStr + new string(' ', padding > 0 ? padding : 0); // �ʿ��� ���� �߰�
            }
            allValues += "\n"; // �� ������ �� �ٲ� �߰�
        }
        Debug.Log(allValues.TrimEnd()); // ������ �� �ٲ� ����
    }
}