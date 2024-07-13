using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(ArriveTargetPosition());
    }

    public override void Execute()
    {
        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.patrolThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        fsm.SetState<Patrol>();
    }

    public override Vector3 ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
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
