using System;
using System.Collections.Generic;
using UnityEngine;

// ===== Save Data Models =====

[Serializable]
public class CustomRecipeData
{
    public string customFoodName;
    public List<int> ingredientIDs = new List<int>();
    public int basePrice;
}

[Serializable]
public class RecipeSaveData
{
    public string foodName;
    public bool hasPremium;
    public bool isUnlocked;
}

/// <summary>인벤토리 아이템 1슬롯의 저장 데이터</summary>
[Serializable]
public class InventorySaveItem
{
    public int ingredientID;
    public int amount;
    public int remainingDays;
}

/// <summary>일별 정산 기록</summary>
[Serializable]
public class DailyRecord
{
    public int day;
    public int grossSales;
    public int expenses;
    public int netProfit;
    public int customerCount;
    public int premiumCount;
    public string topMenu;
}

/// <summary>업그레이드 저장 데이터 (병렬 리스트 대체)</summary>
[Serializable]
public class UpgradeSaveData
{
    public string upgradeID;
    public int level;
}

/// <summary>VIP 단골 호감도 저장 데이터</summary>
[Serializable]
public class VIPLoyaltyData
{
    public string vipName;
    public int loyaltyLevel;
}

/// <summary>환경 설정 데이터 (별도 분리)</summary>
[Serializable]
public class SettingsData
{
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool isVibrationOn = true;
}

[Serializable]
public class SaveData
{
    public int currentMoney = 0;
    public int currentDay = 1;
    public int reputation = 30; // 💡 평판 (기본값 30)

    public List<RecipeSaveData> unlockedRecipes = new List<RecipeSaveData>();
    // 💡 유저가 새롭게 연구하여 만들어낸 커스텀 레시피 목록
    public List<CustomRecipeData> customRecipes = new List<CustomRecipeData>();

    // 💡 인벤토리 저장
    public List<InventorySaveItem> inventoryItems = new List<InventorySaveItem>();

    // 💡 보유 장비 저장 (EquipmentType enum의 int 변환 값)
    public List<int> ownedEquipmentIDs = new List<int>();

    // 💡 웨이팅존 아이템 저장 (asset 이름)
    public List<string> waitingZoneItemNames = new List<string>();

    // 💡 일별 기록 히스토리
    public List<DailyRecord> dailyHistory = new List<DailyRecord>();

    // 💡 알바생, 구역, 플레이어 업그레이드 데이터
    public List<int> hiredWorkerIDs = new List<int>();
    public List<int> unlockedDistrictIDs = new List<int>();
    public int currentDistrictID = 0;
    
    // 💡 단일 리스트로 구조 개선 (병렬 리스트 제거)
    public List<UpgradeSaveData> upgrades = new List<UpgradeSaveData>();

    // 💡 향후 방치형/이벤트 시간 계산용 타임스탬프 (UTC Ticks)
    public long lastSaveTimeTicks = 0;

    // 💡 은행 대출 및 파산 시스템
    public int bankLoan = 0;
    public int bankruptDays = 0;

    // 💡 VIP 단골 시스템
    public List<VIPLoyaltyData> vipLoyalties = new List<VIPLoyaltyData>();

    // 💡 업적 및 칭호 시스템 (누적 통계)
    public int totalCustomersServed = 0;
    public int totalMoneyEarned = 0;
    public List<string> unlockedTitles = new List<string>();
    public string equippedTitleID = "";
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public SaveData CurrentData { get; private set; }
    public SettingsData CurrentSettings { get; private set; }
    
    private const string SAVE_KEY = "TycoonSaveData";
    private const string SETTINGS_KEY = "TycoonSettingsData";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        // 💡 GameManager가 통제하도록 여기서 LoadGameData()를 부르지 않고 대기합니다.
    }

    // 💡 GameManager에서 호출할 수 있도록 명시적 초기화 함수를 부활시켰습니다.
    public void Initialize()
    {
        LoadSettingsData();
        LoadGameData();
        Debug.Log("[DataManager] 초기화 및 데이터 로드 완료");
    }

    public void SaveGameData()
    {
        // 💡 인벤토리 상태를 저장 직전에 동기화
        SyncInventoryToSaveData();

        // 💡 저장 시점 기록
        CurrentData.lastSaveTimeTicks = DateTime.UtcNow.Ticks;

        string json = JsonUtility.ToJson(CurrentData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("[DataManager] 진행 데이터가 저장되었습니다.");
    }

    public void SaveSettingsData()
    {
        if (CurrentSettings == null) CurrentSettings = new SettingsData();
        string json = JsonUtility.ToJson(CurrentSettings);
        PlayerPrefs.SetString(SETTINGS_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("[DataManager] 환경 설정이 저장되었습니다.");
    }

    private void LoadSettingsData()
    {
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            string json = PlayerPrefs.GetString(SETTINGS_KEY);
            CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            CurrentSettings = new SettingsData();
        }
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            CurrentData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("<color=green>[DataManager] 기존 저장 데이터를 성공적으로 불러왔습니다.</color>");
        }
        else
        {
            CurrentData = new SaveData();
            SaveGameData();
            Debug.Log("<color=yellow>[DataManager] 새 게임 데이터를 생성했습니다.</color>");
        }
    }

    // ===== 인벤토리 동기화 =====

    /// <summary>
    /// 현재 InventoryManager의 런타임 데이터를 SaveData로 복사합니다.
    /// SaveGameData() 직전에 호출됩니다.
    /// </summary>
    private void SyncInventoryToSaveData()
    {
        if (InventoryManager.Instance == null) return;

        CurrentData.inventoryItems.Clear();
        var items = InventoryManager.Instance.inventoryItems;
        for (int i = 0; i < items.Count; i++)
        {
            CurrentData.inventoryItems.Add(new InventorySaveItem
            {
                ingredientID = items[i].data.ingredientID,
                amount = items[i].amount,
                remainingDays = items[i].remainingDays
            });
        }
    }

    /// <summary>
    /// 게임 시작 시 SaveData → InventoryManager로 인벤토리를 복원합니다.
    /// GameManager.InitializeSystems() 이후에 호출해야 합니다.
    /// </summary>
    public void RestoreInventory()
    {
        if (InventoryManager.Instance == null || CurrentData.inventoryItems.Count == 0) return;

        RecipeManager recipeManager = GameManager.Instance.recipeManager;
        if (recipeManager == null) return;

        for (int i = 0; i < CurrentData.inventoryItems.Count; i++)
        {
            InventorySaveItem saved = CurrentData.inventoryItems[i];
            IngredientData data = recipeManager.GetIngredientById(saved.ingredientID);
            if (data != null)
            {
                InventoryManager.Instance.AddIngredient(data, saved.amount, saved.remainingDays);
            }
        }

        Debug.Log($"<color=green>[DataManager] 인벤토리 {CurrentData.inventoryItems.Count}슬롯 복원 완료</color>");
    }
}