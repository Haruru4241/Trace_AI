using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI8 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리
    public float stateThreshold = 100f; // 상태 전환 임계값 (public으로 설정)
    public float stateMaxValue = 200f; // 상태값의 최대값 (public으로 설정)
    public float viewAngle = 45f; // 시야 각도
    public float viewDistance = 10f; // 시야 거리
    public float soundDetectionRadius = 5f; // 소리 감지 반경

    public Pathfinding8 pathfinding8; // Pathfinding8 스크립트 참조
    private List<Node8> currentPath; // 현재 경로 저장
    private int targetIndex; // 현재 타겟 노드 인덱스

    public List<Vector3> patrolPoints; // 순찰 지점 리스트
    private int patrolIndex; // 현재 순찰 지점 인덱스

    private enum State { Patrolling, Tracking, Attacking }
    private State currentState;
    private State previousState;

    private Dictionary<string, float> stateValues; // 상태값 딕셔너리
    private Renderer erenderer; // Renderer 컴포넌트 참조
    private Vector3 targetPosition; // 타겟 위치
    private GizmoManager8 gizmoManager; // GizmoManager8 참조

    private Vector3 lastPlayerPosition; // 이전 프레임의 플레이어 위치
    private bool isPlayerMoving; // 플레이어 움직임 상태

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        erenderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기
        pathfinding8 = FindObjectOfType<Pathfinding8>(); // Pathfinding8 인스턴스 찾기
        gizmoManager = FindObjectOfType<GizmoManager8>(); // GizmoManager8 인스턴스 찾기
        patrolIndex = 0; // 초기 순찰 지점 인덱스 설정
        stateValues = new Dictionary<string, float>
        {
            { "distance", 0f },
            { "noise", 0f },
            { "visibility", 0f }
        };
        currentState = State.Patrolling; // 초기 상태
        previousState = currentState;
        patrolIndex = GetClosestPatrolPointIndex(patrolPoints); // 초기 타겟 위치 설정
        targetPosition = patrolPoints[patrolIndex]; // 초기 타겟 위치 설정
        currentPath = new List<Node8>(); // currentPath 초기화
        foreach (var patrolPoint in patrolPoints)
        {
            currentPath.Add(pathfinding8.GetNodeFromPosition(patrolPoint));
        }
        lastPlayerPosition = player.position; // 초기 플레이어 위치 설정
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // 상태값을 업데이트하며 기본적으로 상태값이 일정 비율로 감소
        UpdateStateValues("distance", -1f);
        UpdateStateValues("noise", -1f);
        UpdateStateValues("visibility", -1f);

        // 시야 감지
        DetectPlayerInView();

        // 소리 감지
        DetectPlayerBySound();

        CheckStateTransition();
        UpdateColor();

        MoveToNode();

        // 플레이어의 움직임 감지
        DetectPlayerMovement();
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            if (currentState != previousState)
            {
                if (currentState == State.Tracking)
                {
                    targetPosition = player.position; // 타겟 위치 설정
                    currentPath = pathfinding8.FindPath(transform.position, targetPosition); // 경로 찾기
                    targetIndex = 0; // targetIndex 초기화
                }
                else if (currentState == State.Patrolling)
                {
                    patrolIndex = GetClosestPatrolPointIndex(patrolPoints); // 가장 가까운 순찰 지점 인덱스 설정
                    targetPosition = patrolPoints[patrolIndex]; // 타겟 위치 설정
                    currentPath = new List<Node8>(); // currentPath 초기화
                    for (int i = 0; i < patrolPoints.Count; i++)
                    {
                        int nextIndex = (patrolIndex + i) % patrolPoints.Count;
                        currentPath.AddRange(pathfinding8.FindPath(patrolPoints[nextIndex], patrolPoints[(nextIndex + 1) % patrolPoints.Count]));
                    }
                    targetIndex = 0; // targetIndex 초기화
                }
                previousState = currentState;
            }

            if (currentPath != null)
            {
                Vector3 nodePosition = currentPath[targetIndex].worldPosition;
                Vector3 aiPosition = new Vector3(transform.position.x, 0, transform.position.z);

                if (Vector3.Distance(aiPosition, nodePosition) < 0.1f)
                {
                    targetIndex++;
                    if (currentState == State.Patrolling)
                    {
                        if (targetIndex >= currentPath.Count)
                        {
                            targetIndex = 0; // 순찰 경로의 처음으로 돌아감
                        }
                    }
                    else if (currentState == State.Tracking)
                    {
                        currentPath = pathfinding8.FindPath(transform.position, player.position); // 경로 갱신
                        targetIndex = 0; // targetIndex 초기화
                    }
                }
            }

            // GizmoManager에 현재 경로를 업데이트
            if (gizmoManager != null)
            {
                gizmoManager.currentPath = currentPath;
                gizmoManager.aiObject = transform;
            }

            yield return null;
        }
    }

    void DetectPlayerInView()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance);
        foreach (var hit in hits)
        {
            if (hit.transform == player)
            {
                Vector3 directionToPlayer = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;
                float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

                if (angleBetween < viewAngle / 2)
                {
                    if (Physics.Linecast(transform.position, player.position, out RaycastHit hitInfo))
                    {
                        UpdateStateValues("visibility", 10f); // 감지값 증가
                    }
                }
            }
        }
    }

    void DetectPlayerBySound()
    {
        if (isPlayerMoving)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= soundDetectionRadius)
            {
                UpdateStateValues("noise", 3f); // 감지값 증가
            }
        }
    }

    void DetectPlayerMovement()
    {
        if (player.position != lastPlayerPosition)
        {
            isPlayerMoving = true;
            lastPlayerPosition = player.position;
        }
        else
        {
            isPlayerMoving = false;
        }
    }

    void MoveToNode()
    {
        if (currentPath == null || targetIndex >= currentPath.Count)
        {
            return;
        }

        Vector3 nodePosition = currentPath[targetIndex].worldPosition;
        Vector3 moveDirection = (nodePosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 회전값 변경
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0); // Z축 회전을 0으로 고정
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void Patrol()
    {
        if (patrolPoints.Count == 0)
        {
            return;
        }

        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.1f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count; // 다음 순찰 지점으로 이동
            targetPosition = patrolPoints[patrolIndex]; // 새로운 타겟 위치 설정
            currentPath = pathfinding8.FindPath(transform.position, targetPosition); // 새로운 경로 설정
            targetIndex = 0; // targetIndex 초기화
        }

        // 회전값 변경
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0); // Z축 회전을 0으로 고정
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
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

    // 상태값을 업데이트하는 메서드 (인자로 상태값의 이름과 변경값을 받음)
    void UpdateStateValues(string key, float value)
    {
        if (stateValues != null && stateValues.ContainsKey(key))
        {
            stateValues[key] += value;
            stateValues[key] = Mathf.Clamp(stateValues[key], 0, stateMaxValue); // 상태값을 0과 stateMaxValue 사이로 제한
        }
        else
        {
            Debug.LogWarning($"State key '{key}' not found or stateValues is null.");
        }
    }

    void CheckStateTransition()
    {
        float stateValueSum = 0f;
        foreach (var value in stateValues.Values)
        {
            stateValueSum += value;
        }

        if (stateValueSum >= stateThreshold && currentState != State.Tracking)
        {
            currentState = State.Tracking;
        }
        else if (stateValueSum < stateThreshold && currentState != State.Patrolling)
        {
            currentState = State.Patrolling;
        }
    }

    void UpdateColor()
    {
        float stateValueSum = 0f;
        foreach (var value in stateValues.Values)
        {
            stateValueSum += value;
        }

        float t = Mathf.InverseLerp(0, stateThreshold, stateValueSum);
        erenderer.material.color = Color.Lerp(Color.green, Color.red, t);
    }

    int GetClosestPatrolPointIndex(List<Vector3> points)
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, points[i]);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }
        return closestIndex;
    }

    void OnDrawGizmos()
    {
        // 시야 감지 범위를 부채꼴로 그리기
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // 부채꼴 영역 그리기
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        for (int i = 0; i <= 50; i++)
        {
            float angle = -viewAngle / 2 + viewAngle * i / 50;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward * viewDistance;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }

        // 소리 감지 반경 그리기
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundDetectionRadius);
    }
}
