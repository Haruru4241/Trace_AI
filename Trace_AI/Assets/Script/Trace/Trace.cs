using System.Collections.Generic;
using UnityEngine;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(player.position);
    }

    public override void Execute()
    {
        if(fsm.stateValue <= fsm.patrolThreshold)
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
        return player.position;
    }

    public override Vector3 TraceTargetPosition()
    {
        return player.position;
    }
}
