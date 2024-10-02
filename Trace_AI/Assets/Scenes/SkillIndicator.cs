using UnityEngine;

public class SkillIndicator : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer 컴포넌트
    public int segments = 50; // 원을 구성할 세그먼트 수
    public float radius = 5f; // 원의 반지름

    void Start()
    {
        lineRenderer.positionCount = segments + 1; // 원을 구성할 포인트 수 설정 (종료점 포함)
        lineRenderer.useWorldSpace = false; // 월드 좌표 사용 여부 설정
        CreateCircle();
    }

    // 원형 인디케이터 생성 함수
    void CreateCircle()
    {
        float angle = 360f / segments; // 각 세그먼트의 각도 계산
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z)); // 각 포인트에 좌표 설정
        }
    }

    // 인디케이터의 위치를 업데이트하는 함수
    public void UpdatePosition(Vector3 position)
    {
        transform.position = position; // 마우스나 타겟팅 위치로 인디케이터를 이동
    }
}
