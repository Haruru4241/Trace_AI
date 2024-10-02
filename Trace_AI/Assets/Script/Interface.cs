using UnityEngine;

public interface IInteractable
{
    void Interact(); // 상호작용 메서드
}
public interface ITooltip
{
    void ShowTooltip(); // 툴팁 내용 반환
    void HideTooltip();
}
