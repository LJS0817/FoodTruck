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
    
    // 상태 저장 추가
    public IngredientState state;
    public ProcessType processType;
    public ItemGrade grade;
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

/// <summary>재료 상자(맵 상에 배치된) 상태 저장</summary>
[Serializable]
public class IngredientBoxSaveData
{
    public int index;
    public bool isTempBox;
    public int ingredientID;
    public int amount;
    public List<int> storedItemDays = new List<int>();
    public IngredientState state;
    public ProcessType processType;
    public float qualityScore;
}

/// <summary>진행 중인 백그라운드 가공 태스크 저장</summary>
[Serializable]
public class ProcessTaskSaveData
{
    public EquipmentType equipmentType;
    public int ingredientID;
    public IngredientState state;
    public ProcessType processType;
    public ItemGrade grade;
    public ProcessType targetProcessType;
    public float elapsedTime;
    public float qualityScore;
    public ProcessState processState;
}

/// <summary>대기 중인 손님 저장</summary>
[Serializable]
public class CustomerSaveData
{
    public string customerName;
    public float currentPatience;
}

/// <summary>보유 장비 및 레벨/장착 상태 저장</summary>
[Serializable]
public class EquipmentSaveData
{
    public EquipmentType type;
    public int level;
    public bool isEquipped;
}

/// <summary>활성화된 주문표 저장</summary>
[Serializable]
public class OrderSaveData
{
    public int customerIndex; // activeCustomers 리스트에서의 인덱스
    public string orderedFoodName;
}

[Serializable]
public class SaveData
{
    public int currentMoney = 0;
    public int currentDay = 1;
    public float currentTotalSeconds = 0f; // 💡 장사 진행 시간 (실시간 저장용)
    public DayPhase currentDayPhase = DayPhase.Preparation; // 💡 장사 단계 저장
    public int reputation = 30; // 💡 평판 (기본값 30)
    public float currentStamina = 100f;
    public float currentHygiene = 100f;

    public List<RecipeSaveData> unlockedRecipes = new List<RecipeSaveData>();
    // 💡 유저가 새롭게 연구하여 만들어낸 커스텀 레시피 목록
    public List<CustomRecipeData> customRecipes = new List<CustomRecipeData>();

    // 💡 인벤토리 저장
    public List<InventorySaveItem> inventoryItems = new List<InventorySaveItem>();

    // 💡 보유 장비 저장 (레벨, 장착 여부 포함)
    public List<EquipmentSaveData> equipmentList = new List<EquipmentSaveData>();

    // 💡 웨이팅존 아이템 저장 (asset 이름)
    public List<string> waitingZoneItemNames = new List<string>();

    // 💡 실시간 복구(Mid-Day Save)를 위한 추가 데이터 모음
    public List<IngredientBoxSaveData> activeBoxes = new List<IngredientBoxSaveData>();
    public List<ProcessTaskSaveData> activeProcessTasks = new List<ProcessTaskSaveData>();
    public List<CustomerSaveData> activeCustomers = new List<CustomerSaveData>();
    public List<OrderSaveData> activeOrders = new List<OrderSaveData>();

    // 💡 현재 영업 중인 메뉴 리스트 보존
    public List<string> todayMenuRecipes = new List<string>();

    // 💡 냄비(조리 공간) 복구용 데이터
    public List<int> cookingPotIngredientIDs = new List<int>();
    public int cookingPotPremiumCount = 0;

    // 💡 일별 기록 히스토리
    public List<DailyRecord> dailyHistory = new List<DailyRecord>();

    // 💡 알바생 데이터 전체 저장 (동적 생성 지원)
    public List<WorkerData> hiredWorkers = new List<WorkerData>();
    public List<WorkerData> recruitmentPool = new List<WorkerData>();
    public int lastWorkerRefreshDay = 0;
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
        SyncEquipmentToSaveData();
        SyncStaminaAndHygieneToSaveData();
        SyncTransientStateToSaveData();

        // 💡 저장 시점 기록
        CurrentData.lastSaveTimeTicks = DateTime.UtcNow.Ticks;

        string json = JsonUtility.ToJson(CurrentData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("[DataManager] 진행 데이터가 저장되었습니다.");
    }

    private void SyncEquipmentToSaveData()
    {
        if (EquipmentStoreManager.Instance != null)
        {
            EquipmentStoreManager.Instance.SaveToSaveData(CurrentData.equipmentList);
        }
    }

    public void RestoreEquipment()
    {
        if (EquipmentStoreManager.Instance != null && CurrentData.equipmentList != null)
        {
            EquipmentStoreManager.Instance.RestoreFromSaveData(CurrentData.equipmentList);
            Debug.Log($"<color=green>[DataManager] 보유 장비 {CurrentData.equipmentList.Count}개 복원 완료</color>");
        }
    }

    private void SyncTransientStateToSaveData()
    {
        if (GameTimeManager.Instance != null)
        {
            CurrentData.currentTotalSeconds = GameTimeManager.Instance.TotalSeconds;
        }

        if (DayCycleManager.Instance != null)
        {
            CurrentData.currentDayPhase = DayCycleManager.Instance.CurrentPhase;
        }

        if (IngredientManager.Instance != null)
        {
            CurrentData.activeBoxes.Clear();
            IngredientManager.Instance.SaveBoxStates(CurrentData.activeBoxes);
        }

        if (ProcessManager.Instance != null)
        {
            CurrentData.activeProcessTasks.Clear();
            ProcessManager.Instance.SaveTaskStates(CurrentData.activeProcessTasks);
        }

        if (OrderManager.Instance != null && CustomerManager.Instance != null)
        {
            CurrentData.activeCustomers.Clear();
            CurrentData.activeOrders.Clear();
            OrderManager.Instance.SaveOrderStates(CurrentData.activeOrders, CurrentData.activeCustomers);
        }

        if (CookingManager.Instance != null)
        {
            CookingManager.Instance.SavePotState(CurrentData.cookingPotIngredientIDs, out CurrentData.cookingPotPremiumCount);
        }

        if (MenuManager.Instance != null)
        {
            CurrentData.todayMenuRecipes.Clear();
            foreach (var recipe in MenuManager.Instance.GetAvailableRecipes())
            {
                if (recipe != null) CurrentData.todayMenuRecipes.Add(recipe.foodName);
            }
        }
    }

    public void RestoreTransientState()
    {
        if (CurrentData == null) return;

        // IngredientBox 복원
        if (IngredientManager.Instance != null && CurrentData.activeBoxes != null)
        {
            IngredientManager.Instance.RestoreBoxStates(CurrentData.activeBoxes);
        }

        // ProcessTask 복원
        if (ProcessManager.Instance != null && CurrentData.activeProcessTasks != null)
        {
            ProcessManager.Instance.RestoreTaskStates(CurrentData.activeProcessTasks);
        }

        // 냄비 복원
        if (CookingManager.Instance != null && CurrentData.cookingPotIngredientIDs != null)
        {
            CookingManager.Instance.RestorePotState(CurrentData.cookingPotIngredientIDs, CurrentData.cookingPotPremiumCount);
        }

        // 메뉴 복원
        if (MenuManager.Instance != null && CurrentData.todayMenuRecipes != null && CurrentData.todayMenuRecipes.Count > 0)
        {
            List<FoodData> restoredRecipes = new List<FoodData>();
            foreach (var recipeName in CurrentData.todayMenuRecipes)
            {
                FoodData recipe = GameManager.Instance.recipeManager.GetRecipeByName(recipeName);
                if (recipe != null) restoredRecipes.Add(recipe);
            }
            if (restoredRecipes.Count > 0)
            {
                MenuManager.Instance.SetTodayMenu(restoredRecipes);
            }
        }

        // (선택) 손님/주문 복원 등 추후 확장 가능
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
                remainingDays = items[i].remainingDays,
                state = items[i].state,
                processType = items[i].processType,
                grade = items[i].grade
            });
        }
    }

    private void SyncStaminaAndHygieneToSaveData()
    {
        if (PlayerStaminaManager.Instance != null)
            CurrentData.currentStamina = PlayerStaminaManager.Instance.CurrentStamina;
        if (HygieneManager.Instance != null)
            CurrentData.currentHygiene = HygieneManager.Instance.currentHygiene;
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
                InventoryManager.Instance.AddIngredient(data, saved.amount, saved.remainingDays, saved.state, saved.processType, saved.grade);
            }
        }

        Debug.Log($"<color=green>[DataManager] 인벤토리 {CurrentData.inventoryItems.Count}슬롯 복원 완료</color>");
    }

    // ===== 앱 라이프사이클 (안전망 저장) =====

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && CurrentData != null)
        {
            Debug.Log("<color=orange>[DataManager] 백그라운드 진입 감지: 긴급 저장 수행</color>");
            SaveGameData();
        }
    }

    private void OnApplicationQuit()
    {
        if (CurrentData != null)
        {
            Debug.Log("<color=orange>[DataManager] 앱 종료 감지: 긴급 저장 수행</color>");
            SaveGameData();
        }
    }
}