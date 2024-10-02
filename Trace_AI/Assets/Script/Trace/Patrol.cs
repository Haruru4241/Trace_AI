using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : MoveBase
{
    [Tooltip("2�� �̻��� ��ġ�� �����Ͽ� �ش� ��ġ�� ����")]
    public List<Vector3> patrolPoints;

    [Tooltip("���� ���� �� ������ ��ġ ����")]
    public int RandomPoints = 4;

    [Tooltip("����� ����")]
    public Color Color = Color.cyan;

    private int patrolIndex;


    public override void Initialize()
    {
        base.Initialize();
        if (patrolPoints == null || patrolPoints.Count <= 1)
        {
            //���� ��θ� �������� �ʾ��� �� �������� ��� ����
            patrolPoints = GetRandomNavMeshPosition();
        }

        patrolIndex = FindClosestPoint(transform.position, patrolPoints);

    }

    public override void Enter()
    {
        patrolIndex = FindClosestPoint(transform.position, patrolPoints);
        ai.SetTargetPosition(patrolPoints[patrolIndex]);
        Debug.Log($"{transform.name} Patrol, Enter: {patrolPoints[patrolIndex]}");
    }

    public override void Execute()
    {
        if (!agent.pathPending && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            // remainingDistance가 너무 작지 않고, 에이전트가 경로를 따라 움직이는 상태일 때만 실행
            if (agent.remainingDistance - agent.stoppingDistance < 0.1f)
            {
                ArriveTargetPosition();
            }
        }

        foreach (var rule in fsm.FindStatetargetState(this))
        {
            if (rule.ExitCondition.ExitCondition())
            {
                Exit(rule.escapeState);
                break; // 조건이 만족되면 반복 종료
            }
        }
    }

    public override void Exit(MoveBase newState)
    {
        Debug.Log($"{transform.name} Patrol Exit");
        fsm.SetState(newState);
    }

    public override void ArriveTargetPosition()
    {
        Debug.Log($"{transform.name} Patrol Arrive: {patrolPoints[patrolIndex]}");
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
        var mapBlocksList = gameManager.mapMaker.mapBlocksList;

        int count = 10000;

        for (int i = 0; i < RandomPoints; i++)
        {
            count -= 1;
            if (count == 0) return null;
            Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(0, mapBlocksList.GetLength(0)),
            0,
            UnityEngine.Random.Range(0, mapBlocksList.GetLength(1))
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
