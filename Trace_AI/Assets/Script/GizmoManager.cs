using UnityEngine;
using System.Collections.Generic;

public class GizmoManager : MonoBehaviour
{
    public Pathfinding pathfinding;
    public Transform player;
    public Transform aiObject;

    void OnDrawGizmos()
    {
        // �ʿ��� ��� ��ü�� ��ȿ���� Ȯ��
        if (pathfinding == null || pathfinding.grid == null || player == null || aiObject == null)
        {
            return;
        }

        List<Node> path = pathfinding.FindPath(aiObject.position, player.position);

        if (path != null)
        {
            // AI ��ü�� ���� ��ġ�� ���� ��ġ�� ����
            Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

            foreach (Node n in path)
            {
                Gizmos.color = Color.black;
                Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding.grid.nodeDiameter - .1f));

                // ���� ��ġ���� ���� ��ġ�� ���� �׸��ϴ�.
                Gizmos.DrawLine(previousPosition, gizmoPosition);
                previousPosition = gizmoPosition;
            }
        }
    }
}
