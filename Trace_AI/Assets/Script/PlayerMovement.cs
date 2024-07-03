using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 플레이어의 이동 속도
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

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.position += movement * moveSpeed * Time.deltaTime; // 플레이어 위치 업데이트

        isMoving = movement != Vector3.zero;
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
