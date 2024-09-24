using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMovement1 : CharacterBase
{
    private AudioSource audioSource;
    public AudioClip[] movementSounds; // ���� �Ҹ� Ŭ���� ������ �迭

    private NavMeshAgent agent;
    private bool isGameStarted = false;

    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;

        agent = gameObject.GetComponent<NavMeshAgent>();
    }
    public override void Initialize()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            base.Initialize();
        }


    }

    void Update()
    {
        if (!isGameStarted) return;
        float moveHorizontal = Input.GetAxis("Horizontal"); //WASD �Է����� NavMeshAgent�� �̿��� �̵�
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        if (moveDirection != Vector3.zero)
        {
            agent.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
        }

        isMoving = moveDirection != Vector3.zero;

        if (isMoving && !audioSource.isPlaying)
        {
            if (movementSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, movementSounds.Length); //�ұ�Ģ�� �߼Ҹ� ������ ���� ���� ����
                audioSource.clip = movementSounds[randomIndex];
                audioSource.Play();
                GameEventSystem.RaiseSoundDetected(transform); //�Ҹ� ������ ���� �߼Ҹ� ��ġ ����
            }
        }
        else if (!isMoving && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public override void UpdateSpeed(float value)
    {
        currentMoveSpeed = value;
        if (agent != null) agent.speed = value;//agent�� ���ǵ� �� �Է�
    }
}
