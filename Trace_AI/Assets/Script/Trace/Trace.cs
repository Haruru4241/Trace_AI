using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(TraceTargetPosition());
        ai.projectorManager.ChangeAllProjectorsToChangedColor();
        Debug.Log($"{transform.name} Trace, Enter: {ai.targetList.First().Key.name}");
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();

        // 전환 규칙이 존재하고, 그 조건이 만족되면 상태 탈출
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
        Debug.Log($"{transform.name} Trace Exit");
        ai.projectorManager.ChangeAllProjectorsToInitialColor();
        fsm.SetState(newState);
    }

    public override void ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
            //���� ���� Ŭ���� �߰� ����
            Debug.Log($"{transform.name} : TraceArrive {ai.targetList.First().Key.name}");
        }
    }

    public override Vector3 TraceTargetPosition()
    {
        if (ai.targetList.Any())
        {
            return ai.targetList.First().Key.position;
        }
        return transform.position; // ���ڸ� ��ġ ��ȯ
    }
}
