using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(ArriveTargetPosition());
        Debug.Log($"{transform.name} 추적 상태 진입, 목표: {ai.targetList.First().Key.name}");
    }

    public override void Execute()
    {
        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        fsm.SetState<Boundary>();
        Debug.Log($"{transform.name} 추적 상태 탈출");
    }

    public override Vector3 ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
            Debug.Log($"{transform.name} 추적 목표 재설정: {ai.targetList.First().Key.name}");
            return ai.targetList.First().Key.position;
        }
        
        return transform.position; // 제자리 위치 반환
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
