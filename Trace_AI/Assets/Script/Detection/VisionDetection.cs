using UnityEngine;

public class VisionDetection : Detection
{
    public float visionRange = 10f;
    public float viewAngle;
    public LayerMask detectionLayerMask;

    public override bool Detect()
    {
        Collider[] hits = Physics.OverlapSphere(aiTransform.position, visionRange, detectionLayerMask);
        Vector3 forward = aiTransform.forward;

        foreach (var hit in hits)
        {
            if (hit.transform == player)
            {
                Vector3 directionToPlayer = (player.position - aiTransform.position).normalized;
                float dotProduct = Vector3.Dot(forward, directionToPlayer);

                if (dotProduct > Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad))
                {
                    Ray ray = new Ray(aiTransform.position, directionToPlayer);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, visionRange))
                    {
                        // 플레이어가 아닌 경우에만 처리
                        if (hitInfo.collider.gameObject == player.gameObject)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
    }
}
