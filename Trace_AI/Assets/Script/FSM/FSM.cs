using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public MoveBase PreviousState;
    public MoveBase currentState;

    public float chaseThreshold = 100f;
    public float patrolThreshold = 50f;
    public float stateMaxValue = 200f;
    public float stateValue;

    [System.Serializable]
    public class DetectionValue
    {
        public string detectionType;
        public float value;
    }
    public List<DetectionValue> detectionValues = new List<DetectionValue>();

    private Dictionary<string, float> detectionValueDict;

    public List<MoveBase> availableStates = new List<MoveBase>();

    private MoveBase trace;
    private AI ai;

    void Start()
    {
        trace = GetComponent<MoveBase>();
        ai = GetComponent<AI>();

        // detectionValues 리스트를 Dictionary로 변환
        detectionValueDict = new Dictionary<string, float>();

        // 기본 감지 스크립트 설정
        AddDefaultDetectionValues();

        foreach (var detectionValue in detectionValues)
        {
            detectionValueDict[detectionValue.detectionType] = detectionValue.value;
        }
    }

    private void AddDefaultDetectionValues()
    {
        detectionValueDict["VisionDetection"] = 10f;
        detectionValueDict["SoundDetection"] = 5f;
        detectionValueDict["OptimizedVisionDetection"] = 7f;  // 추가 감지 스크립트
    }

    public void InitializeStates(List<MoveBase> states)
    {
        availableStates = states;
        currentState = availableStates[0];
        ai.HandleEvent("SetBehavior");
    }

    public void SetState(MoveBase newStateHandler)
    {
        PreviousState = currentState;
        currentState = newStateHandler;
        ai.HandleEvent("SetBehavior");
    }

    public void OnDetection(Dictionary<string, int> detectionCounts)
    {
        List<string> detectionTypes = new List<string>(detectionCounts.Keys);
        foreach (var detectionType in detectionTypes)
        {
            if (detectionValueDict.ContainsKey(detectionType))
            {
                stateValue += detectionValueDict[detectionType] * detectionCounts[detectionType];
            }
            else
            {
                stateValue += 5 * detectionCounts[detectionType]; // 기본 값 5 설정
                Debug.LogWarning($"Unhandled detection type: {detectionType}");
            }
        }
        stateValue = Mathf.Clamp(stateValue, 0, stateMaxValue);
    }

    public bool CheckStateTransition()
    {
        if (currentState is Patrol && stateValue >= chaseThreshold)
        {
            SetState(ai.pursueState);
            return true;
        }
        else if (currentState is Trace && stateValue <= patrolThreshold)
        {
            SetState(ai.patrolState);
            return true;
        }
        return false;
    }

    public void UpdateFSM(Dictionary<string, int> detectedTypes, float stateDecrement)
    {
        stateValue -= stateDecrement;
        OnDetection(detectedTypes);
        CheckStateTransition();
    }
}
