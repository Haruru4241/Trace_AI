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

    public override void Initialize()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<FSM>();
        fsm.Initialize();
        AIrenderer = GetComponent<Renderer>();

        base.Initialize();
    }

    void FixedUpdate()
    {
        fsm.UpdateFSM(Detections, ref targetList);

        Vector3 curentTarget = fsm.currentState.TraceTargetPosition();
        if (targetPosition != curentTarget)
        {
            targetPosition = curentTarget;
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
        AIrenderer.material.color = Color.Lerp(Color.green, Color.red, t);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 1f);
    }
}