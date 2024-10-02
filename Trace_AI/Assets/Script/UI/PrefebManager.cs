using UnityEngine;
using System.Collections.Generic;
using System;
public class PrefebManager : MonoBehaviour
{
    // 맵 사이즈 값을 관리하는 버튼 컨트롤러
    public ButtonController mapSizeController;
    public ButtonController maxDepthController;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject hallwayPrefab;
    public GameObject doorPrefab;
    public GameObject dummyPrefeb;
    public GameObject coinPrefab;

    public GameObject slowZonePrefeb;
    public List<GameObject> playerPrefebs;

    // AI 등장 갯수를 관리하는 버튼 컨트롤러 4개
    public List<PrefabCountButton> aiCountControllers;

    private void Start()
    {
        // 값이 변경될 때마다 감지
        mapSizeController.OnValueChanged += OnMapSizeChanged;
        maxDepthController.OnValueChanged += OnMaxDepthChanged;
    }
    private void OnMapSizeChanged(int newValue)
    {
        int mapSizeValue = mapSizeController.GetValue();
        int maxDepthValue = maxDepthController.GetValue();

        // mapSizeValue를 2의 지수로 변환 (예: 32 -> 5, 64 -> 6, 128 -> 7)
        int mapSizePower = (int)Math.Log(mapSizeValue, 2);

        // mapSizePower와 maxDepth의 관계가 -2 ~ 0 사이인지 확인
        int diff = mapSizePower - maxDepthValue;

        if (diff > 2)
        {
            maxDepthController.SetValue(mapSizePower - 2);
        }
        else if (diff < 0) // 차이가 0보다 크면 mapSize를 내려야 함
        {
            maxDepthController.SetValue(mapSizePower - 2);
        }
    }

    private void OnMaxDepthChanged(int newValue)
    {
        int mapSizeValue = mapSizeController.GetValue();
        int maxDepthValue = maxDepthController.GetValue();

        // mapSizeValue를 2의 지수로 변환 (예: 32 -> 5, 64 -> 6, 128 -> 7)
        int mapSizePower = (int)Math.Log(mapSizeValue, 2);

        // mapSizePower와 maxDepth의 관계가 -2 ~ 0 사이인지 확인
        int diff = mapSizePower - maxDepthValue;

        if (diff > 2)
        {
            mapSizeController.SetValue((int)Mathf.Pow(2, maxDepthValue + 2));
        }
        else if (diff < 0) // 차이가 0보다 크면 mapSize를 내려야 함
        {
            mapSizeController.SetValue((int)Mathf.Pow(2, maxDepthValue));
        }
    }


    // 맵 사이즈 값을 가져오는 함수
    public int GetMapSizeValue()
    {
        return mapSizeController.GetValue();
    }
    public int GetMaxDepthValue()
    {
        return maxDepthController.GetValue();
    }

    // 특정 AI 등장 갯수를 가져오는 함수 (인덱스는 0~3)
    public int GetAICountValue(int aiIndex)
    {
        if (aiIndex >= 0 && aiIndex < aiCountControllers.Count)
        {
            return aiCountControllers[aiIndex].buttonController.GetValue();
        }

        return -1;  // 잘못된 인덱스일 경우
    }
    // 특정 플레이어 프리팹을 가져오는 함수
    public GameObject GetPlayerPrefab(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerPrefebs.Count)
        {
            return playerPrefebs[playerIndex];  // 프리팹 리스트에서 특정 인덱스의 프리팹을 반환
        }

        return null;  // 잘못된 인덱스일 경우 null 반환
    }

    // 특정 AI 프리팹을 가져오는 함수
    public GameObject GetAIPrefab(int aiIndex)
    {
        if (aiIndex >= 0 && aiIndex < aiCountControllers.Count)
        {
            return aiCountControllers[aiIndex].Prefab;  // AI 프리팹 반환
        }

        return null;  // 잘못된 인덱스일 경우 null 반환
    }
}

[System.Serializable]
public class PrefabCountButton
{
    public ButtonController buttonController;  // 버튼 컨트롤러
    public GameObject Prefab;                // 해당 AI 타입의 프리팹

    public PrefabCountButton(ButtonController buttonController, GameObject Prefab)
    {
        this.buttonController = buttonController;
        this.Prefab = Prefab;
    }
}