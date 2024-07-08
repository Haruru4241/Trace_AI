using System.Collections.Generic;
using UnityEngine;

public abstract class Detection : MonoBehaviour
{
    protected Transform aiTransform;
    protected Transform player;

    public void Initialize(Transform aiTransform, Transform playerTransform)
    {
        this.aiTransform = aiTransform;
        this.player = playerTransform;
    }

    public Dictionary<string, List<Transform>> DetectedObject(List<Detection> detections)
    {
        Dictionary<string, List<Transform>> detectedObjects = new Dictionary<string, List<Transform>>();

        foreach (var detection in detections)
        {
            List<Transform> detected = detection.Detect();
            if (detected != null && detected.Count > 0)
            {
                string type = detection.GetType().Name;
                if (detectedObjects.ContainsKey(type))
                {
                    detectedObjects[type].AddRange(detected);
                }
                else
                {
                    detectedObjects[type] = new List<Transform>(detected);
                }
            }
        }
        return detectedObjects;
    }

    public virtual List<Transform> Detect() { return null; }
}
