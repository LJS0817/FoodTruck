using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 장비가 처리할 수 있는 ProcessType 목록과 그 효과를 정의합니다.
/// 냉장고처럼 Cool/Frozen 둘 다 처리하는 장비도 지원합니다.
/// </summary>
[System.Serializable]
public struct ProcessTypeEntry
{
    public ProcessType processType;

    [Tooltip("이 장비를 사용할 때 가공 시간에 적용되는 배율 (0.5 = 50% 단축)")]
    [Range(0.1f, 1.0f)]
    public float timeMultiplier;          // 1.0 = 보너스 없음, 0.5 = 50% 단축

    [Tooltip("이 장비를 사용할 때 체력 소모에 적용되는 배율 (0.5 = 50% 절감)")]
    [Range(0.1f, 1.0f)]
    public float staminaMultiplier;       // 1.0 = 보너스 없음, 0.5 = 50% 절감

    [Tooltip("이 장비를 사용할 때 결과물 품질 보너스 (0.0 ~ 1.0, 높을수록 프리미엄 확률 증가)")]
    [Range(0f, 1f)]
    public float qualityBonus;            // 0 = 보너스 없음, 1 = 무조건 프리미엄

    [Tooltip("미니게임 성공 판정 점수 기준을 낮춰주는 완화 보너스 (0 = 완화 없음, 0.3 = 30% 낮춰줌)")]
    [Range(0f, 0.5f)]
    public float miniGameEaseBonus;
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "Tycoon/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Header("기본 정보")]
    public EquipmentType type;
    public string equipmentName;
    public Sprite equipmentSprite;
    public int tier;            // 장비 등급 (1단계, 2단계...)
    public int price;           // 구매 가격
    public int tradeInValue;    // 보상 판매 가격 (교환 시 돌려받는 가치)
    public int maxPurchaseAmount = 1;

    [TextArea]
    public string description;

    [Header("지원하는 가공 방식 및 효과")]
    [Tooltip("이 장비가 지원하는 ProcessType 목록과 각 효과. 냉장고처럼 Cool/Frozen 둘 다 지원 가능.")]
    public List<ProcessTypeEntry> supportedProcessTypes;

    // ─── 런타임 유틸 ──────────────────────────────────────

    /// <summary>
    /// 이 장비가 특정 ProcessType을 지원하는지 확인합니다.
    /// </summary>
    public bool Supports(ProcessType type)
    {
        for (int i = 0; i < supportedProcessTypes.Count; i++)
        {
            if (supportedProcessTypes[i].processType == type) return true;
        }
        return false;
    }

    /// <summary>
    /// 특정 ProcessType에 해당하는 효과 구조체를 반환합니다. 없으면 기본값(보너스 없음)을 반환합니다.
    /// </summary>
    public ProcessTypeEntry GetEntry(ProcessType type)
    {
        for (int i = 0; i < supportedProcessTypes.Count; i++)
        {
            if (supportedProcessTypes[i].processType == type) return supportedProcessTypes[i];
        }
        return new ProcessTypeEntry
        {
            processType = type,
            timeMultiplier = 1f,
            staminaMultiplier = 1f,
            qualityBonus = 0f,
            miniGameEaseBonus = 0f
        };
    }
}
