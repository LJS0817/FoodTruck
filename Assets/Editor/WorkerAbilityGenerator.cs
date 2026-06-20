#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class WorkerAbilityGenerator
{
    [MenuItem("Tycoon/Generate Worker Abilities")]
    public static void GenerateAbilities()
    {
        string folderPath = "Assets/ScriptableObjects/WorkerAbilities";
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "WorkerAbilities");

        CreateAbility(folderPath, "PatienceBoost", WorkerAbility.PatienceBoost, "홀 매니저", "손님의 대기 인내심이 줄어드는 속도를 늦춰줍니다.", 0.05f, 0.2f);
        CreateAbility(folderPath, "StaminaSaver", WorkerAbility.StaminaSaver, "주방 보조", "요리 시 사장님의 피로도 소모량을 줄여줍니다.", 0.05f, 0.2f);
        CreateAbility(folderPath, "AutoCookSpeedUp", WorkerAbility.AutoCookSpeedUp, "메인 셰프", "자동 요리 및 재료 가공 속도를 단축시킵니다.", 0.05f, 0.3f);
        CreateAbility(folderPath, "SpawnRateBoost", WorkerAbility.SpawnRateBoost, "호객꾼", "거리에 손님이 더 자주, 많이 스폰됩니다.", 0.1f, 0.3f);
        CreateAbility(folderPath, "TipBonus", WorkerAbility.TipBonus, "미소 천사", "손님이 요리를 받고 팁을 줄 확률과 금액이 증가합니다.", 0.1f, 0.4f);
        CreateAbility(folderPath, "IngredientDiscount", WorkerAbility.IngredientDiscount, "흥정의 달인", "새벽 시장에서 재료를 구매할 때 할인을 받습니다.", 0.05f, 0.15f);
        CreateAbility(folderPath, "HygieneSaver", WorkerAbility.HygieneSaver, "청소 반장", "시간 경과에 따른 트럭 청결도 하락 속도를 늦춥니다.", 0.1f, 0.25f);
        CreateAbility(folderPath, "PremiumRateBoost", WorkerAbility.PremiumRateBoost, "영업 사원", "손님이 일반 요리 대신 프리미엄 요리를 주문할 확률을 높입니다.", 0.05f, 0.2f);
        CreateAbility(folderPath, "WeatherResist", WorkerAbility.WeatherResist, "날씨 요정", "비나 눈이 올 때 손님이 감소하는 페널티를 완화합니다.", 0.1f, 0.3f);
        CreateAbility(folderPath, "WaitingCapacity", WorkerAbility.WaitingCapacity, "줄세우기 장인", "트럭 앞에 줄을 설 수 있는 웨이팅 최대 인원을 증가시킵니다.", 1f, 3f);
        CreateAbility(folderPath, "SellPriceBonus", WorkerAbility.SellPriceBonus, "회계사", "모든 요리의 최종 판매 수익을 %로 증가시킵니다.", 0.05f, 0.15f);
        CreateAbility(folderPath, "CookingMinigameEasy", WorkerAbility.CookingMinigameEasy, "수석 셰프", "요리 미니게임의 속도가 느려지고 판정이 후해집니다.", 0.1f, 0.3f);
        CreateAbility(folderPath, "VIPSpawnBoost", WorkerAbility.VIPSpawnBoost, "인맥왕", "특별한 보상을 주는 VIP 단골 손님이 등장할 확률을 높입니다.", 0.02f, 0.1f);
        CreateAbility(folderPath, "MarketRefreshDiscount", WorkerAbility.MarketRefreshDiscount, "정보원", "시장 시세 및 알바생 목록 갱신 시 들어가는 비용을 깎아줍니다.", 0.1f, 0.3f);
        CreateAbility(folderPath, "OvertimeBonus", WorkerAbility.OvertimeBonus, "야행성 올빼미", "심야(밤) 영업 시 판매 수익에 보너스가 붙습니다.", 0.1f, 0.25f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan>[WorkerAbility] 15종의 알바생 특화 능력 SO가 성공적으로 생성되었습니다!</color>");
    }

    private static void CreateAbility(string path, string fileName, WorkerAbility type, string name, string desc, string minStr, string maxStr)
    {
    }

    private static void CreateAbility(string path, string fileName, WorkerAbility type, string name, string desc, float minVal, float maxVal)
    {
        string fullPath = $"{path}/{fileName}.asset";
        
        WorkerAbilityData existing = AssetDatabase.LoadAssetAtPath<WorkerAbilityData>(fullPath);
        if (existing == null)
        {
            WorkerAbilityData newData = ScriptableObject.CreateInstance<WorkerAbilityData>();
            newData.abilityType = type;
            newData.abilityName = name;
            newData.description = desc;
            newData.minBaseValue = minVal;
            newData.maxBaseValue = maxVal;
            AssetDatabase.CreateAsset(newData, fullPath);
        }
    }
}
#endif
