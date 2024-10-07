using UnityEngine;
using System.Linq;
public class ChaseThresholdCondition : StateCondition
{
    public float chaseThreshold;
    [Tooltip("true면 이하, false면 이상")]
    public bool isBelow = false; // true면 이하, false면 이상 조건을 평가
    private AI ai;

    private void Awake()
    {
        ai = GetComponent<AI>();
        if (ai == null)
        {
            Debug.LogError("AI 컴포넌트를 찾을 수 없습니다!");
        }
    }

    // 플래그에 따라 chaseThreshold 이상 또는 이하 조건 판단
    public override bool ExitCondition()
    {
        if (ai == null || !ai.targetList.Any())
            return false;

        if (isBelow)
        {
            return ai.targetList.First().Value <= chaseThreshold; // 이하인 경우
        }
        else
        {
            return ai.targetList.First().Value > chaseThreshold; // 이상인 경우
        }
    }
}
