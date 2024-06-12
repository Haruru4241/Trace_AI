using UnityEngine;
using System.Collections.Generic;

public class GizmoManager4 : MonoBehaviour
{
    public Pathfinding4 pathfinding4;
    public Transform player;
    public Transform aiObject;

    void OnDrawGizmos()
    {
        // �ʿ��� ��� ��ü�� ��ȿ���� Ȯ��
        if (pathfinding4 == null || pathfinding4.grid4 == null || player == null || aiObject == null)
        {
            return;
        }

        List<Node4> path = pathfinding4.FindPath(aiObject.position, player.position);

        if (path != null)
        {
            // AI ��ü�� ���� ��ġ�� ���� ��ġ�� ����
            Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

            foreach (Node4 n in path)
            {
                Gizmos.color = Color.black;
                Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding4.grid4.GetNodeDiameter() - .1f));

                // ���� ��ġ���� ���� ��ġ�� ���� �׸��ϴ�.
                Gizmos.DrawLine(previousPosition, gizmoPosition);
                previousPosition = gizmoPosition;
            }
        }
    }
}
