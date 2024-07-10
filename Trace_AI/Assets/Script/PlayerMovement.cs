using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5.0f; // �÷��̾��� �̵� �ӵ�
    private float currentMoveSpeed; // ���� �̵� �ӵ�
    private List<float> speedModifiers;
    private bool isMoving;

    private AudioSource audioSource;
    public AudioClip[] movementSounds; // ���� �Ҹ� Ŭ���� ������ �迭
    private Rigidbody rb;

    void Start()
    {
        speedModifiers = new List<float> { baseMoveSpeed };
        UpdateMoveSpeed();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // ���ڱ� �Ҹ��� �ݺ����� �ʵ��� ����

        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // ���� �̵� �Է� �ޱ�
        float moveVertical = Input.GetAxis("Vertical"); // ���� �̵� �Է� �ޱ�

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        Vector3 move = moveDirection * currentMoveSpeed;

        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z); // Rigidbody�� ����� �̵�

        isMoving = moveDirection != Vector3.zero;

        if (isMoving)
        {
            if (!audioSource.isPlaying)
            {
                if (movementSounds.Length > 0)
                {
                    int randomIndex = Random.Range(0, movementSounds.Length); // ���� �ε��� ����
                    audioSource.clip = movementSounds[randomIndex];
                    audioSource.Play();
                    GameEventSystem.RaiseSoundDetected(transform);
                }
            }
        }
        else if (!isMoving && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    private void UpdateMoveSpeed()
    {
        currentMoveSpeed = 1.0f;
        foreach (float modifier in speedModifiers)
        {
            currentMoveSpeed *= modifier;
        }
    }

    public void AddSpeedModifier(float modifier)
    {
        speedModifiers.Add(modifier);
        UpdateMoveSpeed();
    }

    public void RemoveSpeedModifier(float modifier)
    {
        speedModifiers.Remove(modifier);
        UpdateMoveSpeed();
    }

    public void ResetSpeedModifiers()
    {
        speedModifiers.Clear();
        speedModifiers.Add(baseMoveSpeed);
        UpdateMoveSpeed();
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
