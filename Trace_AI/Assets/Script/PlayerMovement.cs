using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �÷��̾��� �̵� �ӵ�

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // ���� �̵� �Է� �ޱ�
        float moveVertical = Input.GetAxis("Vertical"); // ���� �̵� �Է� �ޱ�

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.position += movement * moveSpeed * Time.deltaTime; // �÷��̾� ��ġ ������Ʈ
    }
}
