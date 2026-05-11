using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achievements Database")]
    public List<AchievementData> allAchievements;

    public event System.Action<AchievementData> OnTitleUnlocked;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 정산 시점에 누적 데이터 업데이트
        if (SettlementManager.Instance != null)
        {
            SettlementManager.Instance.OnSettlementUpdated += UpdateCumulativeStats;
        }
    }

    private void OnDestroy()
    {
        if (SettlementManager.Instance != null)
        {
            SettlementManager.Instance.OnSettlementUpdated -= UpdateCumulativeStats;
        }
    }

    private void UpdateCumulativeStats()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;

        var currentData = DataManager.Instance.CurrentData;
        
        // 정산 데이터 더하기
        currentData.totalCustomersServed += SettlementManager.Instance.GetSatisfiedCustomerCount();
        currentData.totalMoneyEarned += SettlementManager.Instance.GetDailySales();

        CheckAchievements();
    }

    // 조건 달성 여부를 검사하여 칭호를 해금합니다.
    public void CheckAchievements()
    {
        if (allAchievements == null || DataManager.Instance == null) return;
        var currentData = DataManager.Instance.CurrentData;

        foreach (var ach in allAchievements)
        {
            // 이미 해금했으면 패스
            if (currentData.unlockedTitles.Contains(ach.titleID)) continue;

            bool isUnlocked = false;
            switch (ach.type)
            {
                case AchievementType.TotalCustomers:
                    if (currentData.totalCustomersServed >= ach.requirement) isUnlocked = true;
                    break;
                case AchievementType.TotalMoneyEarned:
                    if (currentData.totalMoneyEarned >= ach.requirement) isUnlocked = true;
                    break;
            }

            if (isUnlocked)
            {
                currentData.unlockedTitles.Add(ach.titleID);
                Debug.Log($"<color=yellow>[업적 달성!] 새로운 칭호 획득: {ach.titleName}</color>");
                OnTitleUnlocked?.Invoke(ach);
            }
        }
    }

    // 유저가 칭호를 장착할 때 호출
    public void EquipTitle(string titleID)
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;
        
        if (DataManager.Instance.CurrentData.unlockedTitles.Contains(titleID))
        {
            DataManager.Instance.CurrentData.equippedTitleID = titleID;
            Debug.Log($"[칭호] 현재 장착된 칭호: {titleID}");
        }
    }

    // 장착된 칭호의 버프 혜택 가져오기 (예: 팁 보너스)
    public float GetEquippedTitleTipMultiplier()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return 1.0f;
        string equipped = DataManager.Instance.CurrentData.equippedTitleID;
        if (string.IsNullOrEmpty(equipped)) return 1.0f;

        AchievementData data = allAchievements.Find(a => a.titleID == equipped);
        if (data != null) return data.extraTipMultiplier;
        
        return 1.0f;
    }
}
