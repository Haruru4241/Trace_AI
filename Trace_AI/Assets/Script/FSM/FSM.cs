using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FSM : MonoBehaviour
{
    [Header("State Settings")]
    [Tooltip("Maximum value for the state system")]
    public float stateMaxValue = 200f;  // 상태 시스템의 최대값

    [Tooltip("List of all possible states this AI can transition to")]
    public List<MoveBase> availableStates = new List<MoveBase>();  // AI가 사용할 수 있는 상태 목록

    [Tooltip("The current state the AI is in")]
    public MoveBase currentState;  // AI의 현재 상태

    [Space(10)]
    [Header("State Transition Rules")]
    [Tooltip("List of state transition rules that define how AI transitions between states")]
    [SerializeField] private List<StateTransitionRule> stateTransitionRules;  // 상태 전이 규칙

    [SerializeField] private MoveBase IndicateState;

    [Space(10)]
    [Header("Detection Weights and Layer Values")]
    [Tooltip("List of detection weights that influence AI behavior")]
    public List<DetectionWeight> detectionWeightsList;  // 감지 가중치 목록

    [Tooltip("List of layer values that are used for layer-based AI behavior")]
    public List<LayerValue> layerValuesList;  // 레이어 값 목록

    [Space(10)]
    [Header("Internal State Data")]
    [Tooltip("Dictionary for storing detection weights dynamically")]
    private Dictionary<string, float> detectionWeights;  // 감지 가중치 딕셔너리 (내부 용도)

    [Tooltip("Dictionary for storing layer values dynamically")]
    private Dictionary<string, float> layerValueDict;  // 레이어 값 딕셔너리 (내부 용도)


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

    public void SetState(MoveBase newState)
    {
        currentState = newState;
        // 현재 상태가 지정된 상태와 동일하면 탈출 시 프로젝터 초기화
        if (currentState == IndicateState)
        {
            newState.ai.projectorManager.ChangeAllProjectorsToChangedColor();  // 상태 진입 시 변경
        }
        else if (currentState != IndicateState)
        {
            newState.ai.projectorManager.ChangeAllProjectorsToInitialColor();  // 상태 탈출 시 초기화
        }
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
        //public Detection DetectionType;
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