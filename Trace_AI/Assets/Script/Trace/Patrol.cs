using System.Collections.Generic;
using UnityEngine;

public class Patrol : MoveBase
{
    public List<Vector3> patrolPoints;
    public Color Color=Color.green;
    private int patrolIndex;

    void Start()
    {
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            patrolPoints = GenerateRandomPatrolPoints(4);
        }

        // ¼øÂû ÀÎµ¦½º ÃÊ±âÈ­
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
    }

    public override void Enter()
    {
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
        ai.SetTargetPosition(patrolPoints[patrolIndex]);
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

    public override Vector3 ArriveTargetPosition()
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        return patrolPoints[patrolIndex];
    }

    public override Vector3 TraceTargetPosition()
    {
        return patrolPoints[patrolIndex];
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
