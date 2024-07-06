using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI5 : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private Renderer erenderer; // Renderer ������Ʈ ����

    public Pathfinding5 pathfinding5; // Pathfinding5 ��ũ��Ʈ ����
    private List<Node5> currentPath; // ���� ��� ����
    private int targetIndex; // ���� Ÿ�� ��� �ε���

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
        erenderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������
        pathfinding5 = FindObjectOfType<Pathfinding5>(); // Pathfinding5 �ν��Ͻ� ã��
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
            WanderAround();
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < trackingDistance)
            {
                currentPath = pathfinding5.FindPath(transform.position, player.position); // ��� ã��
                erenderer.material.color = Color.red; // �÷��̾ ���� ���� ������ ������
                targetIndex = 0;
            }
            else
            {
                currentPath = null; // ��� �ʱ�ȭ
                erenderer.material.color = Color.green; // �÷��̾ ���� �ۿ� ������ �ʷϻ�
            }
            yield return new WaitForSeconds(1f); // 1�ʸ��� ��� ����
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
                targetIndex++; // ���� ��ǥ ������ �����ϸ� ���� �������� �̵�
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
}
