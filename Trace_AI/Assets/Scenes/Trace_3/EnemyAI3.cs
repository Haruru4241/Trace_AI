using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI3 : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private Renderer erenderer; // Renderer ������Ʈ ����

    public Pathfinding3 pathfinding3; // Pathfinding3 ��ũ��Ʈ ����
    private List<Node3> currentPath; // ���� ��� ����

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
        erenderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPlayer();
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < trackingDistance)
            {
                pathfinding3.FindPath(transform.position, player.position); // ��� ã��
                erenderer.material.color = Color.red; // �÷��̾ ���� ���� ������ ������
                currentPath = pathfinding3.path;
            }
            else
            {
                currentPath = null; // ��� �ʱ�ȭ
                WanderAround();
                erenderer.material.color = Color.green; // �÷��̾ ���� �ۿ� ������ �ʷϻ�
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
