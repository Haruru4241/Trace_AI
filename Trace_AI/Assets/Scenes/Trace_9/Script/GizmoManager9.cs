using UnityEngine;
using System.Collections.Generic;

public class GizmoManager9 : MonoBehaviour
{
    public GameManager gameManager;
    public Pathfinding9 pathfinding9;
    public Transform player;
    public Transform aiObject;
    public List<Node9> currentPath;

    void Awake()
    {
        if (gameManager != null)
        {
            pathfinding9 = pathfinding9 ?? gameManager.pathfinding9;
            player = player ?? gameManager.player;
            aiObject = aiObject ?? gameManager.aiObject;
        }
    }


    void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

        foreach (Node9 n in currentPath)
        {
            Gizmos.color = Color.black;
            Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
            Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding9.grid9.nodeDiameter - .1f));
            Gizmos.DrawLine(previousPosition, gizmoPosition);
            previousPosition = gizmoPosition;
        }
    }
}
