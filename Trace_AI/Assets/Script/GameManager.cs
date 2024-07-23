using Map.Sample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static ProceduralMap;

public class GameManager : MonoBehaviour
{
    public List<GameObject> Entities;
    [Tooltip("스타팅 포인트 설정, 없을 시 랜덤한 위치를 지정")]
    public List<Transform> StartingPositions;
    public Vector2Int mapSize;

    NavMeshSurface[] navMeshSurfaces;

    private BlockType[,] mapBlocksList;
    // Start is called before the first frame update
    void Awake()
    {
        navMeshSurfaces = GetComponentsInChildren<NavMeshSurface>();

        MapGenerator mapMaker = GetComponent<MapGenerator>();
        if (mapMaker != null)
        {
            mapMaker.Initialize();
            mapBlocksList = mapMaker.mapBlocksList;
        }
        else GenerateDynamicMap();

        foreach (var surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        PlaceEntities();
    }
    public void GenerateDynamicMap()
    {
        // Find all game objects with block tags
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag("Floor")
            .Concat(GameObject.FindGameObjectsWithTag("Wall"))
            .Concat(GameObject.FindGameObjectsWithTag("Hallway"))
            .Concat(GameObject.FindGameObjectsWithTag("Door"))
            .ToArray();

        if (allBlocks.Length == 0)
        {
            Debug.LogError("No blocks found in the scene.");
            return;
        }

        // Determine the map size based on the found blocks
        Vector3 minBounds = allBlocks[0].transform.position;
        Vector3 maxBounds = allBlocks[0].transform.position;

        foreach (var block in allBlocks)
        {
            Vector3 position = block.transform.position;
            if (position.x < minBounds.x) minBounds.x = position.x;
            if (position.y < minBounds.y) minBounds.y = position.y;
            if (position.z < minBounds.z) minBounds.z = position.z;
            if (position.x > maxBounds.x) maxBounds.x = position.x;
            if (position.y > maxBounds.y) maxBounds.y = position.y;
            if (position.z > maxBounds.z) maxBounds.z = position.z;
        }

        int mapWidth = Mathf.CeilToInt(maxBounds.x - minBounds.x) + 1;
        int mapHeight = Mathf.CeilToInt(maxBounds.z - minBounds.z) + 1;
        mapSize = new Vector2Int(mapWidth, mapHeight);
        // Initialize the mapBlocksList with Empty blocks
        mapBlocksList = new BlockType[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapBlocksList[x, y] = BlockType.Empty; // Default to Empty
            }
        }

        // Assign block types based on the tags
        foreach (var block in allBlocks)
        {
            Vector3 position = block.transform.position;
            int x = Mathf.RoundToInt(position.x - minBounds.x);
            int y = Mathf.RoundToInt(position.z - minBounds.z);

            switch (block.tag)
            {
                case "Floor":
                    mapBlocksList[x, y] = BlockType.Floor;
                    break;
                case "Wall":
                    mapBlocksList[x, y] = BlockType.Wall;
                    break;
                case "Hallway":
                    mapBlocksList[x, y] = BlockType.Hallway;
                    break;
                case "Door":
                    mapBlocksList[x, y] = BlockType.Door;
                    break;
                default:
                    mapBlocksList[x, y] = BlockType.Empty;
                    break;
            }
        }
    }

    private void PlaceEntities()
    {
        for (int i = 0; i < Entities.Count; i++)
        {
            Vector3 position;
            if (i < StartingPositions.Count && StartingPositions[i] != null)
            {
                // 미리 지정된 위치가 있는 경우 해당 위치 사용
                position = StartingPositions[i].position;
            }

            else
            {
                // 미리 지정된 위치가 없는 경우 랜덤 위치 사용
                position = GetRandomNavMeshPosition();
            }

            GameObject entity = Instantiate(Entities[i], position, Quaternion.identity);
            entity.GetComponent<CharacterBase>().Initialize();
        }
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(0, mapBlocksList.GetLength(0)),
            0,
            Random.Range(0, mapBlocksList.GetLength(1))
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return GetRandomNavMeshPosition();
    }

    public void SetMapBlocksList(BlockType[,] blocks)
    {
        mapBlocksList = blocks;
    }

    public BlockType[,] getMapBlocksList()
    {
        return mapBlocksList;
    }
}
