using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Managers")]
    public DataManager dataManager;
    public RecipeManager recipeManager;
    GameTimeManager _timeManager;

    private void Awake()
    {
        _timeManager = GetComponent<GameTimeManager>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeMobileSettings(); // 모바일 세팅 먼저 실행
        InitializeSystems();
    }

    // 모바일 기기에 맞춘 필수 환경 설정
    private void InitializeMobileSettings()
    {
        // 1. 기기 방향을 가로 모드(Landscape)로 강제 고정
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // 2. 발열 및 배터리 방어를 위한 프레임 고정 (60프레임이 가장 부드럽고 적당합니다)
        Application.targetFrameRate = 60;

        // 3. 타이쿤 영업 중 화면이 자동으로 꺼지는 현상 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("[GameManager] 모바일 가로 모드 최적화 완료");
    }

    private void InitializeSystems()
    {
        if (dataManager != null) dataManager.Initialize();
        if (recipeManager != null) recipeManager.InitializeRecipeBook();
        _timeManager.Initialize();
        if (DayCycleManager.Instance == null) gameObject.AddComponent<DayCycleManager>();
        if (MenuManager.Instance == null) gameObject.AddComponent<MenuManager>();
        if (SettlementManager.Instance == null) gameObject.AddComponent<SettlementManager>();
        if (WeatherTrendManager.Instance == null) gameObject.AddComponent<WeatherTrendManager>();
        if (PlayerStaminaManager.Instance == null) gameObject.AddComponent<PlayerStaminaManager>();
        if (ReputationManager.Instance == null) gameObject.AddComponent<ReputationManager>();
        
        // 💡 신규 시스템들 초기화
        if (WorkerManager.Instance == null) gameObject.AddComponent<WorkerManager>();
        if (DistrictManager.Instance == null) gameObject.AddComponent<DistrictManager>();
        if (PlayerUpgradeManager.Instance == null) gameObject.AddComponent<PlayerUpgradeManager>();
        if (MarketingManager.Instance == null) gameObject.AddComponent<MarketingManager>();
        if (BankManager.Instance == null) gameObject.AddComponent<BankManager>();
        if (VIPLoyaltyManager.Instance == null) gameObject.AddComponent<VIPLoyaltyManager>();
        if (RandomEventManager.Instance == null) gameObject.AddComponent<RandomEventManager>();
        if (HygieneManager.Instance == null) gameObject.AddComponent<HygieneManager>();
        if (AchievementManager.Instance == null) gameObject.AddComponent<AchievementManager>();

        // 💡 모든 시스템 초기화 후 저장된 데이터 복원
        if (dataManager != null && dataManager.CurrentData != null)
        {
            dataManager.RestoreInventory();
            
            WorkerManager.Instance.LoadFromSaveData(dataManager.CurrentData.hiredWorkerIDs);
            DistrictManager.Instance.LoadFromSaveData(dataManager.CurrentData.unlockedDistrictIDs, dataManager.CurrentData.currentDistrictID);
            PlayerUpgradeManager.Instance.LoadFromSaveData(dataManager.CurrentData.upgrades);
        }
    }
}