#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EquipmentStatsConfigurator : MonoBehaviour
{
    [MenuItem("Tycoon/Setup Equipment Stats")]
    public static void SetupEquipmentStats()
    {
        string[] guids = AssetDatabase.FindAssets("t:EquipmentData", new[] { "Assets/ScriptableObjects/Equipment" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentData eq = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
            
            if (eq != null && eq.supportedProcessTypes != null)
            {
                for (int i = 0; i < eq.supportedProcessTypes.Count; i++)
                {
                    ProcessTypeEntry entry = eq.supportedProcessTypes[i];
                    ApplyUniqueStats(eq, ref entry);
                    eq.supportedProcessTypes[i] = entry;
                }
                EditorUtility.SetDirty(eq);
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("<color=green>[EquipmentStatsConfigurator] 모든 장비의 기본 성능과 성장률 셋업이 완료되었습니다!</color>");
    }

    private static void ApplyUniqueStats(EquipmentData eq, ref ProcessTypeEntry entry)
    {
        string nameLower = eq.equipmentName.ToLower();

        // 초기화 (안전장치)
        entry.timeMultiplier = 1.0f;
        entry.staminaMultiplier = 1.0f;
        entry.qualityBonus = 0f;
        entry.miniGameEaseBonus = 0f;
        
        entry.timeMultiplierGrowth = 0f;
        entry.staminaMultiplierGrowth = 0f;
        entry.qualityBonusGrowth = 0f;
        entry.miniGameEaseBonusGrowth = 0f;

        // 1. 이름 키워드 기반 컨셉 할당
        bool isSpeedFocus = nameLower.Contains("fast") || nameLower.Contains("small") || nameLower.Contains("mini") || nameLower.Contains("portable") || nameLower.Contains("basic");
        bool isQualityFocus = nameLower.Contains("premium") || nameLower.Contains("commercial") || nameLower.Contains("smart") || nameLower.Contains("ai");
        bool isEfficiencyFocus = nameLower.Contains("large") || nameLower.Contains("high") || nameLower.Contains("industrial");

        // 2. Base & Growth 설정
        // 스피드 특화: 기본 속도 빠름, 레벨업 시 속도 단축에 집중
        if (isSpeedFocus)
        {
            entry.timeMultiplier = 0.85f;
            entry.staminaMultiplier = 0.95f;
            entry.timeMultiplierGrowth = 0.03f;
            entry.staminaMultiplierGrowth = 0.01f;
        }
        // 품질 특화: 기본 속도는 평범하지만, 품질과 미니게임 완화 혜택 큼
        else if (isQualityFocus)
        {
            entry.timeMultiplier = 0.95f;
            entry.qualityBonus = 0.10f;
            entry.miniGameEaseBonus = 0.10f;
            
            entry.timeMultiplierGrowth = 0.01f;
            entry.qualityBonusGrowth = 0.03f;
            entry.miniGameEaseBonusGrowth = 0.02f;
        }
        // 효율/체력 특화: 체력 소모를 크게 줄여줌
        else if (isEfficiencyFocus)
        {
            entry.staminaMultiplier = 0.80f;
            entry.timeMultiplier = 0.90f;
            
            entry.staminaMultiplierGrowth = 0.04f;
            entry.timeMultiplierGrowth = 0.02f;
            entry.qualityBonusGrowth = 0.01f;
        }
        // 기본 밸런스형 (그 외)
        else
        {
            entry.timeMultiplier = 0.90f;
            entry.staminaMultiplier = 0.90f;
            entry.qualityBonus = 0.05f;
            
            entry.timeMultiplierGrowth = 0.02f;
            entry.staminaMultiplierGrowth = 0.02f;
            entry.qualityBonusGrowth = 0.02f;
        }

        // 특정 타입(카테고리)별 보정
        if (eq.type == EquipmentType.Refrigerator || eq.type == EquipmentType.Freezer)
        {
            // 냉장고류는 시간 단축이 큼
            entry.timeMultiplier -= 0.1f;
        }
        else if (eq.type == EquipmentType.Battery || eq.type == EquipmentType.Generator || eq.type == EquipmentType.Gas)
        {
            // 동력원류는 체력(에너지) 절감에 집중
            entry.staminaMultiplier -= 0.1f;
            entry.staminaMultiplierGrowth += 0.01f;
        }
    }
}
#endif
