using UnityEngine;
using System.Collections.Generic;

public class SoundDetection : Detection
{
    public float soundRange = 5f;
    public Color Color = Color.blue;
    public int maxPathNodes = 10; // ��ο� ���Ե� �ִ� ��� ��
    public LayerMask detectionLayerMask;

    private MoveBase moveBase; // MoveBase ��ũ��Ʈ�� ����
    private bool isPlayerMoving;

    void Start()
    {
        moveBase = GetComponent<MoveBase>();
    }

    public override List<Transform> Detect()
    {
        List<Transform> detectedObjects = new List<Transform>();

        DetectPlayerMovement(player.GetComponent<PlayerMovement>()); // �÷��̾� ������ ���� �߰�

        if (isPlayerMoving)
        {
            Collider[] hits = Physics.OverlapSphere(aiTransform.position, soundRange, detectionLayerMask);
            foreach (var hit in hits)
            {
                List<Node> path = moveBase.FindPath(aiTransform.position, player.position);
                if (path != null && path.Count <= maxPathNodes)
                {
                    detectedObjects.Add(hit.transform);
                }
            }
        }
        return detectedObjects;
    }

    public void DetectPlayerMovement(PlayerMovement playerMovement)
    {
        isPlayerMoving = playerMovement.IsMoving();
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color;
            Gizmos.DrawWireSphere(transform.position, soundRange);
        }
    }
}
