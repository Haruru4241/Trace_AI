using Map.Sample;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trace : MoveBase
{
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
