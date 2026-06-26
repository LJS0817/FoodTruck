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
        // 1. 기기 방향을 세로 모드(Portrait)로 강제 고정
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.Portrait;

        // 2. 발열 및 배터리 방어를 위한 프레임 고정 (60프레임이 가장 부드럽고 적당합니다)
        Application.targetFrameRate = 60;

        // 3. 타이쿤 영업 중 화면이 자동으로 꺼지는 현상 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("[GameManager] 모바일 세로 모드 최적화 완료");
    }

    private void InitializeSystems()
    {
        if (dataManager != null) dataManager.Initialize();
        if (recipeManager != null) recipeManager.InitializeRecipeBook();
        _timeManager.Initialize();
        // 💡 모든 매니저 컴포넌트들은 에디터 인스펙터에서 직접 씬이나 GameManager 프리팹에 달아두고 세팅값을 넣어야 합니다.
        // 스크립트로 AddComponent를 하면 인스펙터 설정값(프리팹, 리스트 등)이 비어있는 깡통 중복 매니저가 생겨 심각한 버그(NRE)를 유발하므로 모두 제거했습니다.

        // 💡 모든 시스템 초기화 후 저장된 데이터 복원
        if (dataManager != null && dataManager.CurrentData != null)
        {
            dataManager.RestoreInventory();
            
            WorkerManager.Instance.LoadFromSaveData(dataManager.CurrentData.hiredWorkers, dataManager.CurrentData.recruitmentPool, dataManager.CurrentData.lastWorkerRefreshDay);
            UpgradeManager.Instance.District.LoadFromSaveData(dataManager.CurrentData.unlockedDistrictIDs, dataManager.CurrentData.currentDistrictID);
            UpgradeManager.Instance.Upgrade.LoadFromSaveData(dataManager.CurrentData.upgrades);
        }
    }
}