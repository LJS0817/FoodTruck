using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerManagementSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text abilityDescText;
    public TMP_Text dailySalaryText;
    public Button fireButton;

    private WorkerData _currentWorker;
    private WorkerManagementUI _parentUI;

    public void SetupSlot(WorkerData worker, WorkerManagementUI parentUI)
    {
        _currentWorker = worker;
        _parentUI = parentUI;

        if (iconImage != null) iconImage.sprite = worker.workerIcon;
        if (nameText != null) nameText.text = worker.workerName;
        
        // 능력치 포맷팅 (예: 인내심 10% 증가)
        string abilityStr = $"{worker.ability} +{Mathf.RoundToInt(worker.abilityValue * 100)}%";
        if (abilityDescText != null) abilityDescText.text = abilityStr;
        
        if (dailySalaryText != null) dailySalaryText.text = $"일급: {worker.dailySalary:N0}원";

        // 기존 리스너 제거 후 새로 등록 (메모리 누수 방지)
        if (fireButton != null)
        {
            fireButton.onClick.RemoveAllListeners();
            fireButton.onClick.AddListener(OnClickFire);
        }
    }

    private void OnClickFire()
    {
        if (_currentWorker != null)
        {
            // 💡 해고 로직 호출
            WorkerManager.Instance.FireWorker(_currentWorker);
            
            // UI 갱신 (부모에게 새로고침 요청)
            _parentUI.RefreshHiredWorkers();
        }
    }
}
