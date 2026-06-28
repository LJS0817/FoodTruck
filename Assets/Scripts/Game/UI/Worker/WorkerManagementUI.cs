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

        ChangeTab(0); // 기본적으로 Hired 탭을 먼저 보여줌
        RefreshWorkers();
        ClosePanel();
    }

    public void ChangeTab(int tabIndex)
    {
        if (_currentTabIndex == tabIndex) return;

        // 💡 모든 기존 탭 확실하게 닫기 (초기 시작 시 Editor에서 켜져 있던 탭이 겹치는 현상 방지)
        if (hiredWorkersGroup != null)
        {
            hiredWorkersGroup.alpha = 0f;
            hiredWorkersGroup.interactable = false;
            hiredWorkersGroup.blocksRaycasts = false;
        }
        if (recruitmentPoolGroup != null)
        {
            recruitmentPoolGroup.alpha = 0f;
            recruitmentPoolGroup.interactable = false;
            recruitmentPoolGroup.blocksRaycasts = false;
        }

        _currentTabIndex = tabIndex;

        // 새 탭 열기 (우선 투명하게 둔 상태로 컨텐츠 교체)
        if (_currentTabIndex == 0 && hiredWorkersGroup != null)
        {
            hiredWorkersGroup.interactable = true;
            hiredWorkersGroup.blocksRaycasts = true;
            if (scrollRect != null) scrollRect.content = hiredWorkersContent;
        }
        else if (_currentTabIndex == 1 && recruitmentPoolGroup != null)
        {
            recruitmentPoolGroup.interactable = true;
            recruitmentPoolGroup.blocksRaycasts = true;
            if (scrollRect != null) scrollRect.content = recruitmentPoolContent;
        }

        // 컨텐츠 갱신 후, 크기를 먼저 맞추고 화면에 표시하기 위해 RefreshWorkers 호출
        RefreshWorkers();

        // 갱신 및 크기 조절이 끝난 뒤 화면에 표시 (깜빡임/화면 밖 삐져나옴 방지)
        if (_currentTabIndex == 0 && hiredWorkersGroup != null)
            hiredWorkersGroup.alpha = 1f;
        else if (_currentTabIndex == 1 && recruitmentPoolGroup != null)
            recruitmentPoolGroup.alpha = 1f;
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
        var hiredWorkers = workerManager.HiredWorkers;

        // 부족한 만큼 생성 (새로 고용되었을 때)
        while (_spawnedHiredSlots.Count < hiredWorkers.Count)
        {
            WorkerHiredSlotUI newSlot = Instantiate(hiredSlotPrefab, hiredWorkersContent);
            _spawnedHiredSlots.Add(newSlot);
        }

        // 남는 만큼 파괴 (해고되었을 때)
        while (_spawnedHiredSlots.Count > hiredWorkers.Count)
        {
            int lastIndex = _spawnedHiredSlots.Count - 1;
            WorkerHiredSlotUI slotToDestroy = _spawnedHiredSlots[lastIndex];
            slotToDestroy.gameObject.SetActive(false);
            Destroy(slotToDestroy.gameObject);
            _spawnedHiredSlots.RemoveAt(lastIndex);
        }

        // 기존 슬롯 갱신
        for (int i = 0; i < hiredWorkers.Count; i++)
        {
            _spawnedHiredSlots[i].SetupSlot(hiredWorkers[i], this);
        }

        // 2. 알바생 채용 후보 탭 갱신 (오브젝트 풀링 방식 적용)
        var pool = workerManager.RecruitmentPool;

        // 부족한 만큼 생성 (최대 개수만큼 한 번만 생성됨)
        while (_spawnedRecruitSlots.Count < pool.Count)
        {
            WorkerRecruitmentSlotUI newSlot = Instantiate(recruitmentSlotPrefab, recruitmentPoolContent);
            _spawnedRecruitSlots.Add(newSlot);
        }

        // 갱신 및 오브젝트 풀링(SetActive)
        for (int i = 0; i < _spawnedRecruitSlots.Count; i++)
        {
            if (i < pool.Count)
            {
                _spawnedRecruitSlots[i].gameObject.SetActive(true);
                _spawnedRecruitSlots[i].SetupSlot(pool[i], this);
            }
            else
            {
                _spawnedRecruitSlots[i].gameObject.SetActive(false); // 풀링: 보이지 않게 처리
            }
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

        // 스크롤 위치 복구 및 화면 밖 표시(깜빡임) 방지를 위한 레이아웃 즉시 강제 갱신
        if (scrollRect != null)
        {
            if (hiredWorkersContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(hiredWorkersContent);
            if (recruitmentPoolContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(recruitmentPoolContent);
            
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
