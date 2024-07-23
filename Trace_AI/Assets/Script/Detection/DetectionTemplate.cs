using System.Collections.Generic;
using UnityEngine;

public class DetectionTemplate : Detection
{
    [Tooltip("기즈모 색상")]
    public Color gizmoColor = Color.red;

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        return detectedObjects;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
    }
}
