using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public FSM fsm;
    public Trace trace;
    public Detection detection;
    public MoveBase stateHandler;
    public Vector3 targetPosition;

    public Transform player;
    public float moveSpeed = 5f;
    public float stateMaxValue = 200f;
    public Renderer renderer;
    public List<Node> currentPath;

    public Patrol patrolState;
    public MoveBase pursueState;

    public float stateDecrement = 1f;

    public Vector3 previousPosition;

    public List<Detection> detections = new List<Detection>();

    public Grid grid;  // grid 변수 정의

    public event Action<string> OnEventOccurred;

    void Start()
    {
        if (fsm == null) fsm = gameObject.AddComponent<FSM>();
        if (trace == null) trace = gameObject.AddComponent<Trace>(); // Trace 클래스 추가
        if (renderer == null) renderer = GetComponent<Renderer>();
        if (grid == null) grid = FindObjectOfType<Grid>();

        trace.grid = grid;
        trace.player = player;

        if (patrolState == null) patrolState = gameObject.AddComponent<Patrol>();
        if (pursueState == null) pursueState = gameObject.AddComponent<Trace>(); // Trace 클래스로 대체

        patrolState.grid = grid;
        patrolState.player = player;
        pursueState.grid = grid;
        pursueState.player = player;

        patrolState.patrolPoints = patrolState.GenerateRandomPatrolPoints(); // 순찰 지점 생성

        // 미리 적용된 Detection 컴포넌트 인식
        detections.AddRange(GetComponents<Detection>());

        if (player == null)
        {
            Debug.LogError("Player is not set in AI");
        }
        else
        {
            foreach (var detection in detections)
            {
                detection.SetPlayer(player);
                detection.SetAITransform(transform);
            }
        }

        fsm.InitializeStates(new List<MoveBase> { patrolState, pursueState });

        OnEventOccurred += HandleEvent;

        StartCoroutine(LogDebugInfo());

        previousPosition = player.position;
    }

    void Update()
    {
        // 플레이어 움직임 감지
        foreach (var detection in detections)
        {
            if (detection is SoundDetection soundDetection)
            {
                soundDetection.DetectPlayerMovement(player.GetComponent<PlayerMovement>());
            }
        }

        // 감지된 횟수를 측정
        var detectedTypes = UpdateDetections();

        // FSM 상태 업데이트
        fsm.UpdateFSM(detectedTypes, stateDecrement);

        // 타겟 포지션 및 경로 업데이트
        UpdateTargetAndPath();

        // 타겟 포지션을 향해 이동
        MoveAlongPath();

        // AI 추가 메서드 실행
        ExecuteAdditionalMethods();

        // 이전 위치 업데이트
        previousPosition = player.position;
    }

    public void HandleEvent(string eventType)
    {
        switch (eventType)
        {
            case "SetBehavior":
                stateHandler = fsm.currentState;
                stateHandler.UpdateTargetPosition(transform.position, out targetPosition);
                currentPath = stateHandler.UpdatePath(transform.position, targetPosition);
                break;
            case "TargetPosition":
                stateHandler.HandleEvent(this, "TargetPosition");
                break;
            case "PathNode":
                stateHandler.HandleEvent(this, "PathNode");
                break;
            default:
                Debug.LogWarning($"Unhandled event type: {eventType}");
                break;
        }
    }

    void UpdateTargetAndPath()
    {
        if (HasReachedPosition(transform.position, targetPosition, 0.1f))
        {
            OnEventOccurred?.Invoke("TargetPosition");
        }
        else if (currentPath != null && currentPath.Count > 0 && HasReachedPosition(transform.position, currentPath[0].worldPosition, 0.1f))
        {
            OnEventOccurred?.Invoke("PathNode");
        }
    }

    void MoveAlongPath()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Vector3 nodePosition = currentPath[0].worldPosition;
            nodePosition.y = transform.position.y;

            trace.MoveToNode(transform, nodePosition, moveSpeed);
        }
    }

    bool HasReachedPosition(Vector3 a, Vector3 b, float distanceThreshold)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z)) < distanceThreshold;
    }

    void ExecuteAdditionalMethods()
    {
        // 딕셔너리로 관리되는 추가 메서드 실행
        if (additionalMethods != null)
        {
            foreach (var method in additionalMethods)
            {
                if (method.Value)
                {
                    method.Key();
                }
            }
        }
    }

    // 딕셔너리로 관리되는 추가 메서드
    public Dictionary<System.Action, bool> additionalMethods;

    void UpdateColor()
    {
        float stateValue = fsm.stateValue;
        float t = Mathf.InverseLerp(0, fsm.chaseThreshold, stateValue);
        renderer.material.color = Color.Lerp(Color.green, Color.red, t);
    }

    IEnumerator LogDebugInfo()
    {
        while (true)
        {
            try
            {
                Debug.Log($"Current State: {fsm.currentState.GetType().Name}");
                Debug.Log($"Current Position: {transform.position}");
                Debug.Log($"Target Position: {targetPosition}");
                Debug.Log($"Next Node Position: {currentPath?[0].worldPosition ?? Vector3.zero}");
                Debug.Log($"Move Speed: {moveSpeed}");
                Debug.Log($"State Value: {fsm.stateValue}");
                Debug.Log($"Previous State: {fsm.PreviousState?.GetType().Name ?? "None"}");
                Debug.Log($"Remaining Path Nodes: {currentPath?.Count ?? 0}");
                Debug.Log($"Distance to Player: {Vector3.Distance(transform.position, player.position)}");
                Debug.Log($"Distance to Target Point: {Vector3.Distance(transform.position, targetPosition)}");
                Debug.Log($"AI Direction: {transform.forward}");
            }
            catch (Exception ex)
            {
                Debug.Log($"Error in LogDebugInfo: {ex.Message}");
            }

            yield return new WaitForSeconds(20f);
        }
    }

    Dictionary<string, int> UpdateDetections()
    {
        Dictionary<string, int> detectionCounts = new Dictionary<string, int>();

        foreach (var detection in detections)
        {
            bool detected = detection.Detect();
            if (detected)
            {
                string type = detection.GetType().Name;
                if (detectionCounts.ContainsKey(type))
                {
                    detectionCounts[type]++;
                }
                else
                {
                    detectionCounts[type] = 1;
                }
            }
        }

        return detectionCounts;
    }
}
