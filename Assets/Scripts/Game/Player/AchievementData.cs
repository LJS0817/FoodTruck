using UnityEngine;

public enum AchievementType
{
    TotalCustomers,
    TotalMoneyEarned,
    PremiumDishes // 프리미엄 누적 등 자유롭게 확장 가능
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Tycoon/Achievement")]
public class AchievementData : ScriptableObject
{
    public string titleID;       // 고유 ID (예: "Title_Rich")
    public string titleName;     // 칭호 이름 (예: "초보 사장님", "건물주")
    [TextArea]
    public string description;   // 달성 조건 설명
    
    public AchievementType type;
    public int requirement;      // 달성 요구 수치

    [Header("Buffs")]
    public float extraTipMultiplier = 1.0f; // 칭호 장착 시 기본 팁 배율 보너스 (1.0 = 보너스 없음)
}
