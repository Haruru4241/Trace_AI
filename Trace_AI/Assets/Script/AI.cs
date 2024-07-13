using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI : CharacterBase
{
    public List<Detection> Detections;

    public List<DetectionWeight> detectionWeightsList;
    public List<LayerValue> layerValuesList;

    private Dictionary<string, float> detectionWeights;
    private Dictionary<string, float> layerValueDict;

    public Dictionary<Transform, float> targetList = new Dictionary<Transform, float>();
    private Vector3 targetPosition;

    Renderer AIrenderer;
    NavMeshAgent m_Agent;
    FSM fsm;

    protected override void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<FSM>();
        AIrenderer = GetComponent<Renderer>();

        detectionWeights = detectionWeightsList.ToDictionary(dw => dw.detectionType, dw => dw.value);
        layerValueDict = layerValuesList.ToDictionary(lv => lv.layerName, lv => lv.value);

        // 이벤트 처리기 설정
        

        base.Awake();
    }

    private void OnEnable()
    {
        GameEventSystem.OnAiAdditionalEvent += UpdateColor;
        GameEventSystem.OnTargetDestroyed += HandleTargetDestroyed;
    }

    private void OnDisable()
    {
        GameEventSystem.OnAiAdditionalEvent -= UpdateColor;
        GameEventSystem.OnTargetDestroyed -= HandleTargetDestroyed;
    }

    void Update()
    {
        fsm.UpdateFSM(Detections);
        UpdateTargetList(Detections);
        AdjustTargetListValues();

        targetPosition =fsm.currentState.TraceTargetPosition();

        if (m_Agent.pathStatus==NavMeshPathStatus.PathComplete
            && m_Agent.remainingDistance- m_Agent.stoppingDistance<0.1f)
        {
            targetPosition = fsm.currentState.ArriveTargetPosition();
        }
        m_Agent.destination = targetPosition;

        GameEventSystem.RaiseAiAdditionalEvent();
    }

    private void UpdateTargetList(List<Detection> detections)
    {
        Dictionary<Transform, float> updateList = new Dictionary<Transform, float>();

        // 각 Detection 객체에 대해 반복
        foreach (var detection in detections)
        {
            // 감지된 객체 리스트를 가져옴
            List<Transform> detected = detection.Detect();
            string detectionType = detection.GetType().Name;

            if (detectionWeights.TryGetValue(detectionType, out float weight))
            {
                foreach (var target in detected)
                {
                    if (updateList.ContainsKey(target) && weight > updateList[target])
                    {
                        updateList[target] = weight; // 더 높은 가중치로 업데이트
                    }
                    else
                    {
                        updateList[target] = weight;
                    }
                }
            }
        }

        // 2차적으로 타겟 리스트 업데이트
        foreach (var target in updateList.Keys)
        {
            if (targetList.ContainsKey(target))
            {
                targetList[target] += updateList[target]; // 존재하면 가중치 더하기
                targetList[target] = Mathf.Min(targetList[target], fsm.stateMaxValue); // 최대값 제한
            }
            else
            {
                targetList[target] = updateList[target]; // 존재하지 않으면 추가
            }
        }

        // 타겟 리스트를 값의 내림차순으로 정렬
        targetList = targetList.OrderByDescending(t => t.Value).ToDictionary(t => t.Key, t => t.Value);
    }

    private void HandleTargetDestroyed(object sender, GameEventArgs e)
    {
        if (targetList.ContainsKey(e.Source))
        {
            targetList.Remove(e.Source);
        }
    }

    private void AdjustTargetListValues()
    {
        var keys = targetList.Keys.ToList();
        foreach (var key in keys)
        {
            string layerName = LayerMask.LayerToName(key.gameObject.layer);
            if (layerValueDict.TryGetValue(layerName, out float layerValue))
            {
                targetList[key] -= layerValue;
                if (targetList[key] <= 0)
                {
                    targetList.Remove(key); // 밸류가 0 이하가 되면 리스트에서 제거
                }
            }
        }
    }

    public void SetTargetPosition(Vector3 _targetPosition)
    {
        targetPosition = _targetPosition;
    }

    public override void UpdateSpeed(float value)
    {
        if (m_Agent != null) m_Agent.speed = value;
    }

    void UpdateColor()
    {
        float stateValue = 0;
        if (targetList.Any())
        {
            stateValue = targetList.First().Value;
        }

        float t = Mathf.InverseLerp(0, fsm.chaseThreshold, stateValue);
        AIrenderer.material.color = Color.Lerp(Color.green, Color.red, t);
    }
}