using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Tooltip("Layer mask to determine which objects are affected by the slow zone")]
    public LayerMask targetLayerMask;  // 슬로우 존의 영향을 받는 레이어
    public float SlowValue=0.5f;

    private void OnTriggerEnter(Collider other)
    {
        // 오브젝트의 레이어가 슬로우 존에 포함되는지 확인
        if (((1 << other.gameObject.layer) & targetLayerMask) != 0)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.AddSpeedModifier(SlowValue);  // 속도 감소 적용
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 오브젝트의 레이어가 슬로우 존에 포함되는지 확인
        if (((1 << other.gameObject.layer) & targetLayerMask) != 0)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.RemoveSpeedModifier(SlowValue);  // 속도 감소 해제
            }
        }
    }
}
