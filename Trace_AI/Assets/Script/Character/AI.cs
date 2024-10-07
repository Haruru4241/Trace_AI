using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI : CharacterBase
{
    [Header("Detection Settings")]
    [Tooltip("List of all detection components associated with this AI")]
    public List<Detection> Detections;  // 감지 컴포넌트 목록

    [Space(10)]
    [Header("Target and State Management")]
    [Tooltip("Current list of targets and their respective distances")]
    public Dictionary<Transform, float> targetList = new Dictionary<Transform, float>();  // 타겟과 거리 목록

    [Tooltip("Serialized version of target list for easier debugging or display")]
    public TransformFloatPair showtargetList = new TransformFloatPair();  // 타겟 목록을 보여주기 위한 변수

    [Tooltip("Position of the current target")]
    public Vector3 targetPosition;  // 현재 타겟 위치

    [Tooltip("Current state value of the AI")]
    public float stateValue = 0f;  // AI 상태 값

    [Space(10)]
    [Header("AI Components")]
    [Tooltip("Renderer component for the AI's visual representation")]
    private Renderer AIrenderer;  // AI의 렌더러

    [Tooltip("NavMeshAgent component for AI movement")]
    private NavMeshAgent m_Agent;  // 네비게이션 에이전트

    [Tooltip("Finite State Machine (FSM) controlling AI behavior")]
    private FSM fsm;  // FSM 관리

    [Tooltip("LineRenderer component for drawing paths or debugging AI behavior")]
    private LineRenderer lineRenderer;  // 라인 렌더러 (디버깅 및 경로 표시용)

    [Space(10)]
    [Header("Target Visualization")]
    [Tooltip("GameObject used to visually represent the current target")]
    private GameObject targetSphere;  // 목표 구체를 시각적으로 표현

    [Space(10)]
    [Header("Entity Management")]
    [Tooltip("List of entities interacting with this AI")]
    private List<GameObject> entities = new List<GameObject>();  // 상호작용하는 엔티티 목록

    [Tooltip("Reference to the Projector Manager used for visual effects")]
    public ProjectorManager projectorManager;  // 프로젝터 매니저 참조

    [Space(10)]
    [Header("Game Control")]
    [Tooltip("Indicates if the game has started for this AI")]
    public bool isGameStarted = false;  // 게임이 시작되었는지 여부
    
    public void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<FSM>();
        fsm.Initialize();
        AIrenderer = GetComponent<Renderer>();
        // LineRenderer 초기화
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;  // 시작 선의 두께
        lineRenderer.endWidth = 0.1f;    // 끝 선의 두께
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  // 기본 재질 설정
        lineRenderer.positionCount = 2;  // 선을 그릴 두 개의 포인트 필요
        lineRenderer.material.color = Color.blue;

        // 목표 위치에 작은 구체 생성
        targetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetSphere.transform.position = targetPosition;  // 목표 위치에 구체 배치
        targetSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);  // 구체 크기 설정
        targetSphere.GetComponent<Renderer>().material.color = Color.red;   // 구체 색상 설정 (빨간색)
        Destroy(targetSphere.GetComponent<Collider>());  // Collider 제거
        entities.Add(targetSphere);

        m_Agent.avoidancePriority = UnityEngine.Random.Range(1, 1000);
        if (isGameStarted)Initialize();
    }
    public void ClearAI()
    {
        foreach (var entity in entities)
        {
            Destroy(entity);
        }
        entities.Clear();
    }

    public override void Initialize()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            base.Initialize();

            // LineRenderer의 두 개의 포인트 설정
            lineRenderer.positionCount = 2;  // 선을 그릴 두 개의 포인트 필요
            lineRenderer.SetPosition(0, transform.position);  // 시작점: Transform의 위치
            lineRenderer.SetPosition(1, targetPosition);      // 끝점: Target 위치
        }
    }

    void FixedUpdate()
    {
        if (!isGameStarted) return;

        if (targetList.Count > 0)
        {
            // Dictionary의 첫 번째 항목을 가져오기
            foreach (var target in targetList)
            {
                showtargetList.target = target.Key;
                showtargetList.value = target.Value;
                break; // 첫 번째 항목만 가져오므로 루프를 종료
            }
        }

        fsm.UpdateFSM(Detections, ref targetList);
        lineRenderer.SetPosition(0, transform.position);
        if (targetList.Any()) stateValue = targetList.First().Value;
        Vector3 curentTarget = fsm.currentState.TraceTargetPosition();
        if (targetPosition != curentTarget)
        {
            targetPosition = curentTarget;
            lineRenderer.SetPosition(1, targetPosition);

            // 목표 구체 위치도 업데이트
            targetSphere.transform.position = targetPosition;
        }

        m_Agent.destination = targetPosition;

        GameEventSystem.RaiseAiAdditionalEvent();
    }

    private void OnEnable()
    {
        GameEventSystem.OnTargetDestroyed += HandleTargetDestroyed;
    }

    private void OnDisable()
    {
        GameEventSystem.OnTargetDestroyed -= HandleTargetDestroyed;
    }

    public void SetTargetPosition(Vector3 _targetPosition)
    {
        targetPosition = _targetPosition;
    }

    public override void UpdateSpeed(float value)
    {
        currentMoveSpeed = value;
        if (m_Agent != null) m_Agent.speed = value;
    }

    private void HandleTargetDestroyed(object sender, GameEventArgs e)
    {
        if (targetList.ContainsKey(e.Source))
        {
            targetList.Remove(e.Source);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 1f);
    }
}
[Serializable]
public class TransformFloatPair
{
    public Transform target;
    public float value;
}