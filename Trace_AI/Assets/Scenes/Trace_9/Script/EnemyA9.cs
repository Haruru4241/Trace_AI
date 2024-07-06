using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI9 : MonoBehaviour
{
    public GameManager gameManager;
    public Transform player;
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f;
    public float stateThreshold = 100f;
    public float stateMaxValue = 200f;
    public float viewAngle = 60f;
    public float viewDistance = 15f;
    public float soundDetectionRadius = 10f;

    public Pathfinding9 pathfinding9;
    private List<Node9> currentPath;
    private int targetIndex;

    public List<Vector3> patrolPoints;
    private int patrolIndex;

    private enum State { Patrolling, Tracking, Attacking }
    private State currentState;
    private State previousState;

    private Dictionary<string, float> stateValues;
    private Renderer erenderer;
    private Vector3 targetPosition;
    private GizmoManager9 gizmoManager;

    private Vector3 lastPlayerPosition;
    private bool isPlayerMoving;

    void Awake()
    {
        if (gameManager != null)
        {
            pathfinding9 = pathfinding9 ?? gameManager.pathfinding9;
            player = player ?? gameManager.player;
        }
    }


    void Start()
    {
        originalMoveSpeed = moveSpeed;
        erenderer = GetComponent<Renderer>();
        gizmoManager = FindObjectOfType<GizmoManager9>();
        patrolIndex = 0;
        stateValues = new Dictionary<string, float>
        {
            { "distance", 0f },
            { "noise", 0f },
            { "visibility", 0f }
        };
        currentState = State.Patrolling;
        previousState = currentState;
        patrolIndex = GetClosestPatrolPointIndex(patrolPoints);
        targetPosition = patrolPoints[patrolIndex];
        currentPath = new List<Node9>();
        foreach (var patrolPoint in patrolPoints)
        {
            currentPath.Add(pathfinding9.GetNodeFromPosition(patrolPoint));
        }
        lastPlayerPosition = player.position;
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        UpdateStateValues("distance", -1f);
        UpdateStateValues("noise", -1f);
        UpdateStateValues("visibility", -1f);

        DetectPlayerInView();
        DetectPlayerBySound();
        CheckStateTransition();
        UpdateColor();
        MoveToNode();
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
                    targetPosition = player.position;
                    currentPath = pathfinding9.FindPath(transform.position, targetPosition);
                    targetIndex = 0;
                }
                else if (currentState == State.Patrolling)
                {
                    patrolIndex = GetClosestPatrolPointIndex(patrolPoints);
                    targetPosition = patrolPoints[patrolIndex];
                    currentPath = new List<Node9>();
                    for (int i = 0; i < patrolPoints.Count; i++)
                    {
                        int nextIndex = (patrolIndex + i) % patrolPoints.Count;
                        currentPath.AddRange(pathfinding9.FindPath(patrolPoints[nextIndex], patrolPoints[(nextIndex + 1) % patrolPoints.Count]));
                    }
                    targetIndex = 0;
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
                            targetIndex = 0;
                        }
                    }
                    else if (currentState == State.Tracking)
                    {
                        currentPath = pathfinding9.FindPath(transform.position, player.position);
                        targetIndex = 0;
                    }
                }
            }

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
                        UpdateStateValues("visibility", 10f);
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
                UpdateStateValues("noise", 3f);
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

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
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
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
            targetPosition = patrolPoints[patrolIndex];
            currentPath = pathfinding9.FindPath(transform.position, targetPosition);
            targetIndex = 0;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            Destroy(player.gameObject);
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

    void UpdateStateValues(string key, float value)
    {
        if (stateValues != null && stateValues.ContainsKey(key))
        {
            stateValues[key] += value;
            stateValues[key] = Mathf.Clamp(stateValues[key], 0, stateMaxValue);
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
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        for (int i = 0; i <= 50; i++)
        {
            float angle = -viewAngle / 2 + viewAngle * i / 50;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward * viewDistance;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundDetectionRadius);
    }
}
