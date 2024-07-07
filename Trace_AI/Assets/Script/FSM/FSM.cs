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

    public Dictionary<string, float> detectionTypeValueDict= new Dictionary<string, float>
    {
        { "VisionDetection", 10f },
        { "SoundDetection", 50f }
    };

    public Dictionary<string, float> detectionObjectValueDict = new Dictionary<string, float>
    {
        { "Player", 1f },
        { "canDetect", 0.5f }
    };

    public List<MoveBase> availableStates = new List<MoveBase>();
        
    private AI ai;

    void Start()
    {
        ai = GetComponent<AI>();
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
    }

    public void DetectUpdate(Dictionary<string, List<Transform>> detectedObjects)
    {
        List<string> detectionTypes = new List<string>(detectedObjects.Keys);
        foreach (var detectionType in detectionTypes)
        {
            foreach (var detectedObject in detectedObjects[detectionType])
            {
                string layerName = LayerMask.LayerToName(detectedObject.gameObject.layer);
                if (detectionObjectValueDict.ContainsKey(layerName))
                {
                    stateValue += detectionTypeValueDict[detectionType] * detectionObjectValueDict[layerName];
                }
            }
        }
        stateValue = Mathf.Clamp(stateValue, 0, stateMaxValue);
    }

    public bool CheckStateTransition()
    {
        if (currentState is not Trace && stateValue >= chaseThreshold)
        {
            SetState(ai.pursueState);
            ai.HandleEvent("SetBehavior");
            return true;
        }
        else if (currentState is not Patrol && stateValue <= patrolThreshold)
        {
            SetState(ai.patrolState);
            ai.HandleEvent("SetBehavior");
            return true;
        }
        return false;
    }

    public void UpdateFSM(Dictionary<string, List<Transform>> DetectedObjects, float stateDecrement)
    {
        stateValue -= stateDecrement;
        DetectUpdate(DetectedObjects);
        CheckStateTransition();
        //currentState?.Execute(this);
    }
}
