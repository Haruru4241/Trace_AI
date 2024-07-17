using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class ProceduralMap : MonoBehaviour
{
    public int mapSize = 50;
    public int maxDepth = 5;
    public float minDivideSize = 0.3f;
    public float maxDivideSize = 0.7f;
    public int minRoomSize = 5;
    public float removeRoomProbability = 0.2f; // 방을 제거할 확률
    public float secondClosestProbability = 0.4f;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject hallwayPrefab; // Change corridorPrefab to hallwayPrefab
    public GameObject doorPrefab;
    public GameObject ai;
    public GameObject player;
    NavMeshSurface navMeshSurface;
    public enum BlockType
    {
        Empty,
        Floor,
        Wall,
        Hallway, // Change Corridor to Hallway
        Door
    }

    private TreeNode rootNode;
    private List<Room> rooms = new List<Room>();
    private List<BlockType>[,] mapBlocks; // 각 좌표에 여러 블록을 저장하도록 수정
    private List<BlockType[,]> mapBlocksList = new List<BlockType[,]>();
    private List<Vector2Int> floorPositions = new List<Vector2Int>();

    private void Start()
    {
        mapBlocks = InitializeMap(mapSize);
        rootNode = new TreeNode(0, 0, mapSize, mapSize);
        DivideTree(rootNode, 0);
        GenerateRooms(rootNode, 0);
        RemoveRandomRooms();
        FillMapBlocksList();
        ConnectRooms();
        //CreateFinalMap();
        DrawMap();
        navMeshSurface=GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
        PlaceEntities();
    }

    private void PlaceEntities()
    {
        Vector3 randomNavMeshPositionPlayer = GetRandomNavMeshPosition();
        Vector3 randomNavMeshPositionAI = GetRandomNavMeshPosition();

        // 플레이어와 AI 위치 이동
        player.transform.position = randomNavMeshPositionPlayer;
        ai.transform.position = randomNavMeshPositionAI;
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        // 맵 크기 내에서 랜덤 위치 선택
        Vector3 randomPosition = new Vector3(
            Random.Range(0, mapSize),
            0,
            Random.Range(0, mapSize)
        );

        NavMeshHit hit;
        // NavMesh 상의 가장 가까운 점 샘플링
        if (NavMesh.SamplePosition(randomPosition, out hit, mapSize, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // 샘플링에 실패한 경우 다시 시도
        return GetRandomNavMeshPosition();
    }


    private List<BlockType>[,] InitializeMap(int size)
    {
        List<BlockType>[,] map = new List<BlockType>[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                map[x, z] = new List<BlockType> { BlockType.Empty };
            }
        }
        return map;
    }

    private void DivideTree(TreeNode treeNode, int depth)
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

    private void GenerateRooms(TreeNode treeNode, int depth)
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

    private void RemoveRandomRooms()
    {
        List<Room> roomsToRemove = new List<Room>();
        foreach (Room room in rooms)
        {
            if (Random.value < removeRoomProbability)
            {
                roomsToRemove.Add(room);
            }
        }

        foreach (Room room in roomsToRemove)
        {
            rooms.Remove(room);
        }
    }

    private void ConnectRooms()
    {
        HashSet<(Room, Room)> connectedRooms = new HashSet<(Room, Room)>();

        // 순차적으로 방을 묶어서 연결 정보 저장
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Room currentRoom = rooms[i];
            Room nextRoom = rooms[i + 1];
            if (!connectedRooms.Contains((currentRoom, nextRoom)) 
                && !connectedRooms.Contains((nextRoom, currentRoom)))
            {
                connectedRooms.Add((currentRoom, nextRoom));
            }
        }

        // 확률적으로 가까운 방을 추가로 연결
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            Room closestRoom = null;
            float closestDistance = float.MaxValue;

            for (int j = 0; j < rooms.Count; j++)
            {
                if (i == j) continue;

                Room otherRoom = rooms[j];
                if (connectedRooms.Contains((currentRoom, otherRoom))) continue;

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
                if (!connectedRooms.Contains((currentRoom, closestRoom)) && !connectedRooms.Contains((closestRoom, currentRoom)))
                {
                    connectedRooms.Add((currentRoom, closestRoom));
                }
            }
        }

        // 저장된 연결 정보를 사용하여 복도 생성
        foreach (var (start, end) in connectedRooms)
        {
            CreateHallway(start, end);
        }
    }

    private void CreateHallway(Room start, Room end)
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
        else
        {
            mapBlocks[bestStartPos.x, bestStartPos.y].Remove(BlockType.Door);
            mapBlocks[bestEndPos.x, bestEndPos.y].Remove(BlockType.Door);
        }
    }

    private List<Vector2Int> GetDoorCandidates(Room room)
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
    private List<Vector2Int> FindPathBFS(Vector2Int start, Vector2Int end)
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
                if (!visited.Contains(neighbor) && IsWithinBounds(neighbor.x, neighbor.y))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;


                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }



    private List<Vector2Int> GetValidNeighbors(Vector2Int position)
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

    private bool IsTouchingWall(Vector2Int position)
    {
        if (IsWithinBounds(position.x, position.y) 
            && mapBlocks[position.x, position.y].Contains(BlockType.Wall)
            && !mapBlocks[position.x, position.y].Contains(BlockType.Door)
            )
        {
            return true;
        }
        return false;
    }

    private bool IsWithinBounds(int x, int z)
    {
        return x >= 0 && x < mapSize && z >= 0 && z < mapSize;
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.red, 100f);
    }

    private void CreateFinalMap()
    {
        foreach (var map in mapBlocksList)
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    if (map[x, z] != BlockType.Empty)
                    {
                        mapBlocks[x, z].Add(map[x, z]);
                    }
                }
            }
        }
    }
    private void DrawMap()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                // 우선순위에 따라 블록 선택
                BlockType blockToDraw = GetHighestPriorityBlock(mapBlocks[x, z]);
                if (blockToDraw == BlockType.Floor)
                {
                    Instantiate(floorPrefab, new Vector3(x, 0, z), Quaternion.identity);
                }
                else if (blockToDraw == BlockType.Wall)
                {
                    Instantiate(wallPrefab, new Vector3(x, 0, z), Quaternion.identity);
                }
                else if (blockToDraw == BlockType.Hallway)
                {
                    Instantiate(hallwayPrefab, new Vector3(x, 0, z), Quaternion.identity);
                }
                else if (blockToDraw == BlockType.Door)
                {
                    Instantiate(doorPrefab, new Vector3(x, 0, z), Quaternion.identity);
                }
            }
        }
    }
    private BlockType GetHighestPriorityBlock(List<BlockType> blocks)
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
    private void FillMapBlocksList()
    {
        foreach (Room room in rooms)
        {
            RectInt rect = room.rect;
            for (int x = rect.x; x <= rect.x + rect.width; x++)
            {
                for (int z = rect.y; z <= rect.y + rect.height; z++)
                {
                    if (IsWithinBounds(x, z))
                    {
                        if (x == rect.x || x == rect.x + rect.width || z == rect.y || z == rect.y + rect.height)
                        {
                            mapBlocks[x, z].Add(BlockType.Wall);
                        }
                        else
                        {
                            mapBlocks[x, z].Add(BlockType.Floor);
                            floorPositions.Add(new Vector2Int(x, z)); // 바닥 위치 저장
                        }
                    }
                }
            }
        }
    }

    private int GetBlockPriority(BlockType blockType)
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
