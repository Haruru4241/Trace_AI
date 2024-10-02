using System;
using System.Collections;
using UnityEngine;
public class Dummy : MonoBehaviour
{
    public float minRespawnTime = 3f;  // 랜덤 재활성화 시간 범위 최소값
    public float maxRespawnTime = 10f; // 랜덤 재활성화 시간 범위 최대값

    public GameObject dummy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("AI"))
        {
            GameManager.Instance.DebugLog($"{dummy.name} -> {other.name}Colide");
             GameEventSystem.RaiseTargetDestroyed(this.transform); // 이벤트 호출
            dummy.SetActive(false); // 더미 객체 비활성화

            // 일정 시간 뒤 재활성화
            StartCoroutine(RespawnAfterRandomTime());
        }
    }

    private IEnumerator RespawnAfterRandomTime()
    {
        // a~b 사이의 랜덤한 시간 대기
        float respawnTime = UnityEngine.Random.Range(minRespawnTime, maxRespawnTime);
        GameManager.Instance.DebugLog($"{dummy.name} -> {respawnTime}Second");

        yield return new WaitForSeconds(respawnTime);

        // 더미 객체 재활성화
        dummy.SetActive(true);
        GameManager.Instance.DebugLog($"{dummy.name}가 재활성화되었습니다.");
    }
}
