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

    public override void Initialize()
    {
        base.Initialize();
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            patrolPoints = GenerateRandomPatrolPoints();
        }

        // 순찰 인덱스 초기화
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
    }

    void Start()
    {

    }

    public override void Enter()
    {
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
        ai.SetTargetPosition(patrolPoints[patrolIndex]);
        Debug.Log($"{transform.name} 순찰 상태 진입, 목표: {patrolPoints[patrolIndex]}");
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
        Debug.Log($"{transform.name} 순찰 상태 탈출");
    }

    public override Vector3 ArriveTargetPosition()
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        Debug.Log($"{transform.name} 순찰 목표 재설정: {patrolPoints[patrolIndex]}");
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
