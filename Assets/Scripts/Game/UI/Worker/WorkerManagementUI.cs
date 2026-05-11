using System.Collections.Generic;
using UnityEngine;

public class WorkerManagementUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject workerManagementPanel;
    [SerializeField] private Transform slotsParent; // 스크롤 뷰의 Content 트랜스폼
    [SerializeField] private WorkerManagementSlotUI slotPrefab;

    private List<WorkerManagementSlotUI> _spawnedSlots = new List<WorkerManagementSlotUI>();

    private void Start()
    {
        ClosePanel();
    }

    /// <summary>
    /// 직원 관리 창 열기
    /// </summary>
    public void OpenPanel()
    {
        if (workerManagementPanel != null)
        {
            workerManagementPanel.SetActive(true);
            RefreshHiredWorkers();
        }
    }

    /// <summary>
    /// 직원 관리 창 닫기
    /// </summary>
    public void ClosePanel()
    {
        if (workerManagementPanel != null)
            workerManagementPanel.SetActive(false);
    }

    /// <summary>
    /// 현재 고용된 알바생 목록을 불러와 UI 슬롯을 생성/갱신합니다.
    /// </summary>
    public void RefreshHiredWorkers()
    {
        if (WorkerManager.Instance == null) return;

        // 1. 기존 슬롯들 삭제 (오브젝트 풀링을 쓰면 더 좋지만 여기선 직관성을 위해 Destroy)
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            if (_spawnedSlots[i] != null) Destroy(_spawnedSlots[i].gameObject);
        }
        _spawnedSlots.Clear();

        // 2. 현재 고용 중인 알바생 리스트 가져오기
        var hiredWorkers = WorkerManager.Instance.HiredWorkers;

        // 3. 슬롯 생성 및 데이터 세팅
        for (int i = 0; i < hiredWorkers.Count; i++)
        {
            WorkerManagementSlotUI newSlot = Instantiate(slotPrefab, slotsParent);
            newSlot.SetupSlot(hiredWorkers[i], this);
            
            _spawnedSlots.Add(newSlot);
        }
    }
}
