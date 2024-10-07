using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GateController : MonoBehaviour, ITooltip, IInteractable
{
    public GameObject door; // 문 오브젝트
    public GameObject floor; // 바닥 오브젝트

    public bool isOpen = false; // 문이 열려있는지 여부
    private bool isInteractable = true; // 상호작용 가능 여부
    public float openCloseDuration = 0.7f; // 문을 여닫는 데 걸리는 시간
    public NavMeshObstacle navMeshObstacle;

    public Sprite tooltipSprite; // 툴팁으로 사용할 스프라이트 이미지

    private Renderer doorRenderer; // 문 렌더러
    private Renderer floorRenderer; // 바닥 렌더러
    private Color originalDoorColor; // 원래 문 색상
    private Color originalFloorColor; // 원래 바닥 색상

    private void Start()
    {
        // Renderer를 가져와서 원래 색상을 저장
        doorRenderer = door.GetComponent<Renderer>();
        floorRenderer = floor.GetComponent<Renderer>();
        originalDoorColor = doorRenderer.material.color;
        originalFloorColor = floorRenderer.material.color;
        ToggleGate(!isOpen);
    }
    public void Interact()
    {
        if (isInteractable)
        {
            ToggleGate(isOpen);
        }
    }
    // 툴팁을 표시하는 메서드
    public void ShowTooltip()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.tooltipManager.ShowTooltip("Open/Close", tooltipSprite, transform.position);
    }

    // 툴팁을 숨기는 메서드
    public void HideTooltip()
    {
        GameManager.Instance.tooltipManager.HideTooltip();
    }

    // 문을 여닫는 함수
    public void ToggleGate(bool Close)
    {
        if (Close)
        {
            StartCoroutine(CloseGate());
        }
        else
        {
            StartCoroutine(OpenGate());
        }
    }



    // 문을 여는 코루틴
    private IEnumerator OpenGate()
    {
        SetInteractable(false);
        //navMeshObstacle.carving = true;
        // 문을 여는 애니메이션 혹은 동작
        yield return new WaitForSeconds(openCloseDuration); // 여는 데 걸리는 시간
        door.SetActive(false); // 문이 열리면 비활성화
        isOpen = true;
        SetInteractable(true);
        navMeshObstacle.carving = false;

    }

    // 문을 닫는 코루틴
    private IEnumerator CloseGate()
    {
        SetInteractable(false);
        //navMeshObstacle.carving = true;
        // 문을 닫는 애니메이션 혹은 동작
        yield return new WaitForSeconds(openCloseDuration); // 닫는 데 걸리는 시간
        door.SetActive(true); // 문이 닫히면 활성화
        isOpen = false;
        SetInteractable(true);
        navMeshObstacle.carving = true;
    }

    // 일정 시간 동안 상호작용 불가하게 하는 메서드
    public void DisableInteractionForTime(float duration)
    {
        StartCoroutine(DisableInteraction(duration));
    }

    // 상호작용을 비활성화하고 일정 시간 후 다시 활성화하는 코루틴
    private IEnumerator DisableInteraction(float duration)
    {
        SetInteractable(false);
        navMeshObstacle.carving = true;
        yield return new WaitForSeconds(duration);
        SetInteractable(true);

        navMeshObstacle.carving = false;
    }
    // 트리거에 AI가 들어오면 문을 자동으로 열게 한다
    // 트리거에 AI가 들어오면 문을 자동으로 열게 한다
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AI"))
        {
            if (!isOpen)
            {
                StartCoroutine(OpenGate());
            }
        }
    }
    // 상호작용 가능 여부에 따라 색상을 변경하는 함수
    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        if (!isInteractable)
        {
            // 상호작용 불가능 상태일 때 색상을 빨간색으로 변경
            doorRenderer.material.color = Color.red;
        }
        else
        {
            // 상호작용 가능 상태로 돌아왔을 때 원래 색상으로 변경
            doorRenderer.material.color = originalDoorColor;
        }
    }
}
