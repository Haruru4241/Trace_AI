using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MoveBaseTemplate : MoveBase
{
    public override void Enter()
    {
        Debug.Log($"{transform.name} ... ���� ����");
    }

    public override void Execute()
    {
        if (agent.remainingDistance - agent.stoppingDistance < 0.1f) ArriveTargetPosition();

        if (true)//����
        {
            Exit();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{transform.name} ... ���� Ż��");
        //fsm.SetState<MoveBase>();
    }

    public override void ArriveTargetPosition()
    {
        Debug.Log($"{transform.name} ��ǥ �Ϸ�");
    }

    public override Vector3 TraceTargetPosition()
    {
        return transform.position; // ���ڸ� ��ġ ��ȯ
    }
}
