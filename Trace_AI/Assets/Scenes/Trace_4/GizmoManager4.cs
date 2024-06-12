using UnityEngine;
using System.Collections.Generic;

public class GizmoManager4 : MonoBehaviour
{
    public Pathfinding4 pathfinding4;
    public Transform player;
    public Transform aiObject;

    void OnDrawGizmos()
    {
        // 필요한 모든 객체가 유효한지 확인
        if (pathfinding4 == null || pathfinding4.grid4 == null || player == null || aiObject == null)
        {
            return;
        }

        List<Node4> path = pathfinding4.FindPath(aiObject.position, player.position);

        if (path != null)
        {
            // AI 객체의 현재 위치를 시작 위치로 설정
            Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

            foreach (Node4 n in path)
            {
                Gizmos.color = Color.black;
                Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding4.grid4.GetNodeDiameter() - .1f));

                // 이전 위치에서 현재 위치로 선을 그립니다.
                Gizmos.DrawLine(previousPosition, gizmoPosition);
                previousPosition = gizmoPosition;
            }
        }
    }
}
