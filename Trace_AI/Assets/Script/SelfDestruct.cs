using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // �浹 �̺�Ʈ�� �߻��ϸ� �ڱ� �ڽ��� �ı��մϴ�.
        GameEventSystem.RaiseTargetDestroyed(transform);
        Destroy(gameObject);
    }
}
