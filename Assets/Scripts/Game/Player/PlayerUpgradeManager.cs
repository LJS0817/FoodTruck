using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("Catalog")]
    public List<PlayerUpgradeData> allUpgrades;

    // Key: upgradeID, Value: current level (0부터 시작)
    private Dictionary<string, int> _upgradeLevels = new Dictionary<string, int>();

    private void Awake()
    {
    }

    // ===== 업그레이드 시스템 =====

    public int GetCurrentLevel(string upgradeID)
    {
        if (_upgradeLevels.TryGetValue(upgradeID, out int level)) return level;
        return 0;
    }

    public float GetCurrentValue(string upgradeID)
    {
        PlayerUpgradeData data = null;
        for (int i = 0; i < allUpgrades.Count; i++)
        {
            if (allUpgrades[i].upgradeID == upgradeID)
            {
                data = allUpgrades[i];
                break;
            }
        }
        if (data == null) return 0f;

        int level = GetCurrentLevel(upgradeID);
        // 레벨이 배열 인덱스 초과 시 마지막 레벨 수치 반환
        int index = Mathf.Min(level, data.levels.Length - 1);
        return data.levels[index].value;
    }

    public bool IsMaxLevel(string upgradeID)
    {
        PlayerUpgradeData data = null;
        for (int i = 0; i < allUpgrades.Count; i++)
        {
            if (allUpgrades[i].upgradeID == upgradeID)
            {
                data = allUpgrades[i];
                break;
            }
        }
        if (data == null) return true;
        return GetCurrentLevel(upgradeID) >= data.levels.Length - 1;
    }

    public bool PurchaseUpgrade(string upgradeID)
    {
        if (IsMaxLevel(upgradeID)) return false;

        PlayerUpgradeData data = null;
        for (int i = 0; i < allUpgrades.Count; i++)
        {
            if (allUpgrades[i].upgradeID == upgradeID)
            {
                data = allUpgrades[i];
                break;
            }
        }
        if (data == null) return false;

        int nextLevel = GetCurrentLevel(upgradeID) + 1;
        int cost = data.levels[nextLevel].cost;

        if (PlayerManager.Instance.SpendMoney(cost))
        {
            _upgradeLevels[upgradeID] = nextLevel;
            SettlementManager.Instance?.AddExpense(cost);
            Debug.Log($"<color=cyan>[업그레이드] {data.upgradeName} Lv.{nextLevel} 달성!</color>");

            // 체력 등 즉시 갱신이 필요한 이벤트를 위해 (원한다면 event 추가 가능)
            
            SyncToSaveData();
            return true;
        }
        return false;
    }

    // ===== 저장 연동 =====

    public void LoadFromSaveData(List<UpgradeSaveData> upgradeDatas)
    {
        _upgradeLevels.Clear();
        if (upgradeDatas == null) return;

        foreach (var data in upgradeDatas)
        {
            _upgradeLevels[data.upgradeID] = data.level;
        }
    }

    private void SyncToSaveData()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;
        
        List<UpgradeSaveData> saveDataList = new List<UpgradeSaveData>();
        foreach (var kvp in _upgradeLevels)
        {
            saveDataList.Add(new UpgradeSaveData { upgradeID = kvp.Key, level = kvp.Value });
        }
        
        DataManager.Instance.CurrentData.upgrades = saveDataList;
    }
}
