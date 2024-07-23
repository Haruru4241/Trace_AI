using UnityEngine;
using System.Collections.Generic;

public abstract class MapGenerator : MonoBehaviour
{
    public int mapSize = 50;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject hallwayPrefab;
    public GameObject doorPrefab;

    public BlockType[,] mapBlocksList;
    protected List<BlockType>[,] mapBlocks;

    public int maxDepth = 5;
    public float minDivideSize = 0.3f;
    public float maxDivideSize = 0.7f;
    public int minRoomSize = 5;
    public float secondClosestProbability = 0f;

    protected TreeNode rootNode;

    protected List<Room> rooms = new List<Room>();

    protected HashSet<(Room, Room)> connectedRooms = new HashSet<(Room, Room)>();

    public abstract void Initialize();

    protected BlockType GetHighestPriorityBlock(List<BlockType> blocks)
    {
        BlockType highestPriorityBlock = BlockType.Empty;
        int highestPriority = 0;
        foreach (var block in blocks)
        {
            int priority = GetBlockPriority(block);
            if (priority > highestPriority)
            {
                highestPriority = priority;
                highestPriorityBlock = block;
            }
        }
        return highestPriorityBlock;
    }

    protected int GetBlockPriority(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Door: return 4;
            case BlockType.Hallway: return 3;
            case BlockType.Wall: return 2;
            case BlockType.Floor: return 1;
            default: return 0;
        }
    }

    protected void DivideTree(TreeNode treeNode, int depth)
    {
        if (depth >= maxDepth) return;

        RectInt size = treeNode.treeSize;
        bool splitHorizontally = size.width >= size.height;
        int length = splitHorizontally ? size.width : size.height;
        int split = Mathf.RoundToInt(Random.Range(length * minDivideSize, length * maxDivideSize));

        if (splitHorizontally)
        {
            treeNode.leftTree = new TreeNode(size.x, size.y, split, size.height);
            treeNode.rightTree = new TreeNode(size.x + split, size.y, size.width - split, size.height);
            DrawLine(new Vector3(size.x + split, 0, size.y), new Vector3(size.x + split, 0, size.y + size.height));
        }
        else
        {
            treeNode.leftTree = new TreeNode(size.x, size.y, size.width, split);
            treeNode.rightTree = new TreeNode(size.x, size.y + split, size.width, size.height - split);
            DrawLine(new Vector3(size.x, 0, size.y + split), new Vector3(size.x + size.width, 0, size.y + split));
        }

        DivideTree(treeNode.leftTree, depth + 1);
        DivideTree(treeNode.rightTree, depth + 1);
    }

    protected void GenerateRooms(TreeNode treeNode, int depth)
    {
        if (depth == maxDepth)
        {
            RectInt size = treeNode.treeSize;
            int width = Random.Range(size.width / 2, size.width - 1);
            int height = Random.Range(size.height / 2, size.height - 1);
            int x = size.x + Random.Range(1, size.width - width - 1);
            int y = size.y + Random.Range(1, size.height - height - 1);
            treeNode.room = new Room(new RectInt(x, y, width, height));
            if (minRoomSize < width * height) rooms.Add(treeNode.room);

            return;
        }

        if (treeNode.leftTree != null) GenerateRooms(treeNode.leftTree, depth + 1);
        if (treeNode.rightTree != null) GenerateRooms(treeNode.rightTree, depth + 1);
    }

    protected void ConnectRooms()
    {
        // 순차적으로 방을 묶어서 연결 정보 저장
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            connectedRooms.Add((rooms[i], rooms[i + 1]));
        }

        // 확률적으로 가까운 방을 추가로 연결
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            Room closestRoom = null;
            float closestDistance = float.MaxValue;

            for (int j = 0; j < rooms.Count; j++)
            {
                Room otherRoom = rooms[j];
                if (i == j || connectedRooms.Contains((currentRoom, otherRoom))) continue;

                float distance = Vector2Int.Distance(currentRoom.Center, otherRoom.Center);
                if (distance < closestDistance)
                {
                    closestRoom = otherRoom;
                    closestDistance = distance;
                }
            }

            // n% 확률로 가장 가까운 방을 연결
            if (closestRoom != null && Random.value < secondClosestProbability)
            {
                connectedRooms.Add((currentRoom, closestRoom));
            }
        }
    }

    protected void CreateHallway(Room start, Room end)
    {
        List<Vector2Int> startCandidates = GetDoorCandidates(start);
        List<Vector2Int> endCandidates = GetDoorCandidates(end);

        float shortestDistance = float.MaxValue;
        Vector2Int bestStartPos = Vector2Int.zero;
        Vector2Int bestEndPos = Vector2Int.zero;

        // 문 후보지 간 거리 계산
        foreach (Vector2Int startPos in startCandidates)
        {
            foreach (Vector2Int endPos in endCandidates)
            {
                float distance = Vector2Int.Distance(startPos, endPos);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestStartPos = startPos;
                    bestEndPos = endPos;
                }
            }
        }
        mapBlocks[bestStartPos.x, bestStartPos.y].Add(BlockType.Door);
        mapBlocks[bestEndPos.x, bestEndPos.y].Add(BlockType.Door);
        // 최단 거리의 문 후보지를 기준으로 복도 생성
        List<Vector2Int> path = FindPathBFS(bestStartPos, bestEndPos);
        if (path != null)
        {
            // 복도 타일 추가
            foreach (Vector2Int pos in path)
            {
                mapBlocks[pos.x, pos.y].Add(BlockType.Hallway);
            }
        }
    }

    protected List<Vector2Int> GetDoorCandidates(Room room)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        RectInt rect = room.rect;

        // 방 안쪽의 랜덤한 위치를 잡음
        int randomX = Random.Range(rect.x + 1, rect.x + rect.width - 1);
        int randomY = Random.Range(rect.y + 1, rect.y + rect.height - 1);
        Vector2Int randomPosition = new Vector2Int(randomX, randomY);

        // 랜덤한 위치에서 상하좌우 가장자리에 도달하는 문 후보지 추가
        candidates.Add(new Vector2Int(rect.x, randomPosition.y)); // 왼쪽 가장자리
        candidates.Add(new Vector2Int(rect.x + rect.width, randomPosition.y)); // 오른쪽 가장자리
        candidates.Add(new Vector2Int(randomPosition.x, rect.y)); // 아래쪽 가장자리
        candidates.Add(new Vector2Int(randomPosition.x, rect.y + rect.height)); // 위쪽 가장자리

        return candidates;
    }
    protected List<Vector2Int> FindPathBFS(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                while (current != start)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            List<Vector2Int> neighbors = GetValidNeighbors(current);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }

    protected List<Vector2Int> GetValidNeighbors(Vector2Int position)
    {
        // 10. 가로 및 세로 방향으로만 이웃 노드 생성
        List<Vector2Int> neighbors = new List<Vector2Int>
    {
        new Vector2Int(position.x + 1, position.y),
        new Vector2Int(position.x - 1, position.y),
        new Vector2Int(position.x, position.y + 1),
        new Vector2Int(position.x, position.y - 1)
    };

        // 11. 벽과 접촉하지 않는 이웃 노드만 반환
        neighbors.RemoveAll(neighbor => IsTouchingWall(neighbor));
        return neighbors;
    }

    protected bool IsTouchingWall(Vector2Int position)
    {
        int x=position.x;
        int y=position.y;

        if (IsWithinBounds(x, y))
        {
            if (mapBlocks[x, y].Contains(BlockType.Wall)
                    && !mapBlocks[x, y].Contains(BlockType.Door))
            {
                return true;
            }
            return false;
        } return true;
    }

    protected void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.red, 100f);
    }

    protected void DrawMap()
    {
        GameObject mapParent = new GameObject("Map");
        GameObject prefabToInstantiate;
        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                BlockType blockToDraw = GetHighestPriorityBlock(mapBlocks[x, z]);
                mapBlocksList[x, z] = blockToDraw;
                switch (blockToDraw)
                {
                    case BlockType.Floor:
                        prefabToInstantiate = Instantiate(floorPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        prefabToInstantiate.transform.parent = mapParent.transform;
                        break;
                    case BlockType.Wall:
                        prefabToInstantiate = Instantiate(wallPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        prefabToInstantiate.transform.parent = mapParent.transform;
                        break;
                    case BlockType.Hallway:
                        prefabToInstantiate = Instantiate(hallwayPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        prefabToInstantiate.transform.parent = mapParent.transform;
                        break;
                    case BlockType.Door:
                        prefabToInstantiate = Instantiate(doorPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        prefabToInstantiate.transform.parent = mapParent.transform;
                        break;
                    default:
                        //prefabToInstantiate = Instantiate(floorPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        break;
                }

                
            }
        }
    }

    protected void FillMapBlocksList()
    {
        foreach (Room room in rooms)
        {
            RectInt rect = room.rect;
            for (int x = rect.x; x <= rect.x + rect.width; x++)
            {
                for (int z = rect.y; z <= rect.y + rect.height; z++)
                {
                    if (x == rect.x || x == rect.x + rect.width || z == rect.y || z == rect.y + rect.height)
                    {
                        mapBlocks[x, z].Add(BlockType.Wall);
                    }
                    else
                    {
                        mapBlocks[x, z].Add(BlockType.Floor);
                    }
                }
            }
        }
    }

    protected bool IsWithinBounds(int x, int z)
    {
        return x >= 0 && x < mapSize && z >= 0 && z < mapSize;
    }

    public class TreeNode
    {
        public RectInt treeSize;
        public TreeNode leftTree;
        public TreeNode rightTree;
        public Room room;

        public TreeNode(int x, int y, int width, int height)
        {
            treeSize = new RectInt(x, y, width, height);
        }
    }

    public class Room
    {
        public RectInt rect;
        public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

        public Room(RectInt rect)
        {
            this.rect = rect;
        }
    }
}
public enum BlockType
{
    Empty,
    Floor,
    Wall,
    Hallway, // Change Corridor to Hallway
    Door
}