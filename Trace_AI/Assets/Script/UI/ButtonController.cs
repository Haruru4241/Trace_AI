using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ButtonController : MonoBehaviour
{
    public TextMeshProUGUI valueText;  // TMP 텍스트 컴포넌트
    public List<int> valueList;        // 인트 값 리스트

    private int currentIndex = 0;      // 현재 값의 인덱스

    // 값이 변경될 때 호출되는 이벤트
    public event Action<int> OnValueChanged;

    void Start()
    {
        UpdateText();  // 초기 텍스트 설정
    }

    // 값 증가 함수 (인덱스 순환)
    public void IncreaseValue()
    {
        currentIndex = (currentIndex + 1) % valueList.Count;  // 끝에 도달하면 처음으로
        UpdateText();  // 텍스트 업데이트
        OnValueChanged?.Invoke(valueList[currentIndex]);
    }

    // 값 감소 함수 (인덱스 순환)
    public void DecreaseValue()
    {
        currentIndex = (currentIndex - 1 + valueList.Count) % valueList.Count;  // 처음이면 마지막으로
        UpdateText();  // 텍스트 업데이트
        OnValueChanged?.Invoke(valueList[currentIndex]);
    }

    // 현재 인덱스에 맞는 값으로 텍스트를 업데이트하는 함수
    private void UpdateText()
    {
        valueText.text = valueList[currentIndex].ToString();  // TMP 텍스트 업데이트
    }
    public void SetValue(int value)
    {
        // valueList에서 value와 같은 값을 찾아 그 인덱스를 currentIndex로 설정
        int index = valueList.IndexOf(value);

        // 값이 valueList에 존재하면 currentIndex를 변경
        if (index != -1)
        {
            currentIndex = index;
            UpdateText();
        }
        else
        {
            Debug.LogWarning("Value not found in valueList");
        }
    }

    public int GetValue()
    {
        return valueList[currentIndex];
    }
}
