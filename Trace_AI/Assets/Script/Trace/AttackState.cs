using System.Linq;
using UnityEngine;

public class AttackState : MoveBase
{
    private Transform currentTarget; // 현재 타겟
    public AttackBase attackComponent;

    public override void Enter()
    {
        // 타겟 설정 및 추적 시작
        currentTarget = ai.targetList.First().Key;
        agent.isStopped = true;
        attackComponent.Attack(currentTarget.gameObject);
        
        Debug.Log($"{transform.name} 타겟 {currentTarget.name} 공격 실행");
    }

    public override void Execute()
    {
        // 전환 규칙을 검사하여 조건이 만족되면 상태 탈출
        foreach (var rule in fsm.FindStatetargetState(this))
        {
            if (rule.ExitCondition.ExitCondition())
            {
                Exit(rule.escapeState);
                break; // 조건이 만족되면 반복 종료
            }
        }
    }

    public override void Exit(MoveBase newState)
    {
        attackComponent.Stop(currentTarget.gameObject);
        currentTarget=null;
        agent.isStopped = false;
        Debug.Log($"{transform.name} 공격 상태 종료");
        fsm.SetState(newState);
    }

    public override void ArriveTargetPosition()
    {
    }

    public override Vector3 TraceTargetPosition()
    {
        // 타겟의 현재 위치 추적
        return currentTarget != null ? currentTarget.position : transform.position;
    }
}
