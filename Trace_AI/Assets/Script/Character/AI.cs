using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI : CharacterBase
{
    public List<Detection> Detections;

    public Dictionary<Transform, float> targetList = new Dictionary<Transform, float>();
    public Vector3 targetPosition;

    Renderer AIrenderer;
    NavMeshAgent m_Agent;
    FSM fsm;
    private bool isGameStarted = false;
    LineRenderer lineRenderer;
    GameObject targetSphere;  // 목표 구체를 필드로 저장
    List<GameObject> entities= new List<GameObject>();
    public void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<FSM>();
        fsm.Initialize();
        AIrenderer = GetComponent<Renderer>();
        // LineRenderer 초기화
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;  // 시작 선의 두께
        lineRenderer.endWidth = 0.1f;    // 끝 선의 두께
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  // 기본 재질 설정
        lineRenderer.positionCount = 2;  // 선을 그릴 두 개의 포인트 필요
        lineRenderer.material.color = Color.blue;
        
        // 목표 위치에 작은 구체 생성
        targetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetSphere.transform.position = targetPosition;  // 목표 위치에 구체 배치
        targetSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);  // 구체 크기 설정
        targetSphere.GetComponent<Renderer>().material.color = Color.red;   // 구체 색상 설정 (빨간색)
        Destroy(targetSphere.GetComponent<Collider>());  // Collider 제거
        entities.Add(targetSphere);

        m_Agent.avoidancePriority = UnityEngine.Random.Range(1, 1000);

    }
    public void ClearAI(){
        foreach(var entity in entities){
            Destroy(entity);
        }
        entities.Clear();
    }

    public override void Initialize()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            base.Initialize();

            // LineRenderer의 두 개의 포인트 설정
            lineRenderer.positionCount = 2;  // 선을 그릴 두 개의 포인트 필요
            lineRenderer.SetPosition(0, transform.position);  // 시작점: Transform의 위치
            lineRenderer.SetPosition(1, targetPosition);      // 끝점: Target 위치
        }
    }

    void FixedUpdate()
    {
        if (!isGameStarted) return;
        fsm.UpdateFSM(Detections, ref targetList);
        lineRenderer.SetPosition(0, transform.position);

        Vector3 curentTarget = fsm.currentState.TraceTargetPosition();
        if (targetPosition != curentTarget)
        {
            targetPosition = curentTarget;
            lineRenderer.SetPosition(1, targetPosition);

            // 목표 구체 위치도 업데이트
            targetSphere.transform.position = targetPosition;
        }

        m_Agent.destination = targetPosition;

        GameEventSystem.RaiseAiAdditionalEvent();
    }

    private void OnEnable()
    {
        GameEventSystem.OnAiAdditionalEvent += UpdateColor;
        GameEventSystem.OnTargetDestroyed += HandleTargetDestroyed;
    }

    private void OnDisable()
    {
        GameEventSystem.OnAiAdditionalEvent -= UpdateColor;
        GameEventSystem.OnTargetDestroyed -= HandleTargetDestroyed;
    }

    public void SetTargetPosition(Vector3 _targetPosition)
    {
        targetPosition = _targetPosition;
    }

    public override void UpdateSpeed(float value)
    {
        currentMoveSpeed = value;
        if (m_Agent != null) m_Agent.speed = value;
    }

    private void HandleTargetDestroyed(object sender, GameEventArgs e)
    {
        if (targetList.ContainsKey(e.Source))
        {
            targetList.Remove(e.Source);
        }
    }
    void UpdateColor()
    {
        float stateValue = 0;
        if (targetList.Any())
        {
            stateValue = targetList.First().Value;
        }

        float t = Mathf.InverseLerp(0, fsm.chaseThreshold, stateValue);

        // 현재 색상에 빨간색 값을 더해주는 방식
        Color currentColor = AIrenderer.material.color;
        Color redValueToAdd = new Color(t, 0, 0); // 빨간색 채널만 추가
        AIrenderer.material.color = currentColor + redValueToAdd;

        // 색상이 1을 넘지 않도록 clamping 처리
        AIrenderer.material.color = new Color(
            Mathf.Clamp01(AIrenderer.material.color.r),
            Mathf.Clamp01(AIrenderer.material.color.g),
            Mathf.Clamp01(AIrenderer.material.color.b)
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 1f);
    }
}