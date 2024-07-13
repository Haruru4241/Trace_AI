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

        // �̺�Ʈ ó���� ����
        

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

        // �� Detection ��ü�� ���� �ݺ�
        foreach (var detection in detections)
        {
            // ������ ��ü ����Ʈ�� ������
            List<Transform> detected = detection.Detect();
            string detectionType = detection.GetType().Name;

            if (detectionWeights.TryGetValue(detectionType, out float weight))
            {
                foreach (var target in detected)
                {
                    if (updateList.ContainsKey(target) && weight > updateList[target])
                    {
                        updateList[target] = weight; // �� ���� ����ġ�� ������Ʈ
                    }
                    else
                    {
                        updateList[target] = weight;
                    }
                }
            }
        }

        // 2�������� Ÿ�� ����Ʈ ������Ʈ
        foreach (var target in updateList.Keys)
        {
            if (targetList.ContainsKey(target))
            {
                targetList[target] += updateList[target]; // �����ϸ� ����ġ ���ϱ�
                targetList[target] = Mathf.Min(targetList[target], fsm.stateMaxValue); // �ִ밪 ����
            }
            else
            {
                targetList[target] = updateList[target]; // �������� ������ �߰�
            }
        }

        // Ÿ�� ����Ʈ�� ���� ������������ ����
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
                    targetList.Remove(key); // ����� 0 ���ϰ� �Ǹ� ����Ʈ���� ����
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