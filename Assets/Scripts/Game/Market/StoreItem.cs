using UnityEngine;

/// <summary>
/// 상점에 진열되는 아이템 하나의 래퍼 클래스입니다.
/// IngredientData, EquipmentData, FoodData 등 이종 ScriptableObject를 통합하여 다룹니다.
/// </summary>
public class StoreItem
{
    public ScriptableObject data;   // 원본 SO (IngredientData / EquipmentData / FoodData)
    public string itemName;         // 표시용 이름
    public Sprite icon;             // 표시용 아이콘
    public int finalCost;           // 최종 결제 금액 (할인 적용 후)
    public int amount;              // 구매 수량 (재료 전용, 기본 1)

    // ===== 팩토리 메서드: 각 데이터 타입에서 StoreItem 생성 =====

    public static StoreItem FromIngredient(IngredientData ingredient, int price, int amount = 1)
    {
        return new StoreItem
        {
            data = ingredient,
            itemName = ingredient.ingredientName,
            icon = ingredient.ingredientSprite,
            finalCost = price,
            amount = amount,
        };
    }

    public static StoreItem FromEquipment(EquipmentData equipment, int price)
    {
        return new StoreItem
        {
            data = equipment,
            itemName = equipment.equipmentName,
            icon = equipment.equipmentSprite,
            finalCost = price,
            amount = 1,
        };
    }

    public static StoreItem FromRecipe(FoodData recipe, int price)
    {
        return new StoreItem
        {
            data = recipe,
            itemName = recipe.foodName,
            icon = recipe.iconSprite,
            finalCost = price,
            amount = 1,
        };
    }
}
