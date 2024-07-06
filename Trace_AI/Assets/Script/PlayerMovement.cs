using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �÷��̾��� �̵� �ӵ�
    public float canMove = 0.5f;
    private float originalMoveSpeed;
    private bool isMoving;

    void Start()
    {
        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // ���� �̵� �Է� �ޱ�
        float moveVertical = Input.GetAxis("Vertical"); // ���� �̵� �Է� �ޱ�

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        float movementMagnitude = new Vector3(moveHorizontal, 0.0f, moveVertical).magnitude;
        float effectiveSpeed = Mathf.Min(movementMagnitude, 1.0f) * moveSpeed;

        Vector3 targetPosition = transform.position + moveDirection * effectiveSpeed * Time.deltaTime;

        // ���� ��ġ�� ��ֹ��� �ִ��� Ȯ��
        if (!Physics.CheckSphere(targetPosition, canMove, LayerMask.GetMask("unwalkableMask")))
        {
            transform.position = targetPosition; // �÷��̾� ��ġ ������Ʈ
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
