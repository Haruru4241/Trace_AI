using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundDetection : Detection
{
    public float hearingRange = 10f;
    [Tooltip("기즈모 색상")]
    public Color gizmoColor = Color.magenta;

    private List<Transform> detectedSoundSources = new List<Transform>();
    private NavMeshAgent m_Agent;

    private void OnEnable()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        GameEventSystem.OnSoundDetected += HandleGameEvent;
    }

    private void OnDisable()
    {
        GameEventSystem.OnSoundDetected -= HandleGameEvent;
    }

    private void HandleGameEvent(object sender, GameEventArgs e)
    {
        Transform source = e.Source;
        if (Vector3.Distance(transform.position, source.position) <= hearingRange)
        {
            detectedSoundSources.Add(source);
        }
    }

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        if (!m_Agent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgent is not on a NavMesh.");
            return detectedObjects;
        }
        Vector3 originalDestination = m_Agent.destination;

        foreach (var source in detectedSoundSources)
        {
            m_Agent.destination = source.position;
            if (m_Agent.remainingDistance < hearingRange)
            {
                detectedObjects.Add(source);
            }
        }

        m_Agent.destination = originalDestination;
        detectedSoundSources.Clear(); // 감지 후 리스트 초기화

        return detectedObjects;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
