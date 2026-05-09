using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Tycoon/Equipment")]
public class EquipmentData : ScriptableObject
{
    public EquipmentType type;
    public string equipmentName;
    public Sprite equipmentSprite;
    public int tier;            // 장비 등급 (1단계, 2단계...)
    public int price;           // 구매 가격
    public int tradeInValue;    // 보상 판매 가격 (교환 시 돌려받는 가치)
    public int maxPurchaseAmount = 1; // 최대 구매 수량 (장비는 보통 1)

    [TextArea]
    public string description;
}
