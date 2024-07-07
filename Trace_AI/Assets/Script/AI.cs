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
    public Renderer erenderer;
    public List<Node> currentPath;

    public Patrol patrolState;
    public MoveBase pursueState;

    public float stateDecrement = 1f;

    public Vector3 previousPosition;

    public List<Detection> detections = new List<Detection>();

    public Grid grid;

    public event Action<string> OnEventOccurred;

    public List<MoveBase> initialStates;

    public LayerMask[] layerMasks; // 레이어 마스크 배열
    public int[] penalties; // 패널티 값 배열
    

    [System.Serializable]
    public class LayerPenalty
    {
        public LayerMask layer;
        public int penalty;
    }
    public LayerPenalty[] layerPenaltiesArray;
    public Dictionary<int, int> layerPenalties = new Dictionary<int, int>(); // 레이어별 가중치

    void Start()
    {
        for (int i = 0; i < layerMasks.Length; i++)
        {
            layerPenalties[layerMasks[i].value] = penalties[i];
        }
        /*        foreach (LayerPenalty layerPenalty in layerPenaltiesArray)
                {
                    int layer = layerPenalty.layer.value;
                    if (!layerPenalties.ContainsKey(layer))
                    {
                        layerPenalties.Add(layer, layerPenalty.penalty);
                    }
                }*/
        initialStates = new List<MoveBase> { patrolState, pursueState };
        // 순찰 상태와 추적 상태 초기화
        foreach (var state in initialStates)
        {
            state.Initialize(this, layerPenalties);
        }
        

        // 감지 도구 설정
        detections.AddRange(GetComponents<Detection>());

        foreach (var detection in detections)
        {
            detection.Initialize(transform, player);
        }

        // 상태 초기화
        fsm.InitializeStates(initialStates);

        // 이벤트 처리기 설정
        OnEventOccurred += HandleEvent;

        additionalMethods = new Dictionary<System.Action, bool>
    {
        { UpdateColor, true }
    };



        // 이전 위치 설정
        previousPosition = player.position;
    }

    void Update()
    {
        var detectedObjects = detection.DetectedObject(detections);

        fsm.UpdateFSM(detectedObjects, stateDecrement);

        UpdateTargetAndPath();

        MoveAlongPath();

        ExecuteAdditionalMethods();

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

            fsm.currentState.MoveToNode(transform, nodePosition, moveSpeed);
        }
    }

    bool HasReachedPosition(Vector3 a, Vector3 b, float distanceThreshold)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z)) < distanceThreshold;
    }

    void ExecuteAdditionalMethods()
    {
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

    public Dictionary<System.Action, bool> additionalMethods;

    void UpdateColor()
    {
        float stateValue = fsm.stateValue;
        float t = Mathf.InverseLerp(0, fsm.chaseThreshold, stateValue);
        erenderer.material.color = Color.Lerp(Color.green, Color.red, t);
    }

    void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i].worldPosition, currentPath[i + 1].worldPosition);
                Gizmos.DrawSphere(currentPath[i].worldPosition, 0.2f);
            }

            Gizmos.DrawSphere(currentPath[currentPath.Count - 1].worldPosition, 0.2f);
        }
    }
}