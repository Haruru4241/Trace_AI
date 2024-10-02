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

    [Header("Navigation Settings")] // NavMesh 관련 섹션
    [Tooltip("NavMesh Surfaces used for navigation")]
    private NavMeshSurface[] navMeshSurfaces;  // 네비게이션용 서피스

    [Header("Map Settings")] // 맵 생성 관련 섹션
    [Tooltip("Reference to the MapGenerator object")]
    public MapGenerator mapMaker;  // 맵 생성기 참조

    [Space(10)] // 10픽셀 간격 추가
    [Header("Prefab and Entity Management")] // 프리팹 및 엔티티 관리 섹션
    [Tooltip("Reference to the Prefab Manager")]
    public PrefebManager prefebManager;  // 프리팹 매니저

    [Tooltip("Parent object for the generated map")]
    public GameObject mapParent;  // 생성된 맵을 부모로 할 오브젝트

    [Tooltip("Parent object for generated entities")]
    public GameObject entityParent;  // 생성된 엔티티들의 부모 오브젝트

    [Tooltip("List of all generated entities")]
    public List<GameObject> generatedEntities = new List<GameObject>();  // 생성된 엔티티 목록

    [Header("Camera Settings")] // 카메라 관련 섹션
    [Tooltip("Main game camera")]
    public Camera gameCamera;  // 게임 카메라

    [Tooltip("Controller for the main camera")]
    public CameraController cameraController;  // 카메라 컨트롤러

    [Header("Tooltip Manager")] // 툴팁 매니저 섹션
    [Tooltip("Manages tooltips in the game")]
    public TooltipManager tooltipManager;  // 툴팁 매니저

    [Space(10)] // 10픽셀 간격 추가
    [Header("Game States and Debug")] // 게임 상태 및 디버그 관련 섹션
    [Tooltip("Is the map generated?")]
    public bool isMapGenerated = false;  // 맵이 생성되었는지 여부

    [Tooltip("Is the player located?")]
    public bool islocated = false;  // 플레이어의 위치 여부

    [Tooltip("Total collected coins")]
    [SerializeField]
    private int collectedCoins = 0;  // 수집된 코인의 수 (비공개)

    [Tooltip("Enable or disable debug mode")]
    public bool isDebugMode = true;  // 디버그 모드

    public void Awake()
    {
        mapMaker = GetComponent<MapGenerator>();
        if (mapMaker == null)
        {
            mapMaker = gameObject.AddComponent<DynamicMapGenerator>();
        }
        navMeshSurfaces = GetComponentsInChildren<NavMeshSurface>();
    }

    public void CoinCollected()
    {
        collectedCoins++; // 수집된 코인 개수 증가
        tooltipManager.ShowTooltip($"Coins: {collectedCoins}/{mapMaker.numberOfCoins}", null, Vector3.zero); // 스프라이트는 null로 설정


        // 모든 코인을 수집했는지 확인
        if (collectedCoins >= mapMaker.numberOfCoins)
        {
            DebugLog("모든 코인을 수집했습니다. 승리!");

            // 승리 툴팁 메시지로 "굿"을 표시
            tooltipManager.ShowTooltip("Good! You've collected all the coins!", null, Vector3.zero);
        }
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
        cameraController.SetCamera(mapMaker.mapSize);

        foreach (var surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        isMapGenerated = true;
        islocated = true;
        PlaceEntities();
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
                var Character = entity.GetComponent<CharacterBase>();
                if (Character != null) Character.Initialize(); // 플레이어 초기화
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
        foreach (GameObject Entity in generatedEntities)
        {
            if (Entity != null)
            {
                var a = Entity.GetComponent<AI>();
                if (a != null) a.ClearAI();
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
            player.transform.parent = entityParent.transform;
            generatedEntities.Add(player);
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
                    ai.transform.parent = entityParent.transform;
                    generatedEntities.Add(ai);
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
    public void DebugLog(string message)
    {
        if (isDebugMode)
        {
            Debug.Log(message); // 디버그 모드가 켜져 있을 때만 로그 출력
        }
    }
}
