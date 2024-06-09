using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI1 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리

    private Vector3 wanderDirection; // 랜덤 돌아다니기 방향
    private float directionChangeInterval = 3.0f; // 방향 변경 간격
    private float nextDirectionChangeTime; // 다음 방향 변경 시간
    private Renderer renderer; // Renderer 컴포넌트 참조

    public Pathfinding pathfinding; // Pathfinding 스크립트 참조
    private List<Node> currentPath; // 현재 경로 저장
    private GizmoManager gizmoManager; // GizmoManager 참조

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // 초기 랜덤 방향 설정
        renderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기
        gizmoManager = FindObjectOfType<GizmoManager>(); // GizmoManager 찾기
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < trackingDistance)
            {
                currentPath = pathfinding.FindPath(transform.position, player.position); // 경로 찾기
                renderer.material.color = Color.red; // 플레이어가 범위 내에 있으면 빨간색
                if (gizmoManager != null)
                {
                    gizmoManager.pathfinding = pathfinding;
                    gizmoManager.player = player;
                    gizmoManager.aiObject = transform;
                }
            }
            else
            {
                currentPath = null; // 경로 초기화
                WanderAround();
                renderer.material.color = Color.green; // 플레이어가 범위 밖에 있으면 초록색
            }
            yield return new WaitForSeconds(1f); // 1초마다 경로 갱신
        }
    }

    void FollowPlayer()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        Vector3 targetPosition = currentPath[0].worldPosition;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPath.RemoveAt(0); // 현재 목표 지점에 도달하면 다음 지점으로 이동
        }
    }

    void WanderAround()
    {
        if (Time.time >= nextDirectionChangeTime)
        {
            wanderDirection = GetRandomDirection();
            nextDirectionChangeTime = Time.time + directionChangeInterval;
        }
        transform.position += wanderDirection * moveSpeed * Time.deltaTime;
    }

    Vector3 GetRandomDirection()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = originalMoveSpeed;
    }
}
