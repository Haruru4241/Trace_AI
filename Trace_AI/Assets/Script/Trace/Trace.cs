using System.Collections.Generic;
using UnityEngine;

public class Trace : MoveBase
{

    public override void Initialize(AI ai)
    {

    }

    public override void Enter()
    {
        ai.HandleEvent("SetBehavior");
    }

    public override void Execute(FSM fsm)
    {
        if(fsm.stateValue >= fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    public override void UpdateTargetPosition(Vector3 currentPos, out Vector3 targetPos)
    {
        targetPos = player.position;
    }

    public override List<Node> UpdatePath(Vector3 currentPosition, Vector3 targetPosition)
    {
        return FindPath(currentPosition, targetPosition);
    }

    public override void HandleEvent(AI ai, string arrivalType)
    {
        if (arrivalType == "TargetPosition" || arrivalType == "PathNode")
        {
            UpdateTargetPosition(ai.transform.position, out ai.targetPosition);
            ai.currentPath = UpdatePath(ai.transform.position, ai.targetPosition);
        }
    }
}
