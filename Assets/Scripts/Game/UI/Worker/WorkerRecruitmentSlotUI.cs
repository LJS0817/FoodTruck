using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerRecruitmentSlotUI : MonoBehaviour
{
    [Header("Recruitment Specific UI")]
    public TMP_Text hireCostText;
    public Button hireButton;
    public TMP_Text dailySalaryText;

    private WorkerData _currentWorker;
    private WorkerManagementUI _parentUI;

    public void SetupSlot(WorkerData worker, WorkerManagementUI parentUI)
    {
        _currentWorker = worker;
        _parentUI = parentUI;

        var baseUI = GetComponent<WorkerSlotBaseUI>();
        if (baseUI != null)
        {
            baseUI.SetupBaseSlot(worker);
        }

        if (hireCostText != null) hireCostText.text = $"{worker.hiringCost:N0}원";
        if (dailySalaryText != null) dailySalaryText.text = $"일급: {worker.baseDailySalary:N0}원";

        if (hireButton != null)
        {
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(OnClickHire);
        }
    }

    private void OnClickHire()
    {
        if (_currentWorker != null && WorkerManager.Instance.HireWorker(_currentWorker))
        {
            _parentUI.RefreshWorkers();
        }
    }
}
