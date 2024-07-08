using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    FSM fsm;
    MoveBase stateHandler;

    public Patrol patrolState;
    public MoveBase pursueState;

    public Transform player;
    public float moveSpeed = 5f;
    public float stateDecrement = 1f;

    public List<Detection> Detections;
    public List<MoveBase> MoveStates;

    [System.Serializable]
    public class LayerPenalty
    {
        public LayerMask layer;
        public int penalty;
    }
    [HideInInspector]
    public List<LayerPenalty> layerPenaltiesArray; // 인스펙터 창에 노출될 리스트

    public event Action<string> OnEventOccurred;
    private Dictionary<int, int> layerPenalties = new Dictionary<int, int>(); // 레이어별 가중치
    private Renderer AIrenderer;
    private List<Node> currentPath;
    private Vector3 targetPosition;

    void Start()
    {
        fsm = GetComponent<FSM>();
        AIrenderer = GetComponent<Renderer>();
        foreach (LayerPenalty layerPenalty in layerPenaltiesArray)
        {
            layerPenalties.Add(layerPenalty.layer.value, layerPenalty.penalty);
        }

        // 순찰 상태와 추적 상태 초기화
        foreach (var state in MoveStates)
        {
            state.Initialize(this, layerPenalties);
        }

        foreach (var detection in Detections)
        {
            detection.Initialize(transform, player);
        }

        // 상태 초기화
        fsm.InitializeStates(MoveStates);

        // 이벤트 처리기 설정
        OnEventOccurred += HandleEvent;

        additionalMethods = new Dictionary<System.Action, bool>
    {
        { UpdateColor, true }
    };
    }

    void Update()
    {
        var detectedObjects = Detections[0].DetectedObject(Detections);

        fsm.UpdateFSM(detectedObjects, stateDecrement);

        UpdateTargetAndPath();

        MoveAlongPath();

        ExecuteAdditionalMethods();
    }

    public void HandleEvent(string eventType)
    {
        switch (eventType)
        {
            case "SetBehavior":
                stateHandler = fsm.currentState;
                stateHandler.UpdateTargetPosition(transform.position, ref targetPosition);
                stateHandler.UpdatePath(transform.position, targetPosition, ref currentPath);
                break;
            case "TargetPosition":
                stateHandler.HandleEvent(ref targetPosition, ref currentPath, "TargetPosition");
                break;
            case "PathNode":
                stateHandler.HandleEvent(ref targetPosition, ref currentPath, "PathNode");
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
        AIrenderer.material.color = Color.Lerp(Color.green, Color.red, t);
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