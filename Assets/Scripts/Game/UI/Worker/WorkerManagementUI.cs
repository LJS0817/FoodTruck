using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerManagementUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup workerManagementPanel;
    
    [Header("Scroll View Components")]
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Hired Workers Tab")]
    [SerializeField] private RectTransform hiredWorkersContent;
    private CanvasGroup hiredWorkersGroup;
    [SerializeField] private Button hiredTabButton;

    [Header("Recruitment Pool Tab")]
    [SerializeField] private RectTransform recruitmentPoolContent;
    private CanvasGroup recruitmentPoolGroup;
    [SerializeField] private Button recruitmentTabButton;

    [Header("UI Prefabs & Controls")]
    [SerializeField] private WorkerHiredSlotUI hiredSlotPrefab;
    [SerializeField] private WorkerRecruitmentSlotUI recruitmentSlotPrefab;

    [Header("Refresh UI")]
    [SerializeField] private Button refreshPoolButton;
    [SerializeField] private TMP_Text refreshCostText;

    private List<WorkerHiredSlotUI> _spawnedHiredSlots = new List<WorkerHiredSlotUI>();
    private List<WorkerRecruitmentSlotUI> _spawnedRecruitSlots = new List<WorkerRecruitmentSlotUI>();
    
    private int _currentTabIndex = -1; // 0: Hired, 1: Recruitment

    private void Start()
    {
        if (hiredWorkersContent != null) hiredWorkersGroup = hiredWorkersContent.GetComponent<CanvasGroup>();
        if (recruitmentPoolContent != null) recruitmentPoolGroup = recruitmentPoolContent.GetComponent<CanvasGroup>();

        if (refreshPoolButton != null)
        {
            refreshPoolButton.onClick.AddListener(OnClickRefreshButton);
        }
        
        if (hiredTabButton != null) hiredTabButton.onClick.AddListener(() => ChangeTab(0));
        if (recruitmentTabButton != null) recruitmentTabButton.onClick.AddListener(() => ChangeTab(1));

        RefreshWorkers();
        ClosePanel();
    }

    public void ChangeTab(int tabIndex)
    {
        if (_currentTabIndex == tabIndex) return;

        // 기존 탭 닫기
        if (_currentTabIndex == 0 && hiredWorkersGroup != null)
        {
            hiredWorkersGroup.alpha = 0f;
            hiredWorkersGroup.interactable = false;
            hiredWorkersGroup.blocksRaycasts = false;
        }
        else if (_currentTabIndex == 1 && recruitmentPoolGroup != null)
        {
            recruitmentPoolGroup.alpha = 0f;
            recruitmentPoolGroup.interactable = false;
            recruitmentPoolGroup.blocksRaycasts = false;
        }

        _currentTabIndex = tabIndex;

        // 새 탭 열기
        if (_currentTabIndex == 0 && hiredWorkersGroup != null)
        {
            hiredWorkersGroup.alpha = 1f;
            hiredWorkersGroup.interactable = true;
            hiredWorkersGroup.blocksRaycasts = true;
            if (scrollRect != null) scrollRect.content = hiredWorkersContent;
        }
        else if (_currentTabIndex == 1 && recruitmentPoolGroup != null)
        {
            recruitmentPoolGroup.alpha = 1f;
            recruitmentPoolGroup.interactable = true;
            recruitmentPoolGroup.blocksRaycasts = true;
            if (scrollRect != null) scrollRect.content = recruitmentPoolContent;
        }

        RefreshWorkers();
    }

    /// <summary>
    /// 직원 관리 창 열기
    /// </summary>
    public void OpenPanel()
    {
        if (workerManagementPanel != null)
        {
            workerManagementPanel.alpha = 1f;
            workerManagementPanel.interactable = true;
            workerManagementPanel.blocksRaycasts = true;
            
            // 패널을 열 때 기본적으로 채용 대기 탭(1)을 보여줌 (이때 Hired 탭은 alpha 0이 됨)
            ChangeTab(1);
        }
    }

    /// <summary>
    /// 직원 관리 창 닫기
    /// </summary>
    public void ClosePanel()
    {
        workerManagementPanel.alpha = 0f;
        workerManagementPanel.interactable = false;
        workerManagementPanel.blocksRaycasts = false;
    }

    /// <summary>
    /// 현재 고용된 알바생과 채용 대기 중인 알바생 목록을 모두 불러와 UI 슬롯을 생성/갱신합니다.
    /// </summary>
    public void RefreshWorkers()
    {
        if (WorkerManager.Instance == null) return;

        var workerManager = WorkerManager.Instance;
        
        // 새로고침 전 스크롤 위치 저장
        float prevScrollPos = scrollRect != null ? scrollRect.verticalNormalizedPosition : 1f;

        // 1. 내 알바생 탭 갱신
        for (int i = 0; i < _spawnedHiredSlots.Count; i++)
        {
            if (_spawnedHiredSlots[i] != null) Destroy(_spawnedHiredSlots[i].gameObject);
        }
        _spawnedHiredSlots.Clear();

        var hiredWorkers = workerManager.HiredWorkers;
        for (int i = 0; i < hiredWorkers.Count; i++)
        {
            WorkerHiredSlotUI newSlot = Instantiate(hiredSlotPrefab, hiredWorkersContent);
            newSlot.SetupSlot(hiredWorkers[i], this);
            _spawnedHiredSlots.Add(newSlot);
        }

        // 2. 알바생 채용 후보 탭 갱신
        for (int i = 0; i < _spawnedRecruitSlots.Count; i++)
        {
            if (_spawnedRecruitSlots[i] != null) Destroy(_spawnedRecruitSlots[i].gameObject);
        }
        _spawnedRecruitSlots.Clear();

        var pool = workerManager.RecruitmentPool;
        for (int i = 0; i < pool.Count; i++)
        {
            WorkerRecruitmentSlotUI newSlot = Instantiate(recruitmentSlotPrefab, recruitmentPoolContent);
            newSlot.SetupSlot(pool[i], this);
            _spawnedRecruitSlots.Add(newSlot);
        }

        // 4. 수동 갱신 비용 텍스트 업데이트
        if (refreshCostText != null)
        {
            int cost = workerManager.GetRefreshCost();
            if (cost == 0)
                refreshCostText.text = "무료 갱신 (1일 1회)";
            else
                refreshCostText.text = $"{cost:N0}원";
        }

        // 스크롤 위치 복구
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = prevScrollPos;
        }
    }

    private void OnClickRefreshButton()
    {
        if (WorkerManager.Instance != null)
        {
            if (WorkerManager.Instance.ManualRefreshRecruitmentPool())
            {
                RefreshWorkers();
            }
        }
    }
}
