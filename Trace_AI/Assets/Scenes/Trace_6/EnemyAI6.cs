using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI6 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리

    private Vector3 wanderDirection; // 랜덤 돌아다니기 방향
    private float nextDirectionChangeTime; // 다음 방향 변경 시간
    private Renderer erenderer; // Renderer 컴포넌트 참조

    public Pathfinding6 pathfinding6; // Pathfinding6 스크립트 참조
    private List<Node6> currentPath; // 현재 경로 저장
    private int targetIndex; // 현재 타겟 노드 인덱스

    public List<Vector3> patrolPoints; // 순찰 지점 리스트
    private int patrolIndex; // 현재 순찰 지점 인덱스

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        erenderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기
        pathfinding6 = FindObjectOfType<Pathfinding6>(); // Pathfinding6 인스턴스 찾기
        patrolIndex = 0; // 초기 순찰 지점 인덱스 설정
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
        else
        {
            Patrol();
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < trackingDistance)
            {
                currentPath = pathfinding6.FindPath(transform.position, player.position); // 경로 찾기
                erenderer.material.color = Color.red; // 플레이어가 범위 내에 있으면 빨간색
                targetIndex = 0;
            }
            else
            {
                currentPath = null; // 경로 초기화
                erenderer.material.color = Color.green; // 플레이어가 범위 밖에 있으면 초록색
            }
            yield return new WaitForSeconds(1f); // 1초마다 경로 갱신
        }
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        if (targetIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[targetIndex].worldPosition;
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                targetIndex++; // 현재 목표 지점에 도달하면 다음 지점으로 이동
            }
        }
    }

    void Patrol()
    {
        if (patrolPoints.Count == 0)
        {
            return;
        }

        Vector3 targetPosition = patrolPoints[patrolIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count; // 다음 순찰 지점으로 이동
        }
    }

    Vector3 GetRandomDirection()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            Destroy(player.gameObject); // 플레이어를 파괴
        }
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
