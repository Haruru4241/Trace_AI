using System.Collections.Generic;
using UnityEngine;

public class Patrol : MoveBase
{
    public List<Vector3> patrolPoints;
    public int patrolIndex;

    public override void UpdateTargetPosition(Vector3 currentPos, out Vector3 targetPos)
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        targetPos = patrolPoints[patrolIndex];
    }

    public override List<Node> UpdatePath(Vector3 currentPosition, Vector3 targetPosition)
    {
        return FindPath(currentPosition, targetPosition);
    }

    public override void HandleEvent(AI ai, string arrivalType)
    {
        if (arrivalType == "TargetPosition")
        {
            UpdateTargetPosition(ai.transform.position, out ai.targetPosition);
            ai.currentPath = UpdatePath(ai.transform.position, ai.targetPosition);
        }
        else if (arrivalType == "PathNode")
        {
            ai.currentPath = UpdatePath(ai.transform.position, ai.targetPosition);
        }
    }

    public List<Vector3> GenerateRandomPatrolPoints()
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            Vector3 point;
            bool validPoint;
            do
            {
                validPoint = true;
                point = grid.GetRandomPoint();

                foreach (var p in points)
                {
                    if (FindPath(p, point) == null)
                    {
                        validPoint = false;
                        break;
                    }
                }

                if (points.Count > 0 && FindPath(transform.position, point) == null)
                {
                    validPoint = false;
                }
            }
            while (!validPoint);

            points.Add(point);
        }
        return points;
    }
}
