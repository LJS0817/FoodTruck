using UnityEngine;

/// <summary>
/// 상점에 진열되는 아이템 종류를 식별합니다.
/// </summary>
public enum StoreItemType
{
    Ingredient,
    Recipe,
    Equipment,
    Decoration,
    Marketing,
    District,
    RecipeIngredientSet
}

/// <summary>
/// 상점에 진열되는 아이템 하나의 래퍼 클래스입니다.
/// IngredientData, EquipmentData, FoodData 등 이종 ScriptableObject를 통합하여 다룹니다.
/// </summary>
public class StoreItem
{
    public StoreItemType itemType;  // 아이템의 종류 (구매 로직 분기용)
    public ScriptableObject data;   // 원본 SO (IngredientData / EquipmentData / FoodData)
    public string itemName;         // 표시용 이름
    public Sprite icon;             // 표시용 아이콘
    public int finalCost;           // 단가 (할인 적용 후)
    public int amount;              // 기본 구매 단위 (재료 전용, 보통 1)
    public int maxPurchaseAmount;   // 최대 구매 가능 수량

    // ===== 팩토리 메서드: 각 데이터 타입에서 StoreItem 생성 =====

    public static StoreItem FromIngredient(IngredientData ingredient, int price, int amount = 1)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Ingredient,
            data = ingredient,
            itemName = ingredient.ingredientName,
            icon = ingredient.ingredientSprite,
            finalCost = price,
            amount = amount,
            maxPurchaseAmount = ingredient.maxPurchaseAmount
        };
    }

    public static StoreItem FromEquipment(EquipmentData equipment, int price)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Equipment,
            data = equipment,
            itemName = equipment.equipmentName,
            icon = equipment.equipmentSprite,
            finalCost = price,
            amount = 1,
            maxPurchaseAmount = equipment.maxPurchaseAmount
        };
    }

    public static StoreItem FromEquipmentLevel(EquipmentData equipment, int level)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Equipment,
            data = equipment,
            itemName = $"{equipment.equipmentName} Lv.{level}",
            icon = equipment.equipmentSprite,
            finalCost = 0,
            amount = 1,
            maxPurchaseAmount = equipment.maxPurchaseAmount
        };
    }

    public static StoreItem FromRecipe(FoodData recipe, int price, int maxAmount = 1)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Recipe,
            data = recipe,
            itemName = recipe.foodName,
            icon = recipe.iconSprite,
            finalCost = price,
            amount = 1,
            maxPurchaseAmount = maxAmount
        };
    }

    public static StoreItem FromRecipeIngredientSet(FoodData recipe, int price)
    {
        return new StoreItem
        {
            itemType = StoreItemType.RecipeIngredientSet,
            data = recipe,
            itemName = $"{recipe.foodName} 재료 10세트",
            icon = recipe.iconSprite,
            finalCost = price,
            amount = 10,
            maxPurchaseAmount = 99
        };
    }

    public static StoreItem FromDecoration(WaitingZoneItemData wzItem, int price)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Decoration,
            data = wzItem,
            itemName = wzItem.itemName,
            icon = wzItem.icon,
            finalCost = price,
            amount = 1,
            maxPurchaseAmount = 1
        };
    }

    public static StoreItem FromUpgrade(PlayerUpgradeData upgrade, int cost)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Equipment, // 업그레이드는 장비와 유사하게 취급하거나 필요시 별도 타입
            data = upgrade,
            itemName = upgrade.upgradeName,
            icon = null, // 아이콘이 없다면 임시로 null (StoreSlotUI에서 처리 필요)
            finalCost = cost,
            amount = 1,
            maxPurchaseAmount = 1
        };
    }

    public static StoreItem FromMarketing(MarketingData marketing)
    {
        return new StoreItem
        {
            itemType = StoreItemType.Marketing,
            data = marketing,
            itemName = marketing.campaignName,
            icon = null,
            finalCost = marketing.cost,
            amount = 1,
            maxPurchaseAmount = 1
        };
    }

    public static StoreItem FromDistrict(DistrictData district)
    {
        return new StoreItem
        {
            itemType = StoreItemType.District,
            data = district,
            itemName = district.districtName,
            icon = district.backgroundSprite,
            finalCost = district.unlockCost,
            amount = 1,
            maxPurchaseAmount = 1
        };
    }
}
