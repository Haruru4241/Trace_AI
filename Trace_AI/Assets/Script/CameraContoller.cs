using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 카메라 이동 속도
    public float moveSpeed = 10f;
    // 줌 속도
    public float zoomSpeed = 2f;
    
    private Camera gameCamera;

    private void Start()
    {
        // 게임 매니저에서 카메라를 가져옵니다.
        gameCamera = GameManager.Instance.gameCamera;
    }

    private void Update()
    {
        //HandleCameraMovement();
        //HandleCameraZoom();
    }

    // 카메라의 이동을 처리하는 함수
    private void HandleCameraMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        gameCamera.transform.position += movement;
    }

    // 카메라 줌을 처리하는 함수
    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        gameCamera.orthographicSize -= scroll * zoomSpeed;
        gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize, 5f, 50f); // 줌의 최소/최대 값 설정
    }

    // 주어진 크기에 맞게 카메라 설정을 변경하는 함수
    public void SetCamera(int size)
    {
        gameCamera.orthographicSize = size / 2;
        gameCamera.transform.position = new Vector3(size / 2, 30, size / 2);
    }
}
