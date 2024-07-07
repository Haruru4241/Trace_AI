using System.Collections.Generic;
using UnityEngine;

public class SoundDetection : Detection
{
    public float hearingRange = 15f;
    public int maxPathNodes = 50; // ������ �ִ� ��� ��
    public LayerMask detectionLayerMask;

    public Color Color = Color.green;

    private List<Vector3> detectedSoundPositions = new List<Vector3>();
    private MoveBase moveBase;
    private List<Transform> detectedSoundSources = new List<Transform>();
    void Awake()
    {
        moveBase = GetComponent<MoveBase>();
    }

    private void OnEnable()
    {
        GameEventSystem.OnGameEvent += HandleGameEvent;
    }

    private void OnDisable()
    {
        GameEventSystem.OnGameEvent -= HandleGameEvent;
    }

    private void HandleGameEvent(object sender, GameEventArgs e)
    {
        Transform source = e.Source;
        if (Vector3.Distance(aiTransform.position, source.position) <= hearingRange)
        {
            detectedSoundSources.Add(source);
        }
    }

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        foreach (var source in detectedSoundSources)
        {
            float distanceToSound = Vector3.Distance(aiTransform.position, source.position);

            if (distanceToSound <= hearingRange)
            {
                List<Node> path = moveBase.FindPath(aiTransform.position, source.position);
                if (path != null && path.Count <= maxPathNodes)
                {
                    detectedObjects.Add(source);
                }
            }
        }

        detectedSoundSources.Clear(); // ���� �� ����Ʈ �ʱ�ȭ

        return detectedObjects;
    }



    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color;
            Gizmos.DrawWireSphere(transform.position, hearingRange);
        }
    }
}
