using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5.0f; // 플레이어의 이동 속도
    private float currentMoveSpeed; // 현재 이동 속도
    private List<float> speedModifiers;
    private bool isMoving;

    private AudioSource audioSource;
    public AudioClip[] movementSounds; // 여러 소리 클립을 저장할 배열
    private Rigidbody rb;

    void Start()
    {
        speedModifiers = new List<float> { baseMoveSpeed };
        UpdateMoveSpeed();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // 발자국 소리를 반복하지 않도록 설정

        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // 수평 이동 입력 받기
        float moveVertical = Input.GetAxis("Vertical"); // 수직 이동 입력 받기

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        Vector3 move = moveDirection * currentMoveSpeed;

        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z); // Rigidbody를 사용한 이동

        isMoving = moveDirection != Vector3.zero;

        if (isMoving)
        {
            if (!audioSource.isPlaying)
            {
                if (movementSounds.Length > 0)
                {
                    int randomIndex = Random.Range(0, movementSounds.Length); // 랜덤 인덱스 선택
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
