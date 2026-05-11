using UnityEngine;

public enum RandomEventType
{
    None,
    // --- 즉발형 (아침에 즉시 결과 발생) ---
    HealthInspector, // 식약처 단속 (평판에 따라 벌금 또는 포상)
    Thief,           // 좀도둑 (소지금 일부 분실)
    GoodKarma,       // 미담 화제 (평판 대폭 상승)

    // --- 지속형 (오늘 하루 종일 효과 유지) ---
    Festival,        // 지역 축제 (스폰율 대폭발, 인내심 급감)
    Typhoon,         // 태풍 경보 (스폰율 급감, 프리미엄 팁 3배)
    MarketShortage,  // 물가 폭등 (시장 재료비 2배)
    MarketSurplus,   // 재료 풍년 (시장 재료비 반값)
    VIPRush,         // 인플루언서 정모 (VIP 등장 확률 대폭 증가)
    FatigueSpike,    // 열대야/폭염 (사장님 피로도 소모 속도 2배)
}

[CreateAssetMenu(fileName = "New Random Event", menuName = "Tycoon/Random Event")]
public class RandomEventData : ScriptableObject
{
    public string eventName;
    [TextArea]
    public string description;
    
    public RandomEventType eventType;

    [Header("Values (이벤트 타입별로 다르게 사용됨)")]
    public float value1;
    public float value2;
}
