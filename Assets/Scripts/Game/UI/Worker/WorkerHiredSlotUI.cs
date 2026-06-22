using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerHiredSlotUI : MonoBehaviour
{
    [Header("Hired Worker Specific UI")]
    public TMP_Text dailySalaryText;
    
    public Button fireButton;
    public Button upgradeButton;
    public TMP_Text upgradeCostText;
    public Button incentiveButton;
    public TMP_Text incentiveText;

    [Header("Resting UI")]
    public GameObject restingIndicator;
    public TMP_Text restingTimerText;

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
        baseUI.nameText.text = $"{worker.workerName} (Lv.{worker.currentLevel} / {worker.maxLevel})";
        
        int actualSalary = Mathf.RoundToInt(worker.baseDailySalary * worker.incentiveMultiplier);
        if (dailySalaryText != null) dailySalaryText.text = $"일급: {actualSalary:N0}원";

        if (upgradeCostText != null) upgradeCostText.text = $"{GetUpgradeCost():N0}원";
        if (incentiveText != null) incentiveText.text = worker.incentiveMultiplier > 1.1f ? "인센티브 종료" : "인센티브 1.5배";

        if (fireButton != null)
        {
            fireButton.onClick.RemoveAllListeners();
            fireButton.onClick.AddListener(OnClickFire);
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = worker.currentLevel < worker.maxLevel;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        if (incentiveButton != null)
        {
            incentiveButton.onClick.RemoveAllListeners();
            incentiveButton.onClick.AddListener(OnClickIncentive);
        }
    }

    private int GetUpgradeCost()
    {
        return _currentWorker.hiringCost * _currentWorker.currentLevel;
    }

    private void OnClickFire()
    {
        if (_currentWorker != null)
        {
            WorkerManager.Instance.FireWorker(_currentWorker);
            _parentUI.RefreshWorkers();
        }
    }

    private void OnClickUpgrade()
    {
        if (_currentWorker != null)
        {
            if (WorkerManager.Instance.UpgradeWorker(_currentWorker, GetUpgradeCost()))
            {
                _parentUI.RefreshWorkers();
            }
        }
    }

    private void OnClickIncentive()
    {
        if (_currentWorker != null)
        {
            float newMult = _currentWorker.incentiveMultiplier > 1.1f ? 1.0f : 1.5f;
            WorkerManager.Instance.SetWorkerIncentive(_currentWorker, newMult);
            _parentUI.RefreshWorkers();
        }
    }

    private void Update()
    {
        if (_currentWorker != null)
        {
            if (_currentWorker.isResting)
            {
                if (restingIndicator != null && !restingIndicator.activeSelf)
                    restingIndicator.SetActive(true);

                if (restingTimerText != null)
                {
                    if (!restingTimerText.gameObject.activeSelf)
                        restingTimerText.gameObject.SetActive(true);
                    
                    int minutes = Mathf.FloorToInt(_currentWorker.restTimer / 60f);
                    int seconds = Mathf.FloorToInt(_currentWorker.restTimer % 60f);
                    restingTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
            }
            else
            {
                if (restingIndicator != null && restingIndicator.activeSelf)
                    restingIndicator.SetActive(false);
                if (restingTimerText != null && restingTimerText.gameObject.activeSelf)
                    restingTimerText.gameObject.SetActive(false);
            }
        }
    }
}
