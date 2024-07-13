using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundDetection : Detection
{
    public float hearingRange = 15f;

    public Color Color = Color.green;

    private List<Transform> detectedSoundSources = new List<Transform>();
    protected NavMeshAgent m_Agent;

    void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
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
        Gizmos.color = Color;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
