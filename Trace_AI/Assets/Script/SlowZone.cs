using UnityEngine;

public class SlowZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.SetMoveSpeed(enemyAI.moveSpeed / 2);
        }

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(playerMovement.moveSpeed / 2);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.ResetMoveSpeed();
        }

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ResetMoveSpeed();
        }
    }
}
