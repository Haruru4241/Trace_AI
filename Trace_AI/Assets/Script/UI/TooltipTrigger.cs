using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipText;

    // 툴팁이 나타나는 시점
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.Instance.tooltipManager.ShowTooltip(tooltipText, null, Vector3.zero);
    }

    // 툴팁이 사라지는 시점
    public void OnPointerExit(PointerEventData eventData)
    {
        //GameManager.Instance.tooltipManager.HideTooltip();
    }
}
