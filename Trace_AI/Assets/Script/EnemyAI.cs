using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float speed = 5.0f; // AI�� �̵� �ӵ�
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�
    private Renderer renderer; // Renderer ������Ʈ ����

    void Start()
    {
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
        renderer = GetComponent<Renderer>(); // Renderer ������Ʈ ��������
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
        Vector3 moveDirection = (player.position - transform.position).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void WanderAround()
    {
        if (Time.time >= nextDirectionChangeTime)
        {
            wanderDirection = GetRandomDirection();
            nextDirectionChangeTime = Time.time + directionChangeInterval;
        }
        transform.position += wanderDirection * speed * Time.deltaTime;
    }

    Vector3 GetRandomDirection()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    }
}


/*
ver 2
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float speed = 5.0f; // AI�� �̵� �ӵ�
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    private Vector3 wanderDirection; // ���� ���ƴٴϱ� ����
    private float directionChangeInterval = 3.0f; // ���� ���� ����
    private float nextDirectionChangeTime; // ���� ���� ���� �ð�

    void Start()
    {
        wanderDirection = GetRandomDirection(); // �ʱ� ���� ���� ����
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < trackingDistance)
        {
            FollowPlayer();
        }
        else
        {
            WanderAround();
        }
    }

    void FollowPlayer()
    {
        Vector3 moveDirection = (player.position - transform.position).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void WanderAround()
    {
        if (Time.time >= nextDirectionChangeTime)
        {
            wanderDirection = GetRandomDirection();
            nextDirectionChangeTime = Time.time + directionChangeInterval;
        }
        transform.position += wanderDirection * speed * Time.deltaTime;
    }

    Vector3 GetRandomDirection()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    }
}

/*


/*
 * ver 1
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform ����
    public float speed = 5.0f; // AI�� �̵� �ӵ�
    public float trackingDistance = 10.0f; // ���� ���� �Ÿ�

    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < trackingDistance)
        {
            Vector3 moveDirection = (player.position - transform.position).normalized;
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }
}
*/