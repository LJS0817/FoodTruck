using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UpdateDescriptions : EditorWindow
{
    [MenuItem("Tycoon/Tools/Update Ability Descriptions")]
    public static void UpdateAllDescriptions()
    {
        string[] guids = AssetDatabase.FindAssets("t:WorkerAbilityData");
        
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WorkerAbilityData data = AssetDatabase.LoadAssetAtPath<WorkerAbilityData>(path);
            if (data != null)
            {
                string oldDesc = data.description;
                data.description = GetConciseDescription(data.abilityType);
                
                if (oldDesc != data.description)
                {
                    EditorUtility.SetDirty(data);
                    count++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[UpdateDescriptions] 총 {count}개의 스킬 설명이 음슴체/단어형으로 업데이트 되었습니다!");
    }

    private static string GetConciseDescription(WorkerAbility ability)
    {
        switch (ability)
        {
            case WorkerAbility.PatienceBoost: return "손님 대기 인내심 감소 속도 지연";
            case WorkerAbility.StaminaSaver: return "사장님 피로도(체력) 감소 속도 완화";
            case WorkerAbility.AutoCookSpeedUp: return "자동 조리 모드 요리 속도 증가";
            case WorkerAbility.SpawnRateBoost: return "거리를 지나는 손님 스폰율 증가";
            case WorkerAbility.TipBonus: return "팁 획득 확률 및 팁 금액 증가";
            case WorkerAbility.IngredientDiscount: return "상점 및 시장 재료 구매 비용 할인";
            case WorkerAbility.HygieneSaver: return "푸드트럭 청결도 하락 속도 지연";
            case WorkerAbility.PremiumRateBoost: return "비싼 프리미엄 요리 주문 확률 증가";
            case WorkerAbility.WeatherResist: return "악천후(비/눈) 영업 페널티 완화";
            case WorkerAbility.WaitingCapacity: return "웨이팅 존 최대 대기 가능 인원 증가";
            case WorkerAbility.SellPriceBonus: return "모든 요리 판매 수익 추가 보너스";
            case WorkerAbility.CookingMinigameEasy: return "요리 미니게임(썰기/젓기) 난이도 완화";
            case WorkerAbility.VIPSpawnBoost: return "VIP(특수 손님) 등장 확률 증가";
            case WorkerAbility.MarketRefreshDiscount: return "도매 시장 새로고침(갱신) 비용 할인";
            case WorkerAbility.OvertimeBonus: return "심야(야간) 영업 시 판매 수익 보너스";
            default: return "특수 능력치 보너스 제공";
        }
    }
}
