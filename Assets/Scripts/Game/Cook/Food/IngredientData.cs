using UnityEngine;
using System.Collections.Generic;
using System;

public enum IngredientState
{
    Raw,        // 준비 전 (날 것, 생 채소, 생 밀가루 등)
    Prep1,      // 중간 단계 1 (덜 익음 등)
    Prep2,      // 중간 단계 2
    Prep3,      // 중간 단계 3
    Optimal,    // 최적 완료 상태 (알맞게 익음, 완벽히 썰림 등)
    Ruined      // 사용 불가 (타버림, 뭉개짐 등)
}

[Serializable]
public class IngredientStateEntry
{
    public IngredientState state;
    [Tooltip("이 상태가 시작되는 누적 조리 시간 (0부터 시작)")]
    public float timeThreshold; 
    [Tooltip("이 상태일 때 표시될 스프라이트")]
    public Sprite stateSprite;  
}

[Serializable]
public class ProcessMethodData
{
    public ProcessType processType;
    public MiniGameType requiredMiniGame = MiniGameType.None;
    
    [Tooltip("이 가공 방식을 수행할 때 소모되는 기본 체력")]
    public float requiredStamina = 5f;

    [Tooltip("조리 시간에 따른 시각적 상태 변화 배열 (Raw -> ... -> Ruined 순서로 설정)")]
    public List<IngredientStateEntry> stateSteps;

    /// <summary>
    /// Optimal 상태가 시작되는 시간을 반환합니다 (Max 조리 시간 기준).
    /// </summary>
    public float GetOptimalTime()
    {
        if (stateSteps == null || stateSteps.Count == 0) return 0f;
        for (int i = 0; i < stateSteps.Count; i++)
        {
            if (stateSteps[i].state == IngredientState.Optimal)
                return stateSteps[i].timeThreshold;
        }
        return stateSteps[stateSteps.Count - 1].timeThreshold;
    }

    /// <summary>
    /// Ruined 상태가 시작되는 시간(타버리는 시간)을 반환합니다.
    /// </summary>
    public float GetRuinedTime()
    {
        if (stateSteps == null || stateSteps.Count == 0) return 0f;
        for (int i = stateSteps.Count - 1; i >= 0; i--)
        {
            if (stateSteps[i].state == IngredientState.Ruined)
                return stateSteps[i].timeThreshold;
        }
        // Ruined 상태가 설정되지 않았다면 Optimal 시간의 1.5배 등을 기본값으로 처리 가능
        return GetOptimalTime() + 10f; 
    }

    /// <summary>
    /// 누적 시간에 해당하는 현재 상태 엔트리를 반환합니다.
    /// </summary>
    public IngredientStateEntry GetStateAtTime(float elapsedTime)
    {
        if (stateSteps == null || stateSteps.Count == 0) return null;

        IngredientStateEntry current = stateSteps[0];
        for (int i = 0; i < stateSteps.Count; i++)
        {
            if (elapsedTime >= stateSteps[i].timeThreshold)
            {
                current = stateSteps[i];
            }
            else
            {
                break;
            }
        }
        return current;
    }
}

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tycoon/Ingredient")]
public class IngredientData : ScriptableObject
{
    public int ingredientID;
    public string ingredientName;
    public Sprite ingredientSprite;
    public float volume;
    public string description;

    [Header("경제")]
    public int basePrice;               // 재료의 기본 정가
    public int maxShelfLifeDays = 7;    // 구매 시점부터 최대 유통기한(일 수)
    public int maxPurchaseAmount = 99;  // 1회 최대 구매 가능 수량

    [Header("장비 조건")]
    public EquipmentType requiredEquipment = EquipmentType.None; // 이 재료를 구매/보관하려면 필요한 장비

    [Header("미니게임")]
    [Tooltip("이 재료를 상자에 세팅할 때 실행될 미니게임. None이면 바로 세팅됩니다.")]
    public MiniGameType requiredMiniGame = MiniGameType.None;

    [Header("맛 태그")]
    public List<FlavorTag> flavorTags;

    [Header("가공 방식 설정")]
    [Tooltip("이 재료를 장비에 올렸을 때 가능한 가공 방식과 그 결과를 배열로 정의합니다.")]
    public List<ProcessMethodData> processMethods;

    /// <summary>
    /// 특정 가공 방식에 해당하는 데이터를 반환합니다.
    /// </summary>
    public ProcessMethodData GetProcessMethod(ProcessType processType)
    {
        if (processMethods == null) return null;
        for (int i = 0; i < processMethods.Count; i++)
        {
            if (processMethods[i].processType == processType)
                return processMethods[i];
        }
        return null;
    }
}
