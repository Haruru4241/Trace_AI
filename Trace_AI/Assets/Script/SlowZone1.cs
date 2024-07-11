using UnityEngine;
using UnityEngine.AI;

public class SlowZone1 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterBase playerMovement = other.GetComponent<CharacterBase>();
        if (playerMovement != null)
        {
            playerMovement.AddSpeedModifier(0.5f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterBase playerMovement = other.GetComponent<CharacterBase>();
        if (playerMovement != null)
        {
            playerMovement.RemoveSpeedModifier(0.5f);
        }
    }
}
