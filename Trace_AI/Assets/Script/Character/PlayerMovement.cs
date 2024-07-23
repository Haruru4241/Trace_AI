using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : CharacterBase
{
    private AudioSource audioSource;
    public AudioClip[] movementSounds; // ���� �Ҹ� Ŭ���� ������ �迭
    private Rigidbody rb;

    public override void Initialize()
    {
        base.Initialize();

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

    public override void UpdateSpeed(float value) { }
}
