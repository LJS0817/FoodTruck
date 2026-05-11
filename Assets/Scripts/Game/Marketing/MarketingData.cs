using UnityEngine;

public enum MarketingType
{
    SpawnBoost,   // 하루 동안 손님 스폰 증가
    VIPBoost,     // 하루 동안 VIP 등장 확률 증가
    FlavorFocus   // 하루 동안 특정 맛을 찾는 손님 비율 증가 (구현 편의상 SpawnBoost로 대체 가능)
}

[CreateAssetMenu(fileName = "New Marketing Data", menuName = "Tycoon/Marketing")]
public class MarketingData : ScriptableObject
{
    public string campaignName;
    public string description;
    public int cost;
    
    public MarketingType type;
    public float effectMultiplier; // 예: 1.5배
}
