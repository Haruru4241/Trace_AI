using UnityEngine;
using System.Collections.Generic;

public class GizmoManager6 : MonoBehaviour
{
    public Pathfinding6 pathfinding6;
    public Transform player;
    public Transform aiObject;

    void OnDrawGizmos()
    {
        // �ʿ��� ��� ��ü�� ��ȿ���� Ȯ��
        if (pathfinding6 == null || pathfinding6.grid6 == null || player == null || aiObject == null)
        {
            return;
        }

        List<Node6> path = pathfinding6.FindPath(aiObject.position, player.position);

        if (path != null)
        {
            // AI ��ü�� ���� ��ġ�� ���� ��ġ�� ����
            Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

            foreach (Node6 n in path)
            {
                Gizmos.color = Color.black;
                Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding6.grid6.nodeDiameter - .1f));

                // ���� ��ġ���� ���� ��ġ�� ���� �׸��ϴ�.
                Gizmos.DrawLine(previousPosition, gizmoPosition);
                previousPosition = gizmoPosition;
            }
        }
    }
}
