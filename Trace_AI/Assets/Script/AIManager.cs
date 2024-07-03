/*using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameObject aiPrefab;
    public Transform player;
    public Grid grid;

    private List<GameObject> aiInstances = new List<GameObject>();

    private void Awake()
    {
        
    }
    public void CreateAI(GameObject aiPrefab//������ ������, //��ġ,)
    {

        // AI ��ü ����
        GameObject aiInstance = Instantiate(aiPrefab);
        aiInstances.Add(aiInstance);

        // AI ��ũ��Ʈ ����
        AI aiScript = aiInstance.GetComponent<AI>();
        if (aiScript == null)
        {
            Debug.LogError("AI component is not found in AI Prefab");
            return;
        }

        // �ʿ��� ������Ʈ �� �ʱ�ȭ ����
        aiScript.Player = player;
        aiScript.Grid = grid;
        aiScript.fsm = aiInstance.AddComponent<FSM>();
        aiScript.renderer = aiInstance.GetComponent<Renderer>();

        aiScript.trace = aiInstance.AddComponent<Trace>();
        aiScript.patrolState = aiInstance.AddComponent<Patrol>();
        aiScript.pursueState = aiInstance.AddComponent<Trace>();

        aiScript.trace.grid = grid;
        aiScript.patrolState.grid = grid;
        aiScript.pursueState.grid = grid;

        aiScript.detection = new Detection();
        VisionDetection visionDetection = new VisionDetection();
        OptimizedVisionDetection optimizedVisionDetection = new OptimizedVisionDetection();
        SoundDetection soundDetection = new SoundDetection();

        aiScript.detection.AddDetection(visionDetection);
        aiScript.detection.AddDetection(optimizedVisionDetection);
        aiScript.detection.AddDetection(soundDetection);

        // �÷��̾� ������ �����ϱ� ���� SetPlayer ȣ��
        aiScript.detection.SetPlayer(player);

        aiScript.fsm.InitializeStates(new List<MoveBase> { aiScript.patrolState, aiScript.pursueState });

        aiScript.OnEventOccurred += aiScript.HandleEvent;
        aiScript.StartCoroutine(aiScript.LogDebugInfo());

        aiScript.previousPosition = player.position;
    }

    public void DestroyAI(GameObject aiInstance)
    {
        if (aiInstances.Contains(aiInstance))
        {
            aiInstances.Remove(aiInstance);
            Destroy(aiInstance);
        }
        else
        {
            Debug.LogWarning("Attempted to destroy AI instance not managed by AIManager");
        }
    }

    public void DestroyAllAI()
    {
        foreach (var aiInstance in aiInstances)
        {
            Destroy(aiInstance);
        }
        aiInstances.Clear();
    }
}
*/