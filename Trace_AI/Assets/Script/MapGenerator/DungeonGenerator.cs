using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int rows = 39;
    public int cols = 39;
    public int roomMinSize = 3;
    public int roomMaxSize = 9;
    public string roomLayout = "Scattered"; // Options: "Packed", "Scattered"
    public string corridorLayout = "Bent"; // Options: "Labyrinth", "Bent", "Straight"
    public int removeDeadends = 50; // percentage
    public int addStairs = 2; // number of stairs

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject stairUpPrefab;
    public GameObject stairDownPrefab;

    private int[,] dungeon;
    private System.Random rand;

    private const int NOTHING = 0;
    private const int BLOCKED = 1;
    private const int ROOM = 2;
    private const int CORRIDOR = 4;
    private const int PERIMETER = 16;
    private const int ENTRANCE = 32;
    private const int ROOM_ID = 64;

    private const int ARCH = 65536;
    private const int DOOR = 131072;
    private const int LOCKED = 262144;
    private const int TRAPPED = 524288;
    private const int SECRET = 1048576;
    private const int PORTC = 2097152;
    private const int STAIR_DN = 4194304;
    private const int STAIR_UP = 8388608;

    private const int OPENSPACE = ROOM | CORRIDOR;
    private const int DOORSPACE = ARCH | DOOR | LOCKED | TRAPPED | SECRET | PORTC;
    private const int ESPACE = ENTRANCE | DOORSPACE | 16777216;
    private const int STAIRS = STAIR_DN | STAIR_UP;

    private const int BLOCK_ROOM = BLOCKED | ROOM;
    private const int BLOCK_CORR = BLOCKED | PERIMETER | CORRIDOR;
    private const int BLOCK_DOOR = BLOCKED | DOORSPACE;

    void Start()
    {
        rand = new System.Random();
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        dungeon = new int[rows, cols];
        InitializeDungeon();
        PlaceRooms();
        CreateCorridors();
        PlaceStairs();
        RemoveDeadends();
        CreateDungeonObjects();
    }

    void InitializeDungeon()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                dungeon[r, c] = NOTHING;
            }
        }
    }

    void PlaceRooms()
    {
        if (roomLayout == "Packed")
        {
            PackRooms();
        }
        else
        {
            ScatterRooms();
        }
    }

    void PackRooms()
    {
        for (int i = 0; i < rows / 2; i++)
        {
            for (int j = 0; j < cols / 2; j++)
            {
                PlaceRoom(i, j);
            }
        }
    }

    void ScatterRooms()
    {
        int numRooms = (rows * cols) / (roomMaxSize * roomMaxSize);
        for (int i = 0; i < numRooms; i++)
        {
            PlaceRoom();
        }
    }

    void PlaceRoom(int i = -1, int j = -1)
    {
        int roomWidth = rand.Next(roomMinSize, roomMaxSize + 1);
        int roomHeight = rand.Next(roomMinSize, roomMaxSize + 1);
        int roomX = (i == -1) ? rand.Next(1, rows - roomWidth - 1) : i * 2 + 1;
        int roomY = (j == -1) ? rand.Next(1, cols - roomHeight - 1) : j * 2 + 1;

        for (int r = roomX; r < roomX + roomWidth; r++)
        {
            for (int c = roomY; c < roomY + roomHeight; c++)
            {
                dungeon[r, c] = ROOM;
            }
        }
    }

    void CreateCorridors()
    {
        for (int r = 1; r < rows - 1; r += 2)
        {
            for (int c = 1; c < cols - 1; c += 2)
            {
                if (dungeon[r, c] == NOTHING)
                {
                    CreateTunnel(r, c);
                }
            }
        }
    }

    void CreateTunnel(int r, int c, string direction = null)
    {
        if (r <= 0 || c <= 0 || r >= rows - 1 || c >= cols - 1)
            return;

        List<string> directions = new List<string> { "north", "south", "west", "east" };
        if (direction != null && rand.Next(100) < GetCorridorBendProbability())
        {
            directions.Insert(0, direction);
        }

        foreach (string dir in directions)
        {
            int dr = 0, dc = 0;
            if (dir == "north") dr = -1;
            if (dir == "south") dr = 1;
            if (dir == "west") dc = -1;
            if (dir == "east") dc = 1;

            int nr = r + dr * 2;
            int nc = c + dc * 2;

            if (nr > 0 && nr < rows && nc > 0 && nc < cols && dungeon[nr, nc] == NOTHING)
            {
                dungeon[r + dr, c + dc] = CORRIDOR;
                dungeon[nr, nc] = CORRIDOR;
                CreateTunnel(nr, nc, dir);
            }
        }
    }

    int GetCorridorBendProbability()
    {
        if (corridorLayout == "Straight") return 100;
        if (corridorLayout == "Bent") return 50;
        return 0; // Labyrinth
    }

    void PlaceStairs()
    {
        for (int i = 0; i < addStairs; i++)
        {
            PlaceStair(STAIR_UP);
            PlaceStair(STAIR_DN);
        }
    }

    void PlaceStair(int stairType)
    {
        int r, c;
        do
        {
            r = rand.Next(1, rows - 1);
            c = rand.Next(1, cols - 1);
        } while (dungeon[r, c] != ROOM);

        dungeon[r, c] = stairType;
    }

    void RemoveDeadends()
    {
        for (int r = 1; r < rows - 1; r++)
        {
            for (int c = 1; c < cols - 1; c++)
            {
                if (dungeon[r, c] == CORRIDOR && rand.Next(100) < removeDeadends)
                {
                    dungeon[r, c] = NOTHING;
                }
            }
        }
    }

    void CreateDungeonObjects()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 position = new Vector3(r, 0, c);

                if (dungeon[r, c] == ROOM)
                {
                    Instantiate(floorPrefab, position, Quaternion.identity);
                }
                else if (dungeon[r, c] == CORRIDOR)
                {
                    Instantiate(floorPrefab, position, Quaternion.identity);
                }
                else if (dungeon[r, c] == STAIR_UP)
                {
                    Instantiate(stairUpPrefab, position, Quaternion.identity);
                }
                else if (dungeon[r, c] == STAIR_DN)
                {
                    Instantiate(stairDownPrefab, position, Quaternion.identity);
                }
                else
                {
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
