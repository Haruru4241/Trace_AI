using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 코인에 닿으면
        if (other.CompareTag("Player"))
        {
            // 게임 매니저에게 코인 수집을 알림
            GameManager.Instance.CoinCollected();
            
            // 코인 오브젝트 제거
            Destroy(gameObject);
        }
    }
}
