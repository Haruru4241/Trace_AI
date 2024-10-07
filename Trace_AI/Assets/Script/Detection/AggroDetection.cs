using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AggroDetection : Detection
{
    public Color gizmoColor = Color.red; // 기즈모 색상

    private List<Transform> detectedAggroSources = new List<Transform>(); // 감지된 어그로 소스 목록

    private void OnEnable()
    {
        GameEventSystem.OnAggroDetected += HandleAggroEvent;  // 어그로 감지 이벤트 등록
    }

    private void OnDisable()
    {
        GameEventSystem.OnAggroDetected -= HandleAggroEvent;  // 어그로 감지 이벤트 해제
    }

    private void HandleAggroEvent(object sender, AggroSource e)
    {
        // 어그로 소스와 현재 위치 간 거리 계산
        if (Vector3.Distance(transform.position, e.source.position) <= e.distance)
        {
            detectedAggroSources.Add(e.target);
        }
    }

    public override List<Transform> Detect()
    {
        List<Transform> detectedAggroSource = new List<Transform>(detectedAggroSources);
        detectedAggroSources.Clear();               // 감지 목록 초기화

        return detectedAggroSource;
    }

    // Gizmos로 감지 범위 시각화
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
public class AggroSource
{
    public Transform source;
    public Transform target;
    public float distance;

    public AggroSource(Transform target, Transform source, float distance)
    {
        this.target = target;
        this.source = source;
        this.distance = distance;
    }
}