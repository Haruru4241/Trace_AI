using System.Collections.Generic;
using UnityEngine;

public class VisionDetection : Detection
{
    public float visionRange = 10f;
    public float viewAngle = 60f;
    public LayerMask detectionLayerMask;

    public Color Color = Color.red;

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        Collider[] hits = Physics.OverlapSphere(aiTransform.position, visionRange, detectionLayerMask);
        Vector3 forward = aiTransform.forward;

        foreach (var hit in hits)
        {
            Vector3 directionToPlayer = (hit.transform.position - aiTransform.position).normalized;
            float dotProduct = Vector3.Dot(forward, directionToPlayer);

            if (dotProduct > Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad))
            {
                Ray ray = new Ray(aiTransform.position, directionToPlayer);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, visionRange))
                {
                    detectedObjects.Add(hit.transform);
                }
            }
        }

        return detectedObjects;
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);

            // 시야각을 시각적으로 표시
            Vector3 forward = transform.forward;
            Quaternion leftRayRotation = Quaternion.Euler(0, -viewAngle / 2, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, viewAngle / 2, 0);
            Vector3 leftRayDirection = leftRayRotation * forward;
            Vector3 rightRayDirection = rightRayRotation * forward;

            Gizmos.color = Color;
            Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * visionRange);
            Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * visionRange);
        }
    }
}
