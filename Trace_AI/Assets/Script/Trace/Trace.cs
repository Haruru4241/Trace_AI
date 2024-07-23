using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(TraceTargetPosition());
        Debug.Log($"{transform.name} 추적 상태 진입, 목표: {ai.targetList.First().Key.name}");
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();

        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{transform.name} 추적 상태 탈출");
        fsm.SetState<Boundary>();
    }

    public override void ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
            //공격 관련 클래스 추가 예정
            Debug.Log($"{transform.name} 목표 추적 완료: {ai.targetList.First().Key.name}");
        }
    }

    public override Vector3 TraceTargetPosition()
    {
        if (ai.targetList.Any())
        {
            return ai.targetList.First().Key.position;
        }
        return transform.position; // 제자리 위치 반환
    }
}
