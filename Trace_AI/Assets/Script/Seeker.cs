using UnityEngine;
using System.Collections.Generic;

public class Seeker : MonoBehaviour
{
    public List<Vector3> waypoints;
    public float moveSpeed = 5f;
    private int targetIndex;

    void Update()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            Vector3 targetPosition = waypoints[targetIndex];
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                targetIndex++;
                if (targetIndex >= waypoints.Count)
                {
                    waypoints.Clear();
                }
            }
        }
    }
}
