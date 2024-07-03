using UnityEngine;

public class SoundDetection : Detection
{
    public float soundRange = 5f;
    private bool isPlayerMoving;

    public override bool Detect()
    {
        if (isPlayerMoving)
        {
            float distance = Vector3.Distance(aiTransform.position, player.position);
            if (distance <= soundRange)
            {
                return true;
            }
        }
        return false;
    }

    public void DetectPlayerMovement(PlayerMovement playerMovement)
    {
        isPlayerMoving = playerMovement.IsMoving();
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, soundRange);
        }
    }
}
