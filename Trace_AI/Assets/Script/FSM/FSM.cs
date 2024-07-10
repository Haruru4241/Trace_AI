using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FSM : MonoBehaviour
{
    //public MoveBase PreviousState;
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

    void Awake()
    {
        ai = GetComponent<AI>();
    }

    public void InitializeStates(List<MoveBase> states)
    {
        availableStates = states;
        currentState = availableStates[0];
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

    public void SetState<T>() where T : MoveBase
    {
        currentState = availableStates.OfType<T>().FirstOrDefault();
        currentState.Enter();
    }

    public void UpdateFSM(Dictionary<string, List<Transform>> DetectedObjects, float stateDecrement)
    {
        stateValue -= stateDecrement;
        DetectUpdate(DetectedObjects);
        currentState?.Execute();
    }
}
