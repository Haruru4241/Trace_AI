using System.Linq;
using UnityEngine;
public class SlowZoneAttackComponent : AttackBase
{
    private GameObject currentSlowZone;

    private void Start()
    {
    }

    public override void Attack(GameObject Target)
    {
        // 타겟 위치에 슬로우존 설치
        if (currentSlowZone == null)
        {
            currentSlowZone = Instantiate(GameManager.Instance.prefebManager.slowZonePrefeb, Target.transform.position, Quaternion.identity);
            GameManager.Instance.generatedEntities.Add(currentSlowZone);
            Debug.Log("슬로우존을 설치했습니다.");
        }
    }

    public override void Stop(GameObject Target)
    {
        // 슬로우존 제거
        if (currentSlowZone != null)
        {
            Destroy(currentSlowZone);
            Debug.Log("슬로우존을 제거했습니다.");
        }
    }
}
