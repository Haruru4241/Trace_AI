using UnityEngine;
using System.Collections.Generic;

public class EnemyAI1 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리
    public float repathInterval = 1.0f; // 경로를 다시 계산할 간격
    public float targetReachedThreshold = 0.5f; // 목표 지점에 도달했는지 확인하는 거리

    private Vector3 lastPlayerPosition;
    private Vector3 wanderDirection; // 랜덤 돌아다니기 방향
    private Vector3 moveDirection; // 이동할 방향
    private float directionChangeInterval = 3.0f; // 방향 변경 간격
    private float nextDirectionChangeTime; // 다음 방향 변경 시간
    private float nextRepathTime; // 다음 경로 재설정 시간
    private Renderer renderer; // Renderer 컴포넌트 참조

    private Grid grid;
    private Pathfinding pathfinding;
    private List<Node> path;
    private int currentPathIndex;

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // 초기 랜덤 방향 설정
        renderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기

        grid = FindObjectOfType<Grid>(); // Grid 컴포넌트 찾기
        pathfinding = new Pathfinding(); // Pathfinding 인스턴스 생성
        lastPlayerPosition = player.position;
        currentPathIndex = 0;
        nextRepathTime = Time.time + repathInterval;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < trackingDistance)
        {
            FollowPlayer();
            renderer.material.color = Color.red; // 플레이어가 범위 내에 있으면 빨간색
        }
        else
        {
            WanderAround();
            renderer.material.color = Color.green; // 플레이어가 범위 밖에 있으면 초록색
        }
    }

    void FollowPlayer()
    {
        if (Time.time >= nextRepathTime)
        {
            path = pathfinding.FindPath(transform.position, player.position, grid);
            path = SimplifyPath(path);
            lastPlayerPosition = player.position;
            currentPathIndex = 0;
            nextRepathTime = Time.time + repathInterval;
        }

        if (path != null && currentPathIndex < path.Count)
        {
            Vector3 target = path[currentPathIndex].position;
            moveDirection = (target - transform.position).normalized;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target) < targetReachedThreshold)
            {
                currentPathIndex++;
            }
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

    void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i].position, Vector3.one * (grid.nodeRadius * 0.5f));

                if (i < path.Count - 1)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(path[i].position, path[i + 1].position);
                }
            }
        }
    }

    List<Node> SimplifyPath(List<Node> path)
    {
        if (path == null || path.Count < 2)
            return path;

        List<Node> simplifiedPath = new List<Node>();
        Vector3 oldDirection = Vector3.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 newDirection = (path[i].position - path[i - 1].position).normalized;
            if (newDirection != oldDirection)
            {
                simplifiedPath.Add(path[i - 1]);
            }
            oldDirection = newDirection;
        }

        simplifiedPath.Add(path[path.Count - 1]);
        return simplifiedPath;
    }
}
