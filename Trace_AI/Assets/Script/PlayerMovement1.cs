using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMovement1 : CharacterBase
{
    private AudioSource audioSource;
    public AudioClip[] movementSounds; // 여러 소리 클립을 저장할 배열

    private NavMeshAgent agent;

    public override void Initialize()
    {
        base.Initialize();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;

        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); //WASD 입력으로 NavMeshAgent을 이용해 이동
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
                int randomIndex = Random.Range(0, movementSounds.Length); //불규칙한 발소리 생성을 위해 랜덤 선택
                audioSource.clip = movementSounds[randomIndex];
                audioSource.Play();
                GameEventSystem.RaiseSoundDetected(transform); //소리 감지를 위해 발소리 위치 제공
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
        if (agent != null) agent.speed = value;//agent에 스피드 값 입력
    }
}
