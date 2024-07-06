using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 플레이어의 이동 속도
    public float canMove = 0.5f;
    private float originalMoveSpeed;
    private bool isMoving;

    void Start()
    {
        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // 수평 이동 입력 받기
        float moveVertical = Input.GetAxis("Vertical"); // 수직 이동 입력 받기

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        float movementMagnitude = new Vector3(moveHorizontal, 0.0f, moveVertical).magnitude;
        float effectiveSpeed = Mathf.Min(movementMagnitude, 1.0f) * moveSpeed;

        Vector3 targetPosition = transform.position + moveDirection * effectiveSpeed * Time.deltaTime;

        // 다음 위치에 장애물이 있는지 확인
        if (!Physics.CheckSphere(targetPosition, canMove, LayerMask.GetMask("unwalkableMask")))
        {
            transform.position = targetPosition; // 플레이어 위치 업데이트
        }

        isMoving = moveDirection != Vector3.zero;
    }

    public bool IsMoving()
    {
        return isMoving;
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
