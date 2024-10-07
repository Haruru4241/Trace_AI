using UnityEngine;

public class AggroAttack : AttackBase
{
    public float aggroRadius = 10f; // 어그로 발생 범위
    public LayerMask aiLayer; // AI가 속한 레이어를 설정 (예: "AI" 레이어)

    // 어그로 공격 메서드
    public override void Attack(GameObject target)
    {
        if (target == null) return;

        // 어그로 발생 범위 내에 있는 AI들을 검색
        GameEventSystem.RaiseAggroDetected(target.transform, this.transform, aggroRadius);
    }

    // 어그로 공격 중단 메서드 (필요에 따라 구현)
    public override void Stop(GameObject target)
    {
        // 어그로 공격 중단 로직이 필요하다면 여기에 추가
        Debug.Log($"타겟 {target.name}에 대한 어그로 공격을 중단합니다.");
    }
}
