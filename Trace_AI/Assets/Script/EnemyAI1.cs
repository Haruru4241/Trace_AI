using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI1 : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private Renderer renderer; // Renderer ������Ʈ ����

    public Pathfinding pathfinding; // Pathfinding ��ũ��Ʈ ����
    private List<Node> currentPath; // ���� ��� ����
    private GizmoManager gizmoManager; // GizmoManager ����

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
        renderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������
        gizmoManager = FindObjectOfType<GizmoManager>(); // GizmoManager ã��
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < trackingDistance)
            {
                currentPath = pathfinding.FindPath(transform.position, player.position); // ��� ã��
                renderer.material.color = Color.red; // �÷��̾ ���� ���� ������ ������
                if (gizmoManager != null)
                {
                    gizmoManager.pathfinding = pathfinding;
                    gizmoManager.player = player;
                    gizmoManager.aiObject = transform;
                }
            }
            else
            {
                currentPath = null; // ��� �ʱ�ȭ
                WanderAround();
                renderer.material.color = Color.green; // �÷��̾ ���� �ۿ� ������ �ʷϻ�
            }
            yield return new WaitForSeconds(1f); // 1�ʸ��� ��� ����
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
            currentPath.RemoveAt(0); // ���� ��ǥ ������ �����ϸ� ���� �������� �̵�
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
