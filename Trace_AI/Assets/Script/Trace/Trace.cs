using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(TraceTargetPosition());
        Debug.Log($"{transform.name} ���� ���� ����, ��ǥ: {ai.targetList.First().Key.name}");
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
        // if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.chaseThreshold)
        // {
        //     Exit();
        // }
    }

    public override void Exit(MoveBase newState)
    {
        Debug.Log($"{transform.name} ���� ���� Ż��");
        fsm.SetState(newState);
        //fsm.SetState<Boundary>();
    }

    public override void ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
            //���� ���� Ŭ���� �߰� ����
            Debug.Log($"{transform.name} ��ǥ ���� �Ϸ�: {ai.targetList.First().Key.name}");
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
