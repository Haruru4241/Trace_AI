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
                    if (!updateList.ContainsKey(target) || weight > updateList[target])
                    {
                        updateList[target] = weight; // 더 높은 가중치로 업데이트
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
                targetList[target] = Mathf.Min(targetList[target], stateMaxValue); // 최대값 제한
            }
            else
            {
                targetList[target] = updateList[target]; // 존재하지 않으면 추가
            }
        }

        // 타겟 리스트를 값의 내림차순으로 정렬
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
                    targetList.Remove(key); // 밸류가 0 이하가 되면 리스트에서 제거
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

