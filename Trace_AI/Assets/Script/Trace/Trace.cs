using System.Collections.Generic;
using UnityEngine;

public class Trace : MoveBase
{
    public override void Initialize(AI ai, Dictionary<int, int> layerMask)
    {
        layerPenalties = layerMask;
    }

    public override void Enter()
    {
        ai.HandleEvent("SetBehavior");
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

    public override void UpdateTargetPosition(Vector3 currentPos, ref Vector3 targetPos)
    {
        targetPos = player.position;
    }

    public override void UpdatePath(Vector3 currentPosition, Vector3 targetPosition, ref List<Node> currentPath)
    {
        currentPath = FindPath(currentPosition, targetPosition);
    }

    public override void HandleEvent(ref Vector3 targetPosition, ref List<Node> currentPath, string arrivalType)
    {
        if (arrivalType == "TargetPosition" || arrivalType == "PathNode")
        {
            UpdateTargetPosition(transform.position, ref targetPosition);
            UpdatePath(transform.position, targetPosition, ref currentPath);
        }
    }


}
