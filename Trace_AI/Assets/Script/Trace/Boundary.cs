/*using UnityEngine;

public class BoundaryState : MoveBase
{
    private Vector3 targetPosition;

    public override void Enter()
    {
        // 현재 위치에서 가장 가까운 플레이어의 마지막 위치로 이동
        targetPosition = TraceTargetPosition();
    }

    public override void Execute()
    {
        // 목표 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * ai.movementSpeed);

        // 목표 위치에 도착하면 다음 행동을 결정
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = ArriveTargetPosition();
        }
    }

    public override void Exit()
    {
        // 상태 종료 시 필요한 작업
    }

    public override Vector3 ArriveTargetPosition()
    {
        // 목표 위치에 도착했을 때 새로운 목표 위치 설정
        int closestIndex = FindClosestPoint(transform.position, ai.possiblePositions);
        return ai.possiblePositions[closestIndex];
    }

    public override Vector3 TraceTargetPosition()
    {
        // 플레이어의 마지막 위치를 목표 위치로 설정
        return ai.lastKnownPosition;
    }
}*/