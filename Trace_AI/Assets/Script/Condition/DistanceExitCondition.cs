using UnityEngine;

public class DistanceExitCondition : StateCondition
{
    public string targetTag = "Player"; // 감지할 태그
    public float detectionRange = 5.0f; // 감지할 거리

    public bool isOutOfRange = false; // 범위 밖에 있는지 여부

    public override bool ExitCondition()
    {
        // 모든 객체를 검색하여 타겟 태그를 가진 객체 중에서 거리 밖에 있는지 확인
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject potentialTarget in potentialTargets)
        {
            float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);

            if (!isOutOfRange & distance < detectionRange)
            {
                return true; // 첫 번째로 발견된 타겟이 범위 밖에 있으면 true 반환
            }
            else if (isOutOfRange & distance > detectionRange)
            {
                return true;
            }
        }

        // 범위 내에 타겟이 있거나 감지되지 않으면 false
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
