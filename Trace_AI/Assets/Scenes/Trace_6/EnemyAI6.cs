using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI6 : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private Renderer erenderer; // Renderer ������Ʈ ����

    public Pathfinding6 pathfinding6; // Pathfinding6 ��ũ��Ʈ ����
    private List<Node6> currentPath; // ���� ��� ����
    private int targetIndex; // ���� Ÿ�� ��� �ε���

    public List<Vector3> patrolPoints; // ���� ���� ����Ʈ
    private int patrolIndex; // ���� ���� ���� �ε���

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        erenderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������
        pathfinding6 = FindObjectOfType<Pathfinding6>(); // Pathfinding6 �ν��Ͻ� ã��
        patrolIndex = 0; // �ʱ� ���� ���� �ε��� ����
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
                currentPath = pathfinding6.FindPath(transform.position, player.position); // ��� ã��
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
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count; // ���� ���� �������� �̵�
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
            Destroy(player.gameObject); // �÷��̾ �ı�
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
