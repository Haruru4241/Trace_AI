using UnityEngine;
using System.Collections.Generic;

public abstract class MapGenerator : MonoBehaviour
{
    public int mapSize = 50;

    public BlockType[,] mapBlocksList;
    protected List<BlockType>[,] mapBlocks;

    public int maxDepth = 5;
    public float minDivideSize = 0.3f;
    public float maxDivideSize = 0.7f;
    public int minRoomSize = 5;
    public float secondClosestProbability = 0f;

    public int numberOfDummy = 5; // 몇 개의 블록을 생성할지 결정
    public int numberOfSlowZone = 5; // 몇 개의 블록을 생성할지 결정
    public int numberOfCoins= 5; // 생성할 코인 수

    protected TreeNode rootNode;

    protected List<Room> rooms = new List<Room>();

    protected HashSet<(Room, Room)> connectedRooms = new HashSet<(Room, Room)>();

    private List<GameObject> generatedBlocks = new List<GameObject>();

    public abstract void Initialize();

    public List<Vector2Int> FindBlocksOfType(BlockType type)
    {
        List<Vector2Int> blocks = new List<Vector2Int>();

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                // 지정된 타입의 블록만 모음
                if (mapBlocksList[x, z] == type)
                {
                    blocks.Add(new Vector2Int(x, z));
                }
            }
        }

        return blocks;
    }
    public void SpawnRandomBlocks(){
        SpawnRandomBlocks(BlockType.Floor, GameManager.Instance.prefebManager.dummyPrefeb, GameManager.Instance.prefebManager.ObjCountControllers[0].GetValue());
        SpawnRandomBlocks(BlockType.Floor, GameManager.Instance.prefebManager.slowZonePrefeb, GameManager.Instance.prefebManager.ObjCountControllers[1].GetValue());
        SpawnRandomBlocks(BlockType.Floor, GameManager.Instance.prefebManager.coinPrefab, GameManager.Instance.prefebManager.ObjCountControllers[2].GetValue());
    }

    public void SpawnRandomBlocks(BlockType type, GameObject blockToSpawn, int numberOfBlocksToSpawn)
    {
        List<Vector2Int> availablePositions = FindBlocksOfType(type);

        if (availablePositions.Count == 0)
        {
            Debug.LogWarning("해당 타입의 블록을 찾을 수 없습니다.");
            return;
        }

        // 요청된 블록 수보다 선택 가능한 좌표가 적으면 그만큼만 생성
        int blocksToSpawn = Mathf.Min(numberOfBlocksToSpawn, availablePositions.Count);

        for (int i = 0; i < blocksToSpawn; i++)
        {
            // 랜덤 위치 선택
            Vector2Int randomPosition = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(randomPosition); // 중복 생성을 막기 위해 선택한 위치는 제거

            // 블록 생성
            GameObject spawnedBlock = Instantiate(blockToSpawn, new Vector3(randomPosition.x, 0, randomPosition.y), Quaternion.identity);
            spawnedBlock.transform.parent = GameManager.Instance.entityParent.transform;
            GameManager.Instance.generatedEntities.Add(spawnedBlock);
        }
    }



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
        // ���������� ���� ��� ���� ���� ����
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            connectedRooms.Add((rooms[i], rooms[i + 1]));
        }

        // Ȯ�������� ����� ���� �߰��� ����
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

            // n% Ȯ���� ���� ����� ���� ����
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

        // �� �ĺ��� �� �Ÿ� ���
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
        // �ִ� �Ÿ��� �� �ĺ����� �������� ���� ����
        List<Vector2Int> path = FindPathBFS(bestStartPos, bestEndPos);
        if (path != null)
        {
            // ���� Ÿ�� �߰�
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

        // �� ������ ������ ��ġ�� ����
        int randomX = Random.Range(rect.x + 1, rect.x + rect.width - 1);
        int randomY = Random.Range(rect.y + 1, rect.y + rect.height - 1);
        Vector2Int randomPosition = new Vector2Int(randomX, randomY);

        // ������ ��ġ���� �����¿� �����ڸ��� �����ϴ� �� �ĺ��� �߰�
        candidates.Add(new Vector2Int(rect.x, randomPosition.y)); // ���� �����ڸ�
        candidates.Add(new Vector2Int(rect.x + rect.width, randomPosition.y)); // ������ �����ڸ�
        candidates.Add(new Vector2Int(randomPosition.x, rect.y)); // �Ʒ��� �����ڸ�
        candidates.Add(new Vector2Int(randomPosition.x, rect.y + rect.height)); // ���� �����ڸ�

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

        return null; // ��θ� ã�� ���� ���
    }

    protected List<Vector2Int> GetValidNeighbors(Vector2Int position)
    {
        // 10. ���� �� ���� �������θ� �̿� ��� ����
        List<Vector2Int> neighbors = new List<Vector2Int>
    {
        new Vector2Int(position.x + 1, position.y),
        new Vector2Int(position.x - 1, position.y),
        new Vector2Int(position.x, position.y + 1),
        new Vector2Int(position.x, position.y - 1)
    };

        // 11. ���� �������� �ʴ� �̿� ��常 ��ȯ
        neighbors.RemoveAll(neighbor => IsTouchingWall(neighbor));
        return neighbors;
    }

    protected bool IsTouchingWall(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;

        if (IsWithinBounds(x, y))
        {
            if (mapBlocks[x, y].Contains(BlockType.Wall)
                    && !mapBlocks[x, y].Contains(BlockType.Door))
            {
                return true;
            }
            return false;
        }
        return true;
    }

    protected void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.red, 10f);
    }
    protected void DrawMap()
    {
        GameObject prefabToInstantiate;
        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                BlockType blockToDraw = GetHighestPriorityBlock(mapBlocks[x, z]);
                mapBlocksList[x, z] = blockToDraw;

                Quaternion blockRotation = Quaternion.identity;

                switch (blockToDraw)
                {
                    case BlockType.Floor:
                        prefabToInstantiate = GameManager.Instance.prefebManager.floorPrefab;
                        break;
                    case BlockType.Wall:
                        prefabToInstantiate = GameManager.Instance.prefebManager.wallPrefab;
                        break;
                    case BlockType.Hallway:
                        prefabToInstantiate = GameManager.Instance.prefebManager.hallwayPrefab;
                        break;
                    case BlockType.Door:
                        prefabToInstantiate = GameManager.Instance.prefebManager.doorPrefab;
                        // 문에 대한 방향 처리
                        if (IsWithinBounds(x, z - 1) && IsWithinBounds(x, z + 1))
                        {
                            if (mapBlocks[x, z - 1].Contains(BlockType.Wall) && mapBlocks[x, z + 1].Contains(BlockType.Wall))
                            {
                                blockRotation = Quaternion.Euler(0, 90, 0); // 가로 방향 문
                            }
                        }
                        break;
                    default:
                        prefabToInstantiate = null; // 기본적으로 null 반환 (필요 시 다른 기본값 설정 가능)
                        break;
                }
                if (prefabToInstantiate != null)
                {

                    // 블록 생성
                    GameObject instantiatedBlock = Instantiate(prefabToInstantiate, new Vector3(x, 0, z), blockRotation);
                    instantiatedBlock.transform.parent = GameManager.Instance.mapParent.transform;
                    generatedBlocks.Add(instantiatedBlock);
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
    public void ClearMap()
    {
        // 맵 블록 초기화
        mapBlocksList = new BlockType[mapSize, mapSize];
        mapBlocks = new List<BlockType>[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                mapBlocks[x, z] = new List<BlockType>();
            }
        }
        rooms.Clear(); // 방 목록 초기화
        connectedRooms.Clear(); // 연결된 방 목록 초기화
                                // 리스트에 있는 모든 GameObject를 파괴
        foreach (GameObject block in generatedBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }

        // 리스트를 비움
        generatedBlocks.Clear();
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