using System.Collections.Generic;
using UnityEngine;

public class VisionDetection : Detection
{
    public float visionRange = 7f;
    public float viewAngle = 80f;
    [Tooltip("기즈모 색상")]
    public Color gizmoColor = Color.red;

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, detectionLayerMask);
        Vector3 forward = transform.forward;

        foreach (var hit in hits)
        {
            Vector3 directionToPlayer = (hit.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(forward, directionToPlayer);

            if (dotProduct > Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad))
            {
                Ray ray = new Ray(transform.position, directionToPlayer);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, visionRange) 
                    && (detectionLayerMask.value & (1 << hitInfo.collider.gameObject.layer)) != 0)
                {
                    detectedObjects.Add(hit.transform);
                }
            }
        }

        return detectedObjects;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // 시야각을 시각적으로 표시
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -viewAngle / 2, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, viewAngle / 2, 0);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * visionRange);
    }
}
