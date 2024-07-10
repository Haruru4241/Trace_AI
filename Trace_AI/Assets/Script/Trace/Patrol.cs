using System.Collections.Generic;
using UnityEngine;

public class Patrol : MoveBase
{
    public List<Vector3> patrolPoints;
    public Color Color=Color.green;
    private int patrolIndex;

    public override void Initialize()
    {
        layerPenalties = ai.layerPenalties;
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

    public override void Execute()
    {
        if(fsm.stateValue > fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        fsm.SetState<Trace>();
    }

    public override void UpdateTargetPosition(Vector3 currentPos, ref Vector3 targetPos)
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        targetPos = patrolPoints[patrolIndex];
    }

    public override void UpdatePath(Vector3 currentPosition, Vector3 targetPosition, ref List<Node> currentPath)
    {
        currentPath = FindPath(currentPosition, targetPosition);
    }
    public override void HandleEvent(ref Vector3 targetPosition, ref List<Node> currentPath, string arrivalType)
    {
        if (arrivalType == "TargetPosition")
        {
            UpdateTargetPosition(transform.position, ref targetPosition);
            UpdatePath(transform.position, targetPosition, ref currentPath);
        }
        else if (arrivalType == "PathNode")
        {
            UpdatePath(transform.position, targetPosition, ref currentPath);
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
