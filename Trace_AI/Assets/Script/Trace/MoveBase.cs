using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public abstract class MoveBase : MonoBehaviour
{
    [HideInInspector]
    public AI ai;
    protected FSM fsm;
    protected NavMeshAgent agent;
    public int FindClosestPoint(Vector3 currentPosition, List<Vector3> points)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(currentPosition, points[i]);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }

    public abstract void ArriveTargetPosition();

    public abstract Vector3 TraceTargetPosition();

    public abstract void Enter();

    public abstract void Execute();

    public abstract void Exit(MoveBase newState);

    public virtual void Initialize()
    {
        ai = gameObject.GetComponent<AI>();
        fsm = GetComponent<FSM>();
        agent = GetComponent<NavMeshAgent>();
    }
}
