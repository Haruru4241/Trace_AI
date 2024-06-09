using UnityEngine;
using System.Collections.Generic;

public class GizmoManager : MonoBehaviour
{
    public Pathfinding pathfinding;
    public Transform player;
    public Transform aiObject;

    void OnDrawGizmos()
    {
        // 필요한 모든 객체가 유효한지 확인
        if (pathfinding == null || pathfinding.grid == null || player == null || aiObject == null)
        {
            return;
        }

        List<Node> path = pathfinding.FindPath(aiObject.position, player.position);

        if (path != null)
        {
            // AI 객체의 현재 위치를 시작 위치로 설정
            Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

            foreach (Node n in path)
            {
                Gizmos.color = Color.black;
                Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding.grid.nodeDiameter - .1f));

                // 이전 위치에서 현재 위치로 선을 그립니다.
                Gizmos.DrawLine(previousPosition, gizmoPosition);
                previousPosition = gizmoPosition;
            }
        }
    }
}
