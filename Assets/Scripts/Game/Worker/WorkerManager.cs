using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [Header("Catalog")]
    public List<WorkerData> allWorkers;

    // 현재 고용된 알바생들
    private List<WorkerData> _hiredWorkers = new List<WorkerData>();

    public IReadOnlyList<WorkerData> HiredWorkers => _hiredWorkers;

    private void Awake()
    {
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += PayDailySalaries;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= PayDailySalaries;
        }
    }

    // ===== 고용 시스템 =====

    public bool HireWorker(WorkerData worker)
    {
        if (_hiredWorkers.Contains(worker)) return false;

        if (PlayerManager.Instance.SpendMoney(worker.hiringCost))
        {
            _hiredWorkers.Add(worker);
            // 즉시 지출로 기록 (고용비)
            SettlementManager.Instance?.AddExpense(worker.hiringCost);
            Debug.Log($"<color=cyan>[알바생] {worker.workerName} 고용 완료!</color>");

            // SaveData 갱신
            SyncToSaveData();
            return true;
        }
        return false;
    }

    public void FireWorker(WorkerData worker)
    {
        if (_hiredWorkers.Remove(worker))
        {
            SyncToSaveData();
            Debug.Log($"<color=orange>[알바생] {worker.workerName} 해고됨.</color>");
        }
    }

    private void PayDailySalaries()
    {
        int totalSalary = 0;
        for (int i = 0; i < _hiredWorkers.Count; i++)
        {
            totalSalary += _hiredWorkers[i].dailySalary;
        }

        if (totalSalary > 0)
        {
            // 돈이 부족하면 적자(마이너스)로 전환됨 (PlayerManager에서 허용한다고 가정)
            PlayerManager.Instance.SpendMoney(totalSalary);
            SettlementManager.Instance?.AddExpense(totalSalary);
            Debug.Log($"<color=red>[알바생 일급] 알바생 { _hiredWorkers.Count}명의 일급 총 {totalSalary}원이 차감되었습니다.</color>");
        }
    }

    // ===== 능력치 제공 API =====

    public float GetAbilityTotalValue(WorkerAbility targetAbility)
    {
        float total = 0f;
        for (int i = 0; i < _hiredWorkers.Count; i++)
        {
            if (_hiredWorkers[i].ability == targetAbility)
            {
                total += _hiredWorkers[i].abilityValue;
            }
        }
        return total;
    }

    // ===== 저장 연동 =====

    public void LoadFromSaveData(List<int> hiredWorkerIDs)
    {
        _hiredWorkers.Clear();
        for (int i = 0; i < hiredWorkerIDs.Count; i++)
        {
            int id = hiredWorkerIDs[i];
            WorkerData worker = null;
            for (int j = 0; j < allWorkers.Count; j++)
            {
                if (allWorkers[j].workerID == id)
                {
                    worker = allWorkers[j];
                    break;
                }
            }
            if (worker != null) _hiredWorkers.Add(worker);
        }
    }

    private void SyncToSaveData()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;
        
        List<int> ids = new List<int>();
        foreach (var worker in _hiredWorkers) ids.Add(worker.workerID);
        
        DataManager.Instance.CurrentData.hiredWorkerIDs = ids;
    }
}
