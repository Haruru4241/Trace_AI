using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : MoveBase
{
    [Tooltip("2개 이상의 위치를 설정하여 해당 위치를 순찰")]
    public List<Vector3> patrolPoints;
  
    [Tooltip("랜덤 설정 시 생성할 위치 갯수")]
    public int RandomPoints = 4;

    [Tooltip("기즈모 색상")]
    public Color Color = Color.cyan;

    private int patrolIndex;

    public override void Initialize()
    {
        base.Initialize();
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            //순찰 경로를 설정하지 않았을 시 랜덤으로 경로 설정
            patrolPoints = GetRandomNavMeshPosition();
        }

        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
    }

    public override void Enter()
    {
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
        ai.SetTargetPosition(patrolPoints[patrolIndex]);
        Debug.Log($"{transform.name} 순찰 상태 진입, 목표: {patrolPoints[patrolIndex]}");
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();

        if (ai.targetList.Any() && ai.targetList.First().Value > fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{transform.name} 순찰 상태 탈출");
        fsm.SetState<Trace>();
    }

    public override void ArriveTargetPosition()
    {
        Debug.Log($"{transform.name} 순찰 목표 재설정: {patrolPoints[patrolIndex]}");
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
    }

    public override Vector3 TraceTargetPosition()
    {
        return patrolPoints[patrolIndex];
    }

    private List<Vector3> GetRandomNavMeshPosition()
    {
        List<Vector3> patrolPoints = new List<Vector3>();

        GameManager gameManager = FindObjectOfType<GameManager>();
        var mapBlocksList= gameManager.getMapBlocksList();

        int count = 10000;

        for (int i = 0; i < RandomPoints; i++)
        {
            count -= 1;
            if (count == 0) return null;
            Vector3 randomPosition = new Vector3(
            Random.Range(0, mapBlocksList.GetLength(0)),
            0,
            Random.Range(0, mapBlocksList.GetLength(1))
        );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                patrolPoints.Add((Vector3)hit.position);
            }
            else i -= 1; 
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
                Gizmos.DrawSphere(point, 0.5f);
            }
        }
    }
}
