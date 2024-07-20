using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trace : MoveBase
{
    public override void Enter()
    {
        ai.SetTargetPosition(ArriveTargetPosition());
        Debug.Log($"{transform.name} ���� ���� ����, ��ǥ: {ai.targetList.First().Key.name}");
    }

    public override void Execute()
    {
        if (ai.targetList.Any() && ai.targetList.First().Value <= fsm.chaseThreshold)
        {
            Exit();
        }
    }

    public override void Exit()
    {
        fsm.SetState<Boundary>();
        Debug.Log($"{transform.name} ���� ���� Ż��");
    }

    public override Vector3 ArriveTargetPosition()
    {
        if (ai.targetList.Any())
        {
            Debug.Log($"{transform.name} ���� ��ǥ �缳��: {ai.targetList.First().Key.name}");
            return ai.targetList.First().Key.position;
        }
        
        return transform.position; // ���ڸ� ��ġ ��ȯ
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
