using System;
using System.Collections.Generic;
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

    [SerializeField] StoreItemSlotUI _slotPrefab;

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

    void Start()
    {
        PopulateAllCategories();
    }

    // ===== 슬롯 생성 =====

    /// <summary>
    /// 모든 카테고리의 슬롯을 (재)생성합니다.
    /// Start() 및 RefreshUI()에서 호출됩니다.
    /// </summary>
    public void PopulateAllCategories()
    {
        PopulateMarketSlots();
        PopulateEquipmentSlots();
        PopulateRecipeSlots();
    }

    /// <summary>
    /// 시장 카테고리: 모든 재료를 슬롯으로 생성합니다.
    /// </summary>
    private void PopulateMarketSlots()
    {
        Transform parent = storeUIController.GetContentParent(0); // 0: 시장
        storeUIController.ClearSlots(parent);

        List<IngredientData> ingredients = marketManager.allIngredients;
        for (int i = 0; i < ingredients.Count; i++)
        {
            StoreItem item = StoreItem.FromIngredient(ingredients[i], ingredients[i].basePrice);
            CreateSlot(item, parent);
        }
    }

    /// <summary>
    /// 장비 카테고리: 모든 장비를 슬롯으로 생성합니다.
    /// </summary>
    private void PopulateEquipmentSlots()
    {
        Transform parent = storeUIController.GetContentParent(1); // 1: 장비
        storeUIController.ClearSlots(parent);

        List<EquipmentData> equipments = equipmentStoreManager.GetAllEquipments();
        for (int i = 0; i < equipments.Count; i++)
        {
            int finalCost = equipmentStoreManager.CalculateFinalCost(equipments[i]);
            StoreItem item = StoreItem.FromEquipment(equipments[i], finalCost);
            CreateSlot(item, parent);
        }
    }

    /// <summary>
    /// 레시피 카테고리: 모든 레시피를 슬롯으로 생성합니다.
    /// </summary>
    private void PopulateRecipeSlots()
    {
        Transform parent = storeUIController.GetContentParent(2); // 2: 레시피
        storeUIController.ClearSlots(parent);

        List<RecipeStoreItem> catalog = recipeStoreManager.GetCatalog();
        for (int i = 0; i < catalog.Count; i++)
        {
            StoreItem item = StoreItem.FromRecipe(catalog[i].recipeData, catalog[i].price, catalog[i].maxPurchaseAmount);
            CreateSlot(item, parent);
        }
    }

    /// <summary>
    /// _slotPrefab을 Instantiate하고 StoreItem 데이터를 설정합니다.
    /// </summary>
    private void CreateSlot(StoreItem item, Transform parent)
    {
        if (parent == null || _slotPrefab == null) return;
        StoreItemSlotUI slot = Instantiate(_slotPrefab, parent);
        slot.Setup(item);
    }

    public void TryBuyItem(StoreItem item, int quantity)
    {
        if (item == null || item.data == null || quantity <= 0) return;

        // 1. 장비 구매 시: 이미 같은 장비를 보유 중이면 중단 (장비는 보통 1개만 구매 가능)
        if (item.data is EquipmentData equipment)
        {
            if (equipmentStoreManager.GetOwnedEquipment(equipment.type) == equipment)
            {
                Debug.LogWarning($"[StoreManager] 이미 {equipment.equipmentName}을(를) 보유 중입니다.");
                return;
            }
        }

        // 2. 가격 체크 및 구매 처리
        int totalCost = item.finalCost * quantity;

        

        if (PlayerManager.Instance.CheckMoney(totalCost))
        {
            if (item.data is IngredientData ingredient)
            {
                // 시장 재료 구매 (기본 단위 * 선택 수량)
                PlayerManager.Instance.SpendMoney(totalCost);
                int totalAmount = item.amount * quantity;
                DateTime expiration = DateTime.Now.AddDays(ingredient.maxShelfLifeDays);
                InventoryManager.Instance.AddIngredient(ingredient, totalAmount, expiration);
                Debug.Log($"[StoreManager] {ingredient.ingredientName} x{totalAmount} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is EquipmentData equipmentData)
            {
                // 장비 구매
                equipmentStoreManager.BuyEquipment(equipmentData);
                Debug.Log($"[StoreManager] {equipmentData.equipmentName} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is FoodData recipeData)
            {
                // 레시피 구매
                recipeStoreManager.BuyRecipe(recipeData, totalCost);
                Debug.Log($"[StoreManager] {recipeData.foodName} 레시피 구매 완료! ({totalCost}원)");
            }

            // UI 갱신
            storeUIController.RefreshUI();
        }
        else
        {
            Debug.LogWarning($"[StoreManager] 잔액이 부족합니다! ({totalCost}원 필요)");
        }
    }
}
