using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class ProceduralMap : MapGenerator
{

    public override void Initialize()
    {
        Debug.Log("Map generated");
        mapSize=GameManager.Instance.prefebManager.GetMapSizeValue();
        maxDepth=GameManager.Instance.prefebManager.GetMaxDepthValue();
        mapBlocksList = new BlockType[mapSize, mapSize];
        mapBlocks = InitializeMap(mapSize);
        rootNode = new TreeNode(0, 0, mapSize, mapSize);
        DivideTree(rootNode, 0);
        GenerateRooms(rootNode, 0);
        FillMapBlocksList();
        ConnectRooms();

        // ����� ���� ������ ����Ͽ� ���� ����
        foreach (var (start, end) in connectedRooms)
        {
            CreateHallway(start, end);
        }

        DrawMap();
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
}
