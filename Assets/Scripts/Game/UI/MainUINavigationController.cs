using UnityEngine;
using UnityEngine.UI;

public class MainUINavigationController : MonoBehaviour
{
    [Header("UI Controllers")]
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private UpgradeUIController upgradeUI;
    [SerializeField] private StoreUIController storeUI;
    [SerializeField] private WorkerManagementUI workerUI;

    [Header("Navigation Buttons (Optional)")]
    [Tooltip("순서대로: 0(인벤토리), 1(업그레이드), 2(트럭 뷰), 3(상점), 4(직원 관리)")]
    [SerializeField] private Button[] navButtons;

    private int _currentTabIndex = -1;

    private void Start()
    {
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

        // 다른 탭으로 이동하기 전에 기존에 열려있던 모든 관리 UI 닫기
        CloseAllTabs();

        _currentTabIndex = index;
        bool isTruckView = false;

        switch (index)
        {
            case 0:
                if (inventoryUI != null) inventoryUI.OpenInventory();
                break;
            case 1:
                if (upgradeUI != null) upgradeUI.OpenUI();
                break;
            case 2:
                // 트럭 뷰: 모든 탭이 닫혀 인게임 월드가 보이는 상태
                isTruckView = true;
                break;
            case 3:
                if (storeUI != null) storeUI.OpenUI();
                break;
            case 4:
                if (workerUI != null) workerUI.OpenPanel();
                break;
            default:
                isTruckView = true;
                break;
        }

        // 트럭 뷰(인게임 화면)일 때만 시간을 흐르게 하고, 
        // 관리 창(인벤토리, 상점 등)이 열려있을 때는 시간을 멈춥니다.
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
        if (workerUI != null) workerUI.ClosePanel();
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
