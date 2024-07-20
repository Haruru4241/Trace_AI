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
    [HideInInspector]
    public MoveBase currentState;

    public void Initialize()
    {
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

    public void UpdateFSM(List<Detection> Detections)
    {
        currentState?.Execute();
    }
}
