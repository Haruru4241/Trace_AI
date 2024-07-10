using System.Collections.Generic;
using UnityEngine;

public class SoundDetection : Detection
{
    public float hearingRange = 15f;
    public int maxPathNodes = 50; // 설정한 최대 노드 수

    public Color Color = Color.green;

    private List<Transform> detectedSoundSources = new List<Transform>();

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

        foreach (var source in detectedSoundSources)
        {
            float distanceToSound = Vector3.Distance(transform.position, source.position);

            if (distanceToSound <= hearingRange)
            {
                List<Node> path = moveBase.FindPath(transform.position, source.position);
                if (path != null && path.Count <= maxPathNodes)
                {
                    detectedObjects.Add(source);
                }
            }
        }

        detectedSoundSources.Clear(); // 감지 후 리스트 초기화

        return detectedObjects;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
