using UnityEngine;
using System.Collections.Generic;

public class GizmoManager8 : MonoBehaviour
{
    public Pathfinding8 pathfinding8;
    public Transform player;
    public Transform aiObject;
    public List<Node8> currentPath; // AI�� ���� ��θ� �޾ƿ�

    void OnDrawGizmos()
    {
        // �ʿ��� ��� ��ü�� ��ȿ���� Ȯ��
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        // AI ��ü�� ���� ��ġ�� ���� ��ġ�� ����
        Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

        foreach (Node8 n in currentPath)
        {
            Gizmos.color = Color.black;
            Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
            Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding8.grid8.nodeDiameter - .1f));

            // ���� ��ġ���� ���� ��ġ�� ���� �׸��ϴ�.
            Gizmos.DrawLine(previousPosition, gizmoPosition);
            previousPosition = gizmoPosition;
        }
    }
}
