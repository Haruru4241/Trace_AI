using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI7 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리
    public float stateThreshold = 100f; // 상태 전환 임계값 (public으로 설정)
    public float stateMaxValue = 200f; // 상태값의 최대값 (public으로 설정)

    public Pathfinding7 pathfinding7; // Pathfinding7 스크립트 참조
    private List<Node7> currentPath; // 현재 경로 저장
    private int targetIndex; // 현재 타겟 노드 인덱스

    public List<Vector3> patrolPoints; // 순찰 지점 리스트
    private int patrolIndex; // 현재 순찰 지점 인덱스

    private enum State { Patrolling, Tracking, Attacking }
    private State currentState;
    private State previousState;

    private Dictionary<string, float> stateValues; // 상태값 딕셔너리
    private Renderer renderer; // Renderer 컴포넌트 참조
    private Vector3 targetPosition; // 타겟 위치
    private GizmoManager7 gizmoManager; // GizmoManager7 참조

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        renderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기
        pathfinding7 = FindObjectOfType<Pathfinding7>(); // Pathfinding7 인스턴스 찾기
        gizmoManager = FindObjectOfType<GizmoManager7>(); // GizmoManager7 인스턴스 찾기
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
        currentPath = new List<Node7>(); // currentPath 초기화
        foreach (var patrolPoint in patrolPoints)
        {
            currentPath.Add(pathfinding7.GetNodeFromPosition(patrolPoint));
        }
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // 상태값을 업데이트하며 기본적으로 상태값이 일정 비율로 감소
        UpdateStateValues("distance", -1f);
        UpdateStateValues("noise", -1f);
        UpdateStateValues("visibility", -1f);

        // 플레이어가 일정 범위 내에 있을 경우 상태값 크게 상승
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < trackingDistance)
        {
            UpdateStateValues("visibility", (trackingDistance - distance) * 1.2f); // 감지 값을 1.2배로 가중치 적용
        }

        CheckStateTransition();
        UpdateColor();

        MoveToNode();
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
                    currentPath = pathfinding7.FindPath(transform.position, targetPosition); // 경로 찾기
                    targetIndex = 0; // targetIndex 초기화
                }
                else if (currentState == State.Patrolling)
                {
                    patrolIndex = GetClosestPatrolPointIndex(patrolPoints); // 가장 가까운 순찰 지점 인덱스 설정
                    targetPosition = patrolPoints[patrolIndex]; // 타겟 위치 설정
                    currentPath = new List<Node7>(); // currentPath 초기화
                    for (int i = 0; i < patrolPoints.Count; i++)
                    {
                        int nextIndex = (patrolIndex + i) % patrolPoints.Count;
                        currentPath.AddRange(pathfinding7.FindPath(patrolPoints[nextIndex], patrolPoints[(nextIndex + 1) % patrolPoints.Count]));
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
                        currentPath = pathfinding7.FindPath(transform.position, player.position); // 경로 갱신
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

    void MoveToNode()
    {
        if (currentPath == null || targetIndex >= currentPath.Count)
        {
            return;
        }

        Vector3 nodePosition = currentPath[targetIndex].worldPosition;
        Vector3 moveDirection = (nodePosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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
            currentPath = pathfinding7.FindPath(transform.position, targetPosition); // 새로운 경로 설정
            targetIndex = 0; // targetIndex 초기화
        }
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
        if (stateValues.ContainsKey(key))
        {
            stateValues[key] += value;
            stateValues[key] = Mathf.Clamp(stateValues[key], 0, stateMaxValue); // 상태값을 0과 stateMaxValue 사이로 제한
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
        renderer.material.color = Color.Lerp(Color.green, Color.red, t);
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
}
