using System.Collections.Generic;
using UnityEngine;

public class Patrol : MoveBase
{
    public List<Vector3> patrolPoints;
    public Color Color=Color.green;
    private int patrolIndex;

    public override void Initialize(AI AI)
    {
        ai = AI;
        // ¼øÂû ÁöÁ¡ »ý¼º
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            patrolPoints = GenerateRandomPatrolPoints(4);
        }

        // ¼øÂû ÀÎµ¦½º ÃÊ±âÈ­
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
    }

    public override void Enter()
    {
        ai.HandleEvent("SetBehavior");
    }

    public override void Execute(FSM fsm)
    {
        if(fsm.stateValue < fsm.patrolThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    public override void UpdateTargetPosition(Vector3 currentPos, out Vector3 targetPos)
    { 
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        targetPos = patrolPoints[patrolIndex];
    }

    public override List<Node> UpdatePath(Vector3 currentPosition, Vector3 targetPosition)
    {
        return FindPath(currentPosition, targetPosition);
    }

    public override void HandleEvent(AI ai, string arrivalType)
    {
        if (arrivalType == "TargetPosition")
        {
            UpdateTargetPosition(ai.transform.position, out ai.targetPosition);
            ai.currentPath = UpdatePath(ai.transform.position, ai.targetPosition);
        }
        else if (arrivalType == "PathNode")
        {
            ai.currentPath = UpdatePath(ai.transform.position, ai.targetPosition);
        }
    }

    public List<Vector3> GenerateRandomPatrolPoints(int Count)
    {
        List<Vector3> points = new List<Vector3>();

        while (points.Count < Count)
        {
            Vector3 point = grid.GetRandomPoint();

            if (IsValidPatrolPoint(point, 1f))
            {
                points.Add(point);
            }
        }
        return points;
    }



    void OnDrawGizmos()
    {
        if (patrolPoints != null)
        {
            Gizmos.color = Color;
            foreach (var point in patrolPoints)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }
    }
}
