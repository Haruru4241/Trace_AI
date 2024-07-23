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

        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{transform.name} ���� ���� Ż��");
        fsm.SetState<Boundary>();
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
