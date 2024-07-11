using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : CharacterBase
{
    FSM fsm;

    public float moveSpeed = 5f;
    public float stateDecrement = 1f;

    public List<Detection> Detections;

    [System.Serializable]
    public class LayerPenalty
    {
        public LayerMask layer;
        public int penalty;
    }
    [HideInInspector]
    public List<LayerPenalty> layerPenaltiesArray; // 인스펙터 창에 노출될 리스트

    public Dictionary<int, int> layerPenalties = new Dictionary<int, int>(); // 레이어별 가중치
    private Renderer AIrenderer;
    private List<Node> currentPath;
    private Vector3 targetPosition;
    NavMeshAgent m_Agent;

    protected override void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<FSM>();
        AIrenderer = GetComponent<Renderer>();
        foreach (LayerPenalty layerPenalty in layerPenaltiesArray)
        {
            layerPenalties.Add(layerPenalty.layer.value, layerPenalty.penalty);
        }

        // 이벤트 처리기 설정
        GameEventSystem.OnAiAdditionalEvent += UpdateColor;

        base.Awake();
    }

    void Update()
    {
        var detectedObjects = Detections[0].DetectedObject(Detections);

        fsm.UpdateFSM(detectedObjects, stateDecrement);
        targetPosition=fsm.currentState.TraceTargetPosition();
        m_Agent.destination = targetPosition;
        
        if (m_Agent.pathStatus==NavMeshPathStatus.PathComplete
            && m_Agent.remainingDistance- m_Agent.stoppingDistance<0.1f)
        {
            targetPosition = fsm.currentState.ArriveTargetPosition();
            m_Agent.destination = targetPosition;
        }

        GameEventSystem.RaiseAiAdditionalEvent();
    }
    public void SetTargetPosition(Vector3 _targetPosition)
    {
        targetPosition = _targetPosition;
    }

    public List<Transform> GetPriorityTargets(Dictionary<string, List<Transform>> detectedObjects)
    {
        // 감지 유형별 우선순위 맵
        Dictionary<string, int> priorityMap = new Dictionary<string, int>
        {
            { "SightDetection", 2 },
            { "SoundDetection", 1 }
        };

        Dictionary<Transform, int> objectWeights = new Dictionary<Transform, int>();

        foreach (var kvp in detectedObjects)
        {
            string type = kvp.Key;
            List<Transform> objects = kvp.Value;

            if (priorityMap.ContainsKey(type))
            {
                int priority = priorityMap[type];

                foreach (var obj in objects)
                {
                    int layerWeight = GetLayerWeight(obj.gameObject.layer);
                    int weight = priority * layerWeight;

                    if (objectWeights.ContainsKey(obj))
                    {
                        objectWeights[obj] = Mathf.Max(objectWeights[obj], weight);
                    }
                    else
                    {
                        objectWeights[obj] = weight;
                    }
                }
            }
        }

        // 우선순위에 따라 정렬된 타겟 리스트 생성
        List<Transform> priorityTargets = new List<Transform>(objectWeights.Keys);
        priorityTargets.Sort((a, b) => objectWeights[b].CompareTo(objectWeights[a]));

        return priorityTargets;
    }

    private int GetLayerWeight(int layer)
    {
        // 레이어에 따라 가중치를 설정 (예시: 레이어에 따른 가중치 매핑)
        switch (LayerMask.LayerToName(layer))
        {
            case "Enemy":
                return 3;
            case "Item":
                return 2;
            case "Ally":
                return 1;
            default:
                return 1;
        }
    }

    bool HasReachedPosition(Vector3 a, Vector3 b, float distanceThreshold)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z)) < distanceThreshold;
    }

    public override void UpdateSpeed(float value)
    {
        m_Agent.speed = value;
    }

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