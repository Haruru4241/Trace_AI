using UnityEngine;

public class OptimizedVisionDetection : Detection
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
                Vector3 directionToPlayer = (new Vector3(player.position.x, aiTransform.position.y, player.position.z) - aiTransform.position).normalized;
                float angleBetween = Vector3.Angle(aiTransform.forward, directionToPlayer);

                if (angleBetween < viewAngle / 2)
                {
                    if (!Physics.Linecast(aiTransform.position, player.position, out RaycastHit hitInfo))
                    {
                        return true;
                    }
                    else if (hitInfo.collider.gameObject != player.gameObject)
                    {
                        Debug.Log($"Obstacle detected between AI and player: {hitInfo.collider.name}");
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
