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

    [Header("Waiting Zone")]
    [SerializeField] private List<WaitingZoneItemData> waitingZoneCatalog; // 판매 가능한 웨이팅존 아이템 목록

    [Header("Marketing")]
    [SerializeField] private List<MarketingData> marketingCatalog; // 마케팅 캠페인 목록

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
    /// </summary>
    public void PopulateAllCategories()
    {
        PopulateMarketSlots();
        PopulateEquipmentSlots();
        PopulateRecipeSlots();
        PopulateDecorationSlots();
        
        PopulateWorkerSlots();
        PopulateDistrictSlots();
        PopulateUpgradeSlots();
        PopulateMarketingSlots();
    }

    /// <summary>시장 카테고리: 모든 재료를 슬롯으로 생성합니다.</summary>
    private void PopulateMarketSlots()
    {
        Transform parent = storeUIController.GetContentParent(0);
        storeUIController.ClearSlots(parent);

        List<IngredientData> ingredients = marketManager.allIngredients;
        for (int i = 0; i < ingredients.Count; i++)
        {
            StoreItem item = StoreItem.FromIngredient(ingredients[i], ingredients[i].basePrice);
            CreateSlot(item, parent);
        }
    }

    /// <summary>장비 카테고리: 모든 장비를 슬롯으로 생성합니다.</summary>
    private void PopulateEquipmentSlots()
    {
        Transform parent = storeUIController.GetContentParent(1);
        storeUIController.ClearSlots(parent);

        List<EquipmentData> equipments = equipmentStoreManager.GetAllEquipments();
        for (int i = 0; i < equipments.Count; i++)
        {
            int finalCost = equipmentStoreManager.CalculateFinalCost(equipments[i]);
            StoreItem item = StoreItem.FromEquipment(equipments[i], finalCost);
            CreateSlot(item, parent);
        }
    }

    /// <summary>레시피 카테고리: 모든 레시피를 슬롯으로 생성합니다.</summary>
    private void PopulateRecipeSlots()
    {
        Transform parent = storeUIController.GetContentParent(2);
        storeUIController.ClearSlots(parent);

        List<RecipeStoreItem> catalog = recipeStoreManager.GetCatalog();
        for (int i = 0; i < catalog.Count; i++)
        {
            StoreItem item = StoreItem.FromRecipe(catalog[i].recipeData, catalog[i].price, catalog[i].maxPurchaseAmount);
            CreateSlot(item, parent);
        }
    }

    /// <summary>꾸미기 카테고리: 웨이팅존 아이템을 슬롯으로 생성합니다.</summary>
    private void PopulateDecorationSlots()
    {
        Transform parent = storeUIController.GetContentParent(3);
        if (parent == null) return; // Inspector에서 4번째 탭이 아직 없으면 무시
        storeUIController.ClearSlots(parent);

        if (waitingZoneCatalog == null) return;

        for (int i = 0; i < waitingZoneCatalog.Count; i++)
        {
            WaitingZoneItemData wzItem = waitingZoneCatalog[i];
            StoreItem item = StoreItem.FromDecoration(wzItem, wzItem.purchasePrice);
            CreateSlot(item, parent);
        }
    }

    /// <summary>알바생 카테고리 생성</summary>
    private void PopulateWorkerSlots()
    {
        Transform parent = storeUIController.GetContentParent(4);
        if (parent == null || WorkerManager.Instance == null) return;
        storeUIController.ClearSlots(parent);

        List<WorkerData> workers = WorkerManager.Instance.allWorkers;
        if (workers == null) return;
        for (int i = 0; i < workers.Count; i++)
        {
            StoreItem item = StoreItem.FromWorker(workers[i]);
            CreateSlot(item, parent);
        }
    }

    /// <summary>영업 구역 카테고리 생성</summary>
    private void PopulateDistrictSlots()
    {
        Transform parent = storeUIController.GetContentParent(5);
        if (parent == null || DistrictManager.Instance == null) return;
        storeUIController.ClearSlots(parent);

        List<DistrictData> districts = DistrictManager.Instance.allDistricts;
        if (districts == null) return;
        for (int i = 0; i < districts.Count; i++)
        {
            StoreItem item = StoreItem.FromDistrict(districts[i]);
            CreateSlot(item, parent);
        }
    }

    /// <summary>업그레이드 카테고리 생성</summary>
    private void PopulateUpgradeSlots()
    {
        Transform parent = storeUIController.GetContentParent(6);
        if (parent == null || PlayerUpgradeManager.Instance == null) return;
        storeUIController.ClearSlots(parent);

        List<PlayerUpgradeData> upgrades = PlayerUpgradeManager.Instance.allUpgrades;
        if (upgrades == null) return;
        for (int i = 0; i < upgrades.Count; i++)
        {
            // 만렙이 아니면 다음 레벨의 가격을 가져옵니다.
            if (!PlayerUpgradeManager.Instance.IsMaxLevel(upgrades[i].upgradeID))
            {
                int nextLevel = PlayerUpgradeManager.Instance.GetCurrentLevel(upgrades[i].upgradeID) + 1;
                int cost = upgrades[i].levels[nextLevel].cost;
                StoreItem item = StoreItem.FromUpgrade(upgrades[i], cost);
                CreateSlot(item, parent);
            }
        }
    }

    /// <summary>마케팅 카테고리 생성</summary>
    private void PopulateMarketingSlots()
    {
        Transform parent = storeUIController.GetContentParent(7);
        if (parent == null || marketingCatalog == null) return; 
        storeUIController.ClearSlots(parent);
        
        for (int i = 0; i < marketingCatalog.Count; i++)
        {
            StoreItem item = StoreItem.FromMarketing(marketingCatalog[i]);
            CreateSlot(item, parent);
        }
    }

    /// <summary>_slotPrefab을 Instantiate하고 StoreItem 데이터를 설정합니다.</summary>
    private void CreateSlot(StoreItem item, Transform parent)
    {
        if (parent == null || _slotPrefab == null) return;
        StoreItemSlotUI slot = Instantiate(_slotPrefab, parent);
        slot.Setup(item);
    }

    public void TryBuyItem(StoreItem item, int quantity)
    {
        if (item == null || item.data == null || quantity <= 0) return;

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
        int totalCost = item.finalCost * quantity;

        if (PlayerManager.Instance.CheckMoney(totalCost))
        {
            if (item.data is IngredientData ingredient)
            {
                // 시장 재료 구매
                PlayerManager.Instance.SpendMoney(totalCost);
                int totalAmount = item.amount * quantity;
                // 💡 수정: DateTime 대신 maxShelfLifeDays(int)를 직접 전달
                InventoryManager.Instance.AddIngredient(ingredient, totalAmount, ingredient.maxShelfLifeDays);

                // 💡 지출 추적
                SettlementManager.Instance?.AddExpense(totalCost);

                Debug.Log($"[StoreManager] {ingredient.ingredientName} x{totalAmount} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is EquipmentData equipmentData)
            {
                // 장비 구매
                equipmentStoreManager.BuyEquipment(equipmentData);

                // 💡 지출 추적
                SettlementManager.Instance?.AddExpense(totalCost);

                Debug.Log($"[StoreManager] {equipmentData.equipmentName} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is FoodData recipeData)
            {
                // 레시피 구매
                recipeStoreManager.BuyRecipe(recipeData, totalCost);

                // 💡 지출 추적
                SettlementManager.Instance?.AddExpense(totalCost);

                Debug.Log($"[StoreManager] {recipeData.foodName} 레시피 구매 완료! ({totalCost}원)");
            }
            else if (item.data is WaitingZoneItemData wzItem)
            {
                PlayerManager.Instance.SpendMoney(totalCost);
                WaitingZoneManager.Instance?.InstallItem(wzItem);
                SettlementManager.Instance?.AddExpense(totalCost);
                Debug.Log($"[StoreManager] {wzItem.itemName} 설치 완료! ({totalCost}원)");
            }
            else if (item.data is WorkerData worker)
            {
                if (WorkerManager.Instance != null && WorkerManager.Instance.HireWorker(worker))
                    Debug.Log($"[StoreManager] 알바생 {worker.workerName} 고용 성공!");
            }
            else if (item.data is DistrictData district)
            {
                // 해금
                if (DistrictManager.Instance != null && DistrictManager.Instance.UnlockDistrict(district))
                {
                    Debug.Log($"[StoreManager] 구역 {district.districtName} 자릿세 지불 및 해금 성공!");
                    // 원한다면 곧바로 이동도 가능
                    DistrictManager.Instance.MoveToDistrict(district);
                }
            }
            else if (item.data is PlayerUpgradeData upgrade)
            {
                if (PlayerUpgradeManager.Instance != null && PlayerUpgradeManager.Instance.PurchaseUpgrade(upgrade.upgradeID))
                    Debug.Log($"[StoreManager] {upgrade.upgradeName} 업그레이드 완료!");
            }
            else if (item.data is MarketingData marketing)
            {
                if (MarketingManager.Instance != null && MarketingManager.Instance.StartCampaign(marketing))
                    Debug.Log($"[StoreManager] {marketing.campaignName} 마케팅 캠페인 시작!");
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
