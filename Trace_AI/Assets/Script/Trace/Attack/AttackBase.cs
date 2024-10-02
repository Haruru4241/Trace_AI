using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    // 공격 실행 메서드 - 각 자식 클래스에서 구현
    public abstract void Attack(GameObject target);

    // 공격 중단 메서드 - 각 자식 클래스에서 필요에 따라 구현
    public abstract void Stop(GameObject target);
}
