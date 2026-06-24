using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 관련 시스템(시장, 레시피, 꾸미기, 마케팅)을 통합 관리하는 허브 클래스입니다.
/// </summary>
public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Header("Sub Managers")]
    [SerializeField] private MarketManager marketManager;
    [SerializeField] private RecipeStoreManager recipeStoreManager;
    [SerializeField] private MarketingManager marketingManager;
    [SerializeField] private WaitingZoneManager waitingZoneManager;

    [SerializeField] StoreItemSlotUI _slotPrefab;

    [Header("UI Controller")]
    [SerializeField] private StoreUIController storeUIController;

    public MarketManager Market => marketManager;
    public RecipeStoreManager RecipeStore => recipeStoreManager;
    public MarketingManager Marketing => marketingManager;
    public WaitingZoneManager WaitingZone => waitingZoneManager;
    public StoreUIController UIController => storeUIController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        PopulateAllCategories();
    }

    public void PopulateAllCategories()
    {
        PopulateMarketSlots();
        PopulateRecipeSlots();
        PopulateDecorationSlots();
        PopulateMarketingSlots();
        PopulateEquipmentStoreSlots();
        PopulateRecipeSetSlots();
    }

    private void PopulateMarketSlots()
    {
        Transform parent = storeUIController.GetContentParent(0);
        storeUIController.ClearSlots(parent);

        List<IngredientData> ingredients = marketManager.GetAllIngredients();
        for (int i = 0; i < ingredients.Count; i++)
        {
            StoreItem item = StoreItem.FromIngredient(ingredients[i], ingredients[i].basePrice);
            CreateSlot(item, parent);
        }
    }

    private void PopulateRecipeSlots()
    {
        Transform parent = storeUIController.GetContentParent(1);
        storeUIController.ClearSlots(parent);

        List<RecipeStoreItem> catalog = recipeStoreManager.GetCatalog();
        for (int i = 0; i < catalog.Count; i++)
        {
            StoreItem item = StoreItem.FromRecipe(catalog[i].recipeData, catalog[i].price, catalog[i].maxPurchaseAmount);
            CreateSlot(item, parent);
        }
    }

    private void PopulateDecorationSlots()
    {
        Transform parent = storeUIController.GetContentParent(2);
        if (parent == null || waitingZoneManager == null) return;
        storeUIController.ClearSlots(parent);

        var catalog = waitingZoneManager.allWaitingZoneItems;
        if (catalog == null) return;

        for (int i = 0; i < catalog.Count; i++)
        {
            WaitingZoneItemData wzItem = catalog[i];
            StoreItem item = StoreItem.FromDecoration(wzItem, wzItem.purchasePrice);
            CreateSlot(item, parent);
        }
    }

    private void PopulateMarketingSlots()
    {
        Transform parent = storeUIController.GetContentParent(3);
        if (parent == null || marketingManager == null) return; 
        storeUIController.ClearSlots(parent);
        
        var catalog = marketingManager.allMarketingCampaigns;
        if (catalog == null) return;

        for (int i = 0; i < catalog.Count; i++)
        {
            StoreItem item = StoreItem.FromMarketing(catalog[i]);
            CreateSlot(item, parent);
        }
    }

    private void PopulateEquipmentStoreSlots()
    {
        Transform parent = storeUIController.GetContentParent(4);
        if (parent == null || EquipmentStoreManager.Instance == null) return;
        storeUIController.ClearSlots(parent);

        List<EquipmentData> equipments = EquipmentStoreManager.Instance.GetAllEquipments();
        for (int i = 0; i < equipments.Count; i++)
        {
            StoreItem item = StoreItem.FromEquipment(equipments[i], equipments[i].price);
            CreateSlot(item, parent);
        }
    }

    private void PopulateRecipeSetSlots()
    {
        Transform parent = storeUIController.GetContentParent(5);
        if (parent == null) return;
        storeUIController.ClearSlots(parent);

        if (CookingManager.Instance == null || CookingManager.Instance.recipeManager == null) return;

        List<FoodData> unlockedRecipes = CookingManager.Instance.recipeManager.GetAllUnlockedRecipes();
        for (int i = 0; i < unlockedRecipes.Count; i++)
        {
            int price = CalculateRecipeSetPrice(unlockedRecipes[i]) * 10; // 10세트 가격
            StoreItem item = StoreItem.FromRecipeIngredientSet(unlockedRecipes[i], price);
            CreateSlot(item, parent);
        }
    }

    private void CreateSlot(StoreItem item, Transform parent)
    {
        if (parent == null || _slotPrefab == null) return;
        StoreItemSlotUI slot = storeUIController.GetOrCreateSlot(_slotPrefab, parent);
        slot.Setup(item, (i) => storeUIController.ShowItemInfo(i, true));
    }

    public void TryBuyItem(StoreItem item, int quantity)
    {
        if (item == null || item.data == null || quantity <= 0) return;

        int totalCost = item.finalCost * quantity;

        if (PlayerManager.Instance.CheckMoney(totalCost))
        {
            if (item.data is IngredientData ingredient)
            {
                PlayerManager.Instance.SpendMoney(totalCost);
                int totalAmount = item.amount * quantity;
                InventoryManager.Instance.AddIngredient(ingredient, totalAmount, ingredient.maxShelfLifeDays);
                SettlementManager.Instance?.AddExpense(totalCost);
                Debug.Log($"[StoreManager] {ingredient.ingredientName} x{totalAmount} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is FoodData recipeData)
            {
                if (item.itemType == StoreItemType.RecipeIngredientSet)
                {
                    ExecuteRecipeIngredientSetPurchase(recipeData, item.amount * quantity);
                }
                else
                {
                    recipeStoreManager.BuyRecipe(recipeData, totalCost);
                    SettlementManager.Instance?.AddExpense(totalCost);
                    Debug.Log($"[StoreManager] {recipeData.foodName} 레시피 구매 완료! ({totalCost}원)");
                }
            }
            else if (item.data is WaitingZoneItemData wzItem)
            {
                PlayerManager.Instance.SpendMoney(totalCost);
                waitingZoneManager?.InstallItem(wzItem);
                SettlementManager.Instance?.AddExpense(totalCost);
                Debug.Log($"[StoreManager] {wzItem.itemName} 설치 완료! ({totalCost}원)");
            }
            else if (item.data is MarketingData marketing)
            {
                if (marketingManager != null && marketingManager.StartCampaign(marketing))
                    Debug.Log($"[StoreManager] {marketing.campaignName} 마케팅 캠페인 시작!");
            }
            else if (item.data is EquipmentData equipment)
            {
                if (EquipmentStoreManager.Instance.HasEquipment(equipment))
                {
                    Debug.LogWarning($"[StoreManager] 이미 {equipment.equipmentName}을 보유 중입니다.");
                    return; // 이미 있으면 무시
                }

                EquipmentData currentEq = EquipmentStoreManager.Instance.GetEquippedEquipment(equipment.type);
                if (currentEq != null)
                {
                    // 장착 중인 동일 타입 장비가 있으면 팝업을 띄웁니다.
                    int normalCost = equipment.price;
                    int tradeInCost = EquipmentStoreManager.Instance.CalculateTradeInCost(equipment);
                    storeUIController.OpenTradeInPopup(equipment, normalCost, tradeInCost);
                }
                else
                {
                    // 없으면 바로 일반 구매
                    ExecuteEquipmentPurchase(equipment, false);
                }
                return; // 구매 로직은 ExecuteEquipmentPurchase로 위임됨
            }

            storeUIController.RefreshUI();
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
        }
        else
        {
            Debug.LogWarning($"[StoreManager] 잔액이 부족합니다! ({totalCost}원 필요)");
        }
    }

    public void ExecuteEquipmentPurchase(EquipmentData equipment, bool isTradeIn)
    {
        int cost = isTradeIn ? EquipmentStoreManager.Instance.CalculateTradeInCost(equipment) : equipment.price;

        if (EquipmentStoreManager.Instance.BuyEquipment(equipment, isTradeIn))
        {
            SettlementManager.Instance?.AddExpense(cost);
            
            // 구매 성공 시 Upgrade 창(인벤토리)에 실시간 슬롯 추가
            int level = EquipmentStoreManager.Instance.GetEquipmentLevel(equipment);
            StoreItem newItem = StoreItem.FromEquipmentLevel(equipment, level);
            UpgradeManager.Instance.UIController.AddEquipmentSlot(newItem);

            storeUIController.RefreshUI();
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
        }
    }

    public bool ExecuteRecipeIngredientSetPurchase(FoodData recipe, int sets)
    {
        int price = CalculateRecipeSetPrice(recipe) * (sets / 10); // CalculateRecipeSetPrice returns 1 set's price
        if (sets % 10 != 0) 
        {
             // Fallback if sets is not a multiple of 10
             price = CalculateRecipeSetPrice(recipe) * sets;
        }
        else 
        {
             price = CalculateRecipeSetPrice(recipe) * sets;
        }

        if (PlayerManager.Instance.SpendMoney(price))
        {
            if (recipe.ingredientConfigs != null)
            {
                List<int> processedIds = new List<int>();
                for (int i = 0; i < recipe.ingredientConfigs.Length; i++)
                {
                    var raw = recipe.ingredientConfigs[i].rawIngredient;
                    if (raw != null && !processedIds.Contains(raw.ingredientID))
                    {
                        processedIds.Add(raw.ingredientID);
                        InventoryManager.Instance.AddIngredient(raw, sets, raw.maxShelfLifeDays);
                    }
                }
            }
            SettlementManager.Instance?.AddExpense(price);
            Debug.Log($"[StoreManager] {recipe.foodName} 재료 {sets}세트 구매 완료! ({price}원)");
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
            return true;
        }
        return false;
    }

    public int CalculateRecipeSetPrice(FoodData recipe)
    {
        int price = 0;
        if (recipe.ingredientConfigs != null)
        {
            List<int> processedIds = new List<int>();
            for (int i = 0; i < recipe.ingredientConfigs.Length; i++)
            {
                var raw = recipe.ingredientConfigs[i].rawIngredient;
                if (raw != null && !processedIds.Contains(raw.ingredientID))
                {
                    processedIds.Add(raw.ingredientID);
                    price += raw.basePrice;
                }
            }
        }
        return price;
    }
}
