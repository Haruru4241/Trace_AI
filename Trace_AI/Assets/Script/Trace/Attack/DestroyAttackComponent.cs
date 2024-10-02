using System.Linq;
using UnityEngine;
public class DestroyAttackComponent : AttackBase
{
    public override void Attack(GameObject Target)
    {
        // 타겟을 즉시 파괴하는 로직
        Debug.Log("타겟 파괴");
        Destroy(Target); // 타겟 파괴
    }
    
    public override void Stop(GameObject Target)
    {
        Debug.Log("타겟을 잃음");
    }
}
