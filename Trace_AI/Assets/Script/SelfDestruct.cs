using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // 충돌 이벤트가 발생하면 자기 자신을 파괴합니다.
        GameEventSystem.RaiseTargetDestroyed(transform);
        Destroy(gameObject);
    }
}
