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

    private void Awake()
    {
        currentState = availableStates[0];
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
