using UnityEngine;
using UnityEngine.UI;

public class MainUINavigationController : MonoBehaviour
{
    [Header("UI Controllers")]
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private UpgradeUIController upgradeUI;
    [SerializeField] private StoreUIController storeUI;
    // [Removed] WorkerManagementUI is no longer used; hiring is handled via TruckManagementUIController
    [SerializeField] private RecipeUIController recipeUI;
    [SerializeField] private TruckManagementUIController truckManagementUI;

    [Header("Navigation Buttons (Optional)")]
    [Tooltip("순서대로: 0(인벤토리), 1(레시피), 2(홈), 3(트럭 관리), 4(마켓)")]
    [SerializeField] private Button[] navButtons;

    private int _currentTabIndex = -1;

    private void Start()
    {
        for(int i = 0; i < navButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            if (navButtons[i] != null)
            {
                navButtons[i].onClick.AddListener(() => SelectTab(index));
            }
        }
        // 기본적으로 2번(트럭 뷰, 인게임 화면) 탭을 활성화 상태로 시작
        SelectTab(2); 
    }

    /// <summary>
    /// 탭 선택 함수. UI 버튼의 OnClick 이벤트에 이 함수를 연결하고,
    /// 매개변수로 0~4의 인덱스를 전달하세요.
    /// 0: 인벤토리 / 1: 업그레이드 / 2: 트럭 뷰 / 3: 상점 / 4: 직원 관리
    /// </summary>
    public void SelectTab(int index)
    {
        if (_currentTabIndex == index) return;

        // 전환 중일 때는 탭 클릭 무시
        if (ViewManager.Instance != null && ViewManager.Instance.IsTransitioning) return;

        if (ViewManager.Instance != null)
        {
            // ViewManager에 만들어둔 Fade 이펙트 재사용
            ViewManager.Instance.PerformFadeTransition(() => {
                SwitchTabInternal(index);
            });
        }
        else
        {
            SwitchTabInternal(index);
        }
    }

    private void SwitchTabInternal(int index)
    {
        // Close all existing tabs
        CloseAllTabs();

        _currentTabIndex = index;
        bool isTruckView = false;

        switch (index)
        {
            case 0:
                if (inventoryUI != null) inventoryUI.OpenInventory();
                break;
            case 1:
                if (recipeUI != null) recipeUI.OpenUI();
                break;
            case 2:
                // Home / Truck View
                isTruckView = true;
                break;
            case 3:
                if (truckManagementUI != null) truckManagementUI.OpenDefault();
                break;
            case 4:
                if (storeUI != null) storeUI.OpenUI();
                break;
            default:
                isTruckView = true;
                break;
        }

        // Time scale handling
        Time.timeScale = isTruckView ? 1f : 0f;
        UpdateButtonStates(index);
    }

    /// <summary>
    /// 각 UI 스크립트의 닫기(Close) 메서드를 호출합니다.
    /// </summary>
    public void CloseAllTabs()
    {
        if (inventoryUI != null) inventoryUI.CloseInventory();
        if (upgradeUI != null) upgradeUI.CloseUI();
        if (storeUI != null) storeUI.CloseUI();
        if (truckManagementUI != null) truckManagementUI.CloseUI();
    }

    /// <summary>
    /// 활성화된 탭 버튼을 시각적으로 비활성화하여(Interactable = false)
    /// 유저가 현재 어떤 탭에 있는지 알 수 있게 합니다.
    /// </summary>
    private void UpdateButtonStates(int activeIndex)
    {
        if (navButtons == null || navButtons.Length == 0) return;
        
        for (int i = 0; i < navButtons.Length; i++)
        {
            if (navButtons[i] == null) continue;
            
            navButtons[i].interactable = (i != activeIndex);
        }
    }
}
