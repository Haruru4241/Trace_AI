using UnityEngine;
using System.Collections.Generic;

public class GizmoManager7 : MonoBehaviour
{
    public Pathfinding7 pathfinding7;
    public Transform player;
    public Transform aiObject;
    public List<Node7> currentPath; // AI의 현재 경로를 받아옴

    void OnDrawGizmos()
    {
        // 필요한 모든 객체가 유효한지 확인
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        // AI 객체의 현재 위치를 시작 위치로 설정
        Vector3 previousPosition = new Vector3(aiObject.position.x, 1, aiObject.position.z);

        foreach (Node7 n in currentPath)
        {
            Gizmos.color = Color.black;
            Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
            Gizmos.DrawCube(gizmoPosition, Vector3.one * (pathfinding7.grid7.nodeDiameter - .1f));

            // 이전 위치에서 현재 위치로 선을 그립니다.
            Gizmos.DrawLine(previousPosition, gizmoPosition);
            previousPosition = gizmoPosition;
        }
    }
}
