using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MoveBaseTemplate : MoveBase
{
    public override void Enter()
    {
        Debug.Log($"{transform.name} ... 상태 진입");
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();

        if (true)//조건
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{transform.name} ... 상태 탈출");
        //fsm.SetState<MoveBase>();
    }

    public override void ArriveTargetPosition()
    {
        Debug.Log($"{transform.name} 목표 완료");
    }

    public override Vector3 TraceTargetPosition()
    {
        return transform.position; // 제자리 위치 반환
    }
}
