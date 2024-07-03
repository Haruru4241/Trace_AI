using System.Collections.Generic;
using UnityEngine;

public abstract class Detection : MonoBehaviour
{
    protected Transform aiTransform;  // aiTransform�� ��ȣ�� �ʵ�� �߰�
    protected Transform player;

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void SetAITransform(Transform aiTransform)
    {
        this.aiTransform = aiTransform;
    }

    public abstract bool Detect();
}
