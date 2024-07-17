/*using UnityEngine;

public class BoundaryState : MoveBase
{
    private Vector3 targetPosition;

    public override void Enter()
    {
        // ���� ��ġ���� ���� ����� �÷��̾��� ������ ��ġ�� �̵�
        targetPosition = TraceTargetPosition();
    }

    public override void Execute()
    {
        // ��ǥ ��ġ�� �̵�
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * ai.movementSpeed);

        // ��ǥ ��ġ�� �����ϸ� ���� �ൿ�� ����
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = ArriveTargetPosition();
        }
    }

    public override void Exit()
    {
        // ���� ���� �� �ʿ��� �۾�
    }

    public override Vector3 ArriveTargetPosition()
    {
        // ��ǥ ��ġ�� �������� �� ���ο� ��ǥ ��ġ ����
        int closestIndex = FindClosestPoint(transform.position, ai.possiblePositions);
        return ai.possiblePositions[closestIndex];
    }

    public override Vector3 TraceTargetPosition()
    {
        // �÷��̾��� ������ ��ġ�� ��ǥ ��ġ�� ����
        return ai.lastKnownPosition;
    }
}*/