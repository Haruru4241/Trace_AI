using UnityEngine;
using System.Linq;

public class DynamicMapGenerator : MapGenerator
{
    public override void Initialize()
    {
        // You can initialize your map here
        Debug.Log("Initializing Dynamic Map Generator");
        GenerateDynamicMap();
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
        mapSize = mapWidth; // Assuming square mapSize for simplicity
        Debug.Log($"Calculated map size: {mapWidth}x{mapHeight}");

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

        // Optionally: Draw or visualize the generated map here
        DrawMap();
    }
}
