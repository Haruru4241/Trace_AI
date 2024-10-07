using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 네임스페이스 추가

public class TooltipManager : MonoBehaviour
{
    public TMP_Text tooltipText;           // 툴팁 텍스트 (툴팁 내용)
    public Image tooltipImage;             // 툴팁 이미지 (스프라이트)
    public GameObject tooltip;             // 툴팁 오브젝트

    // 툴팁 표시 (텍스트와 스프라이트, 그리고 위치)
    public void ShowTooltip(string text, Sprite sprite, Vector3 worldPosition)
    {
        if (tooltipText == null || tooltipImage == null || tooltip == null|| Camera.main == null) return;
        tooltipText.text = text;
        tooltipImage.sprite = sprite;
        tooltip.SetActive(true);

        // 월드 좌표를 화면 좌표로 변환하여 툴팁 위치 설정
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        tooltip.transform.position = screenPosition;
        Debug.Log(text);
    }

    // 툴팁 숨기기
    public void HideTooltip()
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
            tooltipText.text = null;
        }
    }
}
