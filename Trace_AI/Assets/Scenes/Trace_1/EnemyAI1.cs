// EnemyAI1.cs
using UnityEngine;

public class EnemyAI1 : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float moveSpeed = 5f;
    private float originalMoveSpeed;
    public float trackingDistance = 10.0f; // 추적 시작 거리

    private Vector3 wanderDirection; // 랜덤 돌아다니기 방향
    private float directionChangeInterval = 3.0f; // 방향 변경 간격
    private float nextDirectionChangeTime; // 다음 방향 변경 시간
    private Renderer renderer; // Renderer 컴포넌트 참조

    void Start()
    {
        originalMoveSpeed = moveSpeed;
        wanderDirection = GetRandomDirection(); // 초기 랜덤 방향 설정
        renderer = GetComponent<Renderer>(); // Renderer 컴포넌트 가져오기
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < trackingDistance)
        {
            FollowPlayer();
            renderer.material.color = Color.red; // 플레이어가 범위 내에 있으면 빨간색
        }
        else
        {
            WanderAround();
            renderer.material.color = Color.green; // 플레이어가 범위 밖에 있으면 초록색
        }
    }

    void FollowPlayer()
    {
        Vector3 moveDirection = (player.position - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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



/*
ver 2
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform 참조
    public float speed = 5.0f; // AI의 이동 속도
    public float trackingDistance = 10.0f; // 추적 시작 거리

    private Vector3 wanderDirection; // 랜덤 돌아다니기 방향
    private float directionChangeInterval = 3.0f; // 방향 변경 간격
    private float nextDirectionChangeTime; // 다음 방향 변경 시간

    void Start()
    {
        wanderDirection = GetRandomDirection(); // 초기 랜덤 방향 설정
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
    public Transform player; // 플레이어의 Transform 참조
    public float speed = 5.0f; // AI의 이동 속도
    public float trackingDistance = 10.0f; // 추적 시작 거리

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