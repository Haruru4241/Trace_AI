using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Pathfinding9 pathfinding9;
    public GizmoManager9 gizmoManager9;
    public EnemyAI9 enemyAI9;
    public Grid9 grid9;
    public Transform player;
    public Transform aiObject;

    void Awake()
    {
        // Pathfinding9, GizmoManager9, EnemyAI9 스크립트를 찾아 할당
        pathfinding9 = pathfinding9 ?? FindObjectOfType<Pathfinding9>();
        gizmoManager9 = gizmoManager9 ?? FindObjectOfType<GizmoManager9>();
        enemyAI9 = enemyAI9 ?? FindObjectOfType<EnemyAI9>();
        grid9 = grid9 ?? FindObjectOfType<Grid9>();

        // Player와 AI 오브젝트를 태그를 통해 찾아 할당
        player = player ?? GameObject.FindGameObjectWithTag("Player")?.transform;
        aiObject = aiObject ?? GameObject.FindGameObjectWithTag("AI")?.transform;

        if (pathfinding9 != null)
        {
            pathfinding9.gameManager = this;
        }

        if (gizmoManager9 != null)
        {
            gizmoManager9.gameManager = this;
            if (gizmoManager9.player == null) gizmoManager9.player = player;
            if (gizmoManager9.aiObject == null) gizmoManager9.aiObject = aiObject;
        }

        if (enemyAI9 != null)
        {
            enemyAI9.gameManager = this;
            if (enemyAI9.player == null) enemyAI9.player = player;
        }

        if (grid9 != null)
        {
            grid9.gameManager = this;
        }
    }

}
