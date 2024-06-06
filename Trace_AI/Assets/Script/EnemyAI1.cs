using UnityEngine;
using System.Collections.Generic;

public class EnemyAI1 : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�
    public float repathInterval = 1.0f; // ��θ� �ٽ� ����� ����
    public float targetReachedThreshold = 0.5f; // ��ǥ ������ �����ߴ��� Ȯ���ϴ� �Ÿ�

    private Vector3 lastPlayerPosition;
    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private Vector3 moveDirection; // �̵��� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private float nextRepathTime; // ���� ��� �缳�� �ð�
    private Renderer renderer; // Renderer ������Ʈ ����

    private Grid grid;
    private Pathfinding pathfinding;
    private List<Node> path;
    private int currentPathIndex;

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
        renderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������

        grid = FindObjectOfType<Grid>(); // Grid ������Ʈ ã��
        pathfinding = new Pathfinding(); // Pathfinding �ν��Ͻ� ����
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
            renderer.material.color = Color.red; // �÷��̾ ���� ���� ������ ������
        }
        else
        {
            WanderAround();
            renderer.material.color = Color.green; // �÷��̾ ���� �ۿ� ������ �ʷϻ�
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
