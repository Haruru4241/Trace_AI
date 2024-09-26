using System;
using System.Collections.Generic;
using System.Linq;
using OpenCover.Framework.Model;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public float chaseThreshold = 100f;
    public float patrolThreshold = 50f;
    public float stateMaxValue = 200f;

    public List<MoveBase> availableStates = new List<MoveBase>();
    public MoveBase currentState;
    [SerializeField] private List<StateTransitionRule> stateTransitionRules;
    public List<DetectionWeight> detectionWeightsList;
    public List<LayerValue> layerValuesList;

    private Dictionary<string, float> detectionWeights;
    private Dictionary<string, float> layerValueDict;

    public void Initialize()
    {
        detectionWeights = detectionWeightsList.ToDictionary(dw => dw.detectionType, dw => dw.value);
        layerValueDict = layerValuesList.ToDictionary(lv => lv.layerName, lv => lv.value);

        currentState = availableStates[0];
        foreach (MoveBase state in availableStates)
        {
            state.Initialize();
        }
    }

    public void SetState<T>() where T : MoveBase
    {
        currentState = availableStates.OfType<T>().FirstOrDefault();
        currentState.Enter();
    }

    public void SetState(MoveBase newState)
    {
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateTargetList(List<Detection> detections, ref Dictionary<Transform, float> targetList)
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
                    if (!updateList.ContainsKey(target) || weight > updateList[target])
                    {
                        updateList[target] = weight; // �� ���� ����ġ�� ������Ʈ
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
                targetList[target] = Mathf.Min(targetList[target], stateMaxValue); // �ִ밪 ����
            }
            else
            {
                targetList[target] = updateList[target]; // �������� ������ �߰�
            }
        }

        // Ÿ�� ����Ʈ�� ���� ������������ ����
        targetList = targetList.OrderByDescending(t => t.Value).ToDictionary(t => t.Key, t => t.Value);
    }

    public void AdjustTargetListValues(ref Dictionary<Transform, float> targetList)
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
    public List<StateTransitionRule> FindStatetargetState(MoveBase currentState)
    {
        return stateTransitionRules.Where(rule => rule.targetState == currentState).ToList();
    }
    public List<StateTransitionRule> FindStateescapeState(MoveBase currentState)
    {
        return stateTransitionRules.Where(rule => rule.escapeState == currentState).ToList();
    }

    public void UpdateFSM(List<Detection> Detections, ref Dictionary<Transform, float> targetList)
    {
        UpdateTargetList(Detections, ref targetList);
        AdjustTargetListValues(ref targetList);
        currentState?.Execute();
    }

    [Serializable]
    public class DetectionWeight
    {
        public string detectionType;
        public float value;
    }
    [Serializable]
    public class LayerValue
    {
        public string layerName;
        public float value;
    }
}

[System.Serializable]
public class StateTransitionRule
{
    public MoveBase targetState; // 탈출 전 상태
    public StateCondition ExitCondition; // 상태 전환 조건 (거리 등)
    public MoveBase escapeState; // 탈출 조건 발생 시 진입할 상태

    public StateTransitionRule(MoveBase escapeState, MoveBase targetState, StateCondition ExitCondition)
    {
        this.escapeState = escapeState;
        this.targetState = targetState;
        this.ExitCondition = ExitCondition;
    }
}

public abstract class StateCondition : MonoBehaviour
{
    public abstract bool ExitCondition();
}

// 추상 클래스를 상속받는 구체적인 상태 조건