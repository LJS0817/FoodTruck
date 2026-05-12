using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Sub Managers")]
    [SerializeField] private EquipmentStoreManager equipmentStoreManager;
    [SerializeField] private WorkerManager workerManager;
    [SerializeField] private DistrictManager districtManager;
    [SerializeField] private PlayerUpgradeManager playerUpgradeManager;

    [SerializeField] StoreItemSlotUI _slotPrefab;

    [Header("UI Controller")]
    [SerializeField] private UpgradeUIController upgradeUIController;

    public EquipmentStoreManager EquipmentStore => equipmentStoreManager;
    public WorkerManager Worker => workerManager;
    public DistrictManager District => districtManager;
    public PlayerUpgradeManager Upgrade => playerUpgradeManager;
    public UpgradeUIController UIController => upgradeUIController;

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
        PopulateEquipmentSlots();
        PopulateWorkerSlots();
        PopulateDistrictSlots();
        PopulateUpgradeSlots();
    }

    private void PopulateEquipmentSlots()
    {
        Transform parent = upgradeUIController.GetContentParent(0);
        if (parent == null || equipmentStoreManager == null) return;
        upgradeUIController.ClearSlots(parent);

        List<EquipmentData> equipments = equipmentStoreManager.GetAllEquipments();
        for (int i = 0; i < equipments.Count; i++)
        {
            int finalCost = equipmentStoreManager.CalculateFinalCost(equipments[i]);
            StoreItem item = StoreItem.FromEquipment(equipments[i], finalCost);
            CreateSlot(item, parent);
        }
    }

    private void PopulateWorkerSlots()
    {
        Transform parent = upgradeUIController.GetContentParent(1);
        if (parent == null || workerManager == null) return;
        upgradeUIController.ClearSlots(parent);

        List<WorkerData> workers = workerManager.allWorkers;
        if (workers == null) return;
        for (int i = 0; i < workers.Count; i++)
        {
            StoreItem item = StoreItem.FromWorker(workers[i]);
            CreateSlot(item, parent);
        }
    }

    private void PopulateDistrictSlots()
    {
        Transform parent = upgradeUIController.GetContentParent(2);
        if (parent == null || districtManager == null) return;
        upgradeUIController.ClearSlots(parent);

        List<DistrictData> districts = districtManager.allDistricts;
        if (districts == null) return;
        for (int i = 0; i < districts.Count; i++)
        {
            StoreItem item = StoreItem.FromDistrict(districts[i]);
            CreateSlot(item, parent);
        }
    }

    private void PopulateUpgradeSlots()
    {
        Transform parent = upgradeUIController.GetContentParent(3);
        if (parent == null || playerUpgradeManager == null) return;
        upgradeUIController.ClearSlots(parent);

        List<PlayerUpgradeData> upgrades = playerUpgradeManager.allUpgrades;
        if (upgrades == null) return;
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (!playerUpgradeManager.IsMaxLevel(upgrades[i].upgradeID))
            {
                int nextLevel = playerUpgradeManager.GetCurrentLevel(upgrades[i].upgradeID) + 1;
                int cost = upgrades[i].levels[nextLevel].cost;
                StoreItem item = StoreItem.FromUpgrade(upgrades[i], cost);
                CreateSlot(item, parent);
            }
        }
    }

    private void CreateSlot(StoreItem item, Transform parent)
    {
        if (parent == null || _slotPrefab == null) return;
        StoreItemSlotUI slot = upgradeUIController.GetOrCreateSlot(_slotPrefab, parent);
        slot.Setup(item, (i) => upgradeUIController.ShowItemInfo(i, true));
    }

    public void TryBuyUpgrade(StoreItem item, int quantity)
    {
        if (item == null || item.data == null || quantity <= 0) return;

        if (item.data is EquipmentData equipment)
        {
            if (equipmentStoreManager.GetOwnedEquipment(equipment.type) == equipment)
            {
                Debug.LogWarning($"[UpgradeManager] 이미 {equipment.equipmentName}을(를) 보유 중입니다.");
                return;
            }
        }

        int totalCost = item.finalCost * quantity;

        if (PlayerManager.Instance.CheckMoney(totalCost))
        {
            if (item.data is EquipmentData equipmentData)
            {
                equipmentStoreManager.BuyEquipment(equipmentData);
                SettlementManager.Instance?.AddExpense(totalCost);
                Debug.Log($"[UpgradeManager] {equipmentData.equipmentName} 구매 완료! ({totalCost}원)");
            }
            else if (item.data is WorkerData worker)
            {
                if (workerManager != null && workerManager.HireWorker(worker))
                    Debug.Log($"[UpgradeManager] 알바생 {worker.workerName} 고용 성공!");
            }
            else if (item.data is DistrictData district)
            {
                if (districtManager != null && districtManager.UnlockDistrict(district))
                {
                    Debug.Log($"[UpgradeManager] 구역 {district.districtName} 자릿세 지불 및 해금 성공!");
                    districtManager.MoveToDistrict(district);
                }
            }
            else if (item.data is PlayerUpgradeData upgrade)
            {
                if (playerUpgradeManager != null && playerUpgradeManager.PurchaseUpgrade(upgrade.upgradeID))
                    Debug.Log($"[UpgradeManager] {upgrade.upgradeName} 업그레이드 완료!");
            }

            upgradeUIController.RefreshUI();
        }
        else
        {
            Debug.LogWarning($"[UpgradeManager] 잔액이 부족합니다! ({totalCost}원 필요)");
        }
    }
}
