using UnityEngine;
using System.Collections.Generic;

public class GizmoManager : MonoBehaviour
{
    public Pathfinding pathfinding;
    public Transform player;
    public Transform aiObject;
    public List<Node> currentPath;

    void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

        foreach (Node n in currentPath)
        {
            Gizmos.color = Color.black;
            Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
            Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding.grid.nodeDiameter - .1f));

            Gizmos.DrawLine(previousPosition, gizmoPosition);
            previousPosition = gizmoPosition;
        }
    }
}
