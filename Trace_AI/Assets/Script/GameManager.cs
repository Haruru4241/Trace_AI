using Map.Sample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static ProceduralMap;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스를 저장할 정적 필드
    private static GameManager _instance;

    // 싱글톤 인스턴스에 접근할 수 있는 프로퍼티
    // 싱글톤 인스턴스에 접근할 수 있는 프로퍼티
    public static GameManager Instance
    {
        get
        {
            // 인스턴스가 없으면 찾거나 생성
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(GameManager).ToString());
                    _instance = singleton.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    public List<GameObject> Entities;
    [Tooltip("��Ÿ�� ����Ʈ ����, ���� �� ������ ��ġ�� ����")]
    public List<Transform> StartingPositions;
    public Vector2Int mapSize;

    NavMeshSurface[] navMeshSurfaces;

    public MapGenerator mapMaker;

    [Header("New")]
    public PrefebManager prefebManager;
    public GameObject mapParent;
    private List<GameObject> generatedEntities = new List<GameObject>();
    public Camera gameCamera;

    private bool isMapGenerated = false;
    private bool islocated = false;

    public void Awake()
    {
        mapMaker = GetComponent<MapGenerator>();
        if (mapMaker == null)
        {
            mapMaker = gameObject.AddComponent<DynamicMapGenerator>();
        }
        navMeshSurfaces = GetComponentsInChildren<NavMeshSurface>();
    }

    public void GenerateMap()
    {
        StartCoroutine(GenerateMapCoroutine());//비동기 문제가 있어 코루틴으로 gameClear가 완전히 끝날 때까지 기다림
    }

    private IEnumerator GenerateMapCoroutine()
    {
        gameClear();

        // gameClear가 완전히 끝날 때까지 기다림
        yield return new WaitForEndOfFrame();

        mapMaker.Initialize();

        foreach (var surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        isMapGenerated = true;
        Relocation();
    }

    public void gameClear()
    {
        foreach (var surface in navMeshSurfaces)
        {
            surface.RemoveData();
        }

        mapMaker.ClearMap();
        ClearEntity();
        isMapGenerated = false;
        islocated = false;
    }
    public void GameStart()
    {
        if (isMapGenerated && islocated)
        {
            foreach (var entity in generatedEntities)
            {
                entity.GetComponent<CharacterBase>().Initialize(); // 플레이어 초기화
            }
        }
    }
    public void Relocation()
    {
        if (isMapGenerated)
        {
            ClearEntity();
            PlaceEntities();
            islocated = true;
        }
    }

    public void ClearEntity()
    {
        // 현재 게임 오브젝트 또는 자식 오브젝트에 있는 모든 LineRenderer 컴포넌트를 찾음
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();

        // 모든 LineRenderer를 제거
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            Destroy(lineRenderer);
        }
        foreach (GameObject Entity in generatedEntities)
        {
            if (Entity != null)
            {
                Destroy(Entity);
            }
        }

        // 리스트를 비움
        generatedEntities.Clear();
    }
    private void PlaceEntities()
    {
        // 1. 플레이어 프리팹 생성
        GameObject playerPrefab = prefebManager.GetPlayerPrefab(0); // 플레이어 프리팹 가져오기 (0번 인덱스 사용)
        if (playerPrefab != null)
        {
            // 플레이어가 시작할 위치 (첫 번째 StartingPosition 사용)
            Vector3 playerPosition = GetRandomNavMeshPosition();
            GameObject player = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
            generatedEntities.Add(player);
            //player.GetComponent<CharacterBase>().Initialize(); // 플레이어 초기화
        }

        // 2. AI 프리팹 생성
        foreach (var aiController in prefebManager.aiCountControllers)
        {
            GameObject aiPrefab = aiController.Prefab;  // AI 프리팹 가져오기
            int aiCount = aiController.buttonController.GetValue();  // 각 AI 타입별로 몇 개를 생성할지 가져오기
            if (aiPrefab != null && aiCount > 0)
            {
                for (int j = 0; j < aiCount; j++)
                {
                    // AI가 시작할 위치
                    Vector3 aiPosition = GetRandomNavMeshPosition();
                    GameObject ai = Instantiate(aiPrefab, aiPosition, Quaternion.identity);
                    generatedEntities.Add(ai);
                    //ai.GetComponent<CharacterBase>().Initialize();  // AI 초기화
                }
            }
        }

    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(0, mapMaker.mapBlocksList.GetLength(0)),
            0,
            Random.Range(0, mapMaker.mapBlocksList.GetLength(1))
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return GetRandomNavMeshPosition();
    }
}
