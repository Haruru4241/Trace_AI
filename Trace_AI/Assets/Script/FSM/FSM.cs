using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public float chaseThreshold = 100f;
    public float patrolThreshold = 50f;
    public float stateMaxValue = 200f;

    public List<MoveBase> availableStates = new List<MoveBase>();
    public MoveBase currentState;

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

