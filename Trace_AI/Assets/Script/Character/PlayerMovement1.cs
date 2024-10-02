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

    private List<GameObject> objectsInRange = new List<GameObject>();

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 리스트에서 파괴된 오브젝트를 제거하면서 상호작용 처리
            objectsInRange.RemoveAll(obj => obj == null); // 리스트에서 null(파괴된 오브젝트) 제거

            foreach (GameObject obj in objectsInRange)
            {
                if (obj != null) // 오브젝트가 여전히 유효한지 확인
                {
                    IInteractable interactable = obj.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact(); // 상호작용 메서드 호출
                    }
                }
            }
        }


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

    private void OnTriggerEnter(Collider other)
    {
        if (!objectsInRange.Contains(other.gameObject))
        {
            objectsInRange.Add(other.gameObject);
            ITooltip Tooltip = other.gameObject.GetComponent<ITooltip>();
            if (Tooltip != null && GameManager.Instance != null)
            {
                Tooltip.ShowTooltip(); // 상호작용 호출
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsInRange.Contains(other.gameObject))
        {
            ITooltip Tooltip = other.gameObject.GetComponent<ITooltip>();
            if (Tooltip != null)
            {
                Tooltip.HideTooltip(); // 상호작용 호출
            }
            objectsInRange.Remove(other.gameObject);
        }
    }

    public override void UpdateSpeed(float value)
    {
        currentMoveSpeed = value;
        if (agent != null) agent.speed = value;//agent�� ���ǵ� �� �Է�
    }
    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 RaiseTargetDestroyed 호출
        if (GameManager.Instance != null)
        {
            GameEventSystem.RaiseTargetDestroyed(this.transform);

            if (GameManager.Instance.tooltipManager != null)
                // 적절한 게임 오버 또는 실패 메시지 출력
                GameManager.Instance.tooltipManager.ShowTooltip("You have been destroyed! Mission failed!", null, Vector3.zero);

            Debug.Log($"{gameObject.name} 파괴");
        }
    }
}
