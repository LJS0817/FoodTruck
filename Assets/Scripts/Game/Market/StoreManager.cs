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
                recipeStoreManager.BuyRecipe(recipeData, totalCost);
                SettlementManager.Instance?.AddExpense(totalCost);
                Debug.Log($"[StoreManager] {recipeData.foodName} 레시피 구매 완료! ({totalCost}원)");
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

            storeUIController.RefreshUI();
        }
        else
        {
            Debug.LogWarning($"[StoreManager] 잔액이 부족합니다! ({totalCost}원 필요)");
        }
    }
}
