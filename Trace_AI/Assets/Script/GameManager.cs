using Map.Sample;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static ProceduralMap;

public class GameManager : MonoBehaviour
{
    public List<GameObject> Entities;
    [Tooltip("��Ÿ�� ����Ʈ ����, ���� �� ������ ��ġ�� ����")]
    public List<Transform> StartingPositions;

    public NavMeshSurface[] navMeshSurfaces;

    private BlockType[,] mapBlocksList;
    // Start is called before the first frame update
    void Awake()
    {
        MapGenerator mapMaker = GetComponent<MapGenerator>();
        if (mapMaker != null)
        {
            mapMaker.Initialize();
            mapBlocksList = mapMaker.mapBlocksList;
        }
        foreach (var surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        PlaceEntities();
    }

    private void PlaceEntities()
    {
        for (int i = 0; i < Entities.Count; i++)
        {
            Vector3 position;
            if (i < StartingPositions.Count && StartingPositions[i] != null)
            {
                // �̸� ������ ��ġ�� �ִ� ��� �ش� ��ġ ���
                position = StartingPositions[i].position;
            }

            else
            {
                // �̸� ������ ��ġ�� ���� ��� ���� ��ġ ���
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
