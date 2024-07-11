using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovementAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector2 moveInput;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 moveDirection = transform.TransformDirection(move);
        Vector3 targetPosition = transform.position + moveDirection * agent.speed * Time.deltaTime;
        agent.destination = targetPosition;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
