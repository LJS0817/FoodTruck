using System;
using UnityEngine;

/// <summary>
/// 상점 관련 시스템(시장, 장비, 레시피, UI)을 통합 관리하는 허브 클래스입니다.
/// </summary>
public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Header("Sub Managers")]
    [SerializeField] private MarketManager marketManager;
    [SerializeField] private EquipmentStoreManager equipmentStoreManager;
    [SerializeField] private RecipeStoreManager recipeStoreManager;

    [Header("UI Controller")]
    [SerializeField] private StoreUIController storeUIController;

    // Getter 프로퍼티
    public MarketManager Market => marketManager;
    public EquipmentStoreManager EquipmentStore => equipmentStoreManager;
    public RecipeStoreManager RecipeStore => recipeStoreManager;
    public StoreUIController UIController => storeUIController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void TryBuyItem(StoreItem item)
    {
        if (item == null || item.data == null) return;

        // 1. 장비 구매 시: 이미 같은 장비를 보유 중이면 중단
        if (item.data is EquipmentData equipment)
        {
            if (equipmentStoreManager.GetOwnedEquipment(equipment.type) == equipment)
            {
                Debug.LogWarning($"[StoreManager] 이미 {equipment.equipmentName}을(를) 보유 중입니다.");
                return;
            }
        }

        // 2. 가격 체크 및 구매 처리
        if (PlayerManager.Instance.SpendMoney(item.finalCost))
        {
            if (item.data is IngredientData ingredient)
            {
                // 시장 재료 구매
                DateTime expiration = DateTime.Now.AddDays(ingredient.maxShelfLifeDays);
                InventoryManager.Instance.AddIngredient(ingredient, item.amount, expiration);
                Debug.Log($"[StoreManager] {ingredient.ingredientName} x{item.amount} 구매 완료! ({item.finalCost}원)");
            }
            else if (item.data is EquipmentData equipmentData)
            {
                // 장비 구매
                equipmentStoreManager.BuyEquipment(equipmentData);
                Debug.Log($"[StoreManager] {equipmentData.equipmentName} 구매 완료! ({item.finalCost}원)");
            }
            else if (item.data is FoodData recipeData)
            {
                // 레시피 구매
                recipeStoreManager.BuyRecipe(recipeData, item.finalCost);
                Debug.Log($"[StoreManager] {recipeData.foodName} 레시피 구매 완료! ({item.finalCost}원)");
            }

            // UI 갱신
            storeUIController.RefreshUI();
        }
        else
        {
            Debug.LogWarning($"[StoreManager] 잔액이 부족합니다! ({item.finalCost}원 필요)");
        }
    }
}
