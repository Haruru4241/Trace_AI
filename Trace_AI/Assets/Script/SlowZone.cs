using UnityEngine;

public class SlowZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.AddSpeedModifier(0.5f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.RemoveSpeedModifier(0.5f);
        }
    }
}
