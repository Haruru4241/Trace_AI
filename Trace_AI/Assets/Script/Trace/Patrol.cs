using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : MoveBase
{
    public List<Vector3> patrolPoints;
    public Color Color=Color.green;
    private int patrolIndex;

    public int RandomPoints = 4;
    public float range = 200.0f;

    void Start()
    {
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            patrolPoints = GenerateRandomPatrolPoints();
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
        if(ai.targetList.Any() && ai.targetList.First().Value > fsm.chaseThreshold)
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

    public List<Vector3> GenerateRandomPatrolPoints()
    {
        List<Vector3> patrolPoints = new List<Vector3>();

        for (int i = 0; i < RandomPoints; i++)
        {
            Vector3 randomPoint = Random.insideUnitSphere * range;
            randomPoint += transform.position; // Offset by the position of the object

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
            {
                patrolPoints.Add(hit.position);
            }
            else
            {
                // If the point is not on the NavMesh, decrement i to try again
                i--;
            }
        }

        return patrolPoints;
    }

    private Vector3 GetRandomPointWithinRange()
    {
        float x = Random.Range(-range, range);
        float z = Random.Range(-range, range);
        return new Vector3(x, 0, z);
    }

    void OnDrawGizmos()
    {
        if (patrolPoints != null)
        {
            Gizmos.color = Color;
            foreach (var point in patrolPoints)
            {
                Gizmos.DrawSphere(point, 2f);
            }
        }
    }
}
