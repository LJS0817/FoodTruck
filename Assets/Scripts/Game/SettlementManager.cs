using System;
using System.Collections.Generic;
using UnityEngine;

// DailyRecord 클래스는 DataManager.cs에 정의되어 있습니다.

public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    [Header("Daily Stats")]
    private int dailySales = 0;
    private int dailyExpenses = 0;
    private int satisfiedCustomerCount = 0;
    private int premiumDishCount = 0;
    private int lostCustomerCount = 0; // 💡 화나서 돌아간 손님 수

    // 메뉴별 판매량 추적 (LINQ 없이 Dictionary 사용)
    private Dictionary<string, int> salesByMenu = new Dictionary<string, int>(16);

    // 일별 기록 히스토리 (최근 30일 유지)
    private const int MAX_HISTORY_DAYS = 30;
    private List<DailyRecord> _dailyHistory = new List<DailyRecord>(MAX_HISTORY_DAYS);

    public event Action OnSettlementUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted += OnNewDayStarted;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted -= OnNewDayStarted;
        }
    }

    // ===== Phase Events =====

    private void OnPhaseChanged(DayPhase phase)
    {
        if (phase == DayPhase.Settlement)
        {
            SaveDailyRecord();
            Debug.Log($"[Settlement] 매출: {dailySales:N0}원 | 지출: {dailyExpenses:N0}원 | 순이익: {GetNetProfit():N0}원 | 손님: {satisfiedCustomerCount}명 | 프리미엄: {premiumDishCount}개 | 인기 메뉴: {GetTopMenu()}");
            OnSettlementUpdated?.Invoke();
        }
    }

    private void OnNewDayStarted()
    {
        ResetDailyStats();
    }

    // ===== Record Writing =====

    /// <summary>매출 발생 시 호출 (CustomerLeaveState에서 호출)</summary>
    public void AddSales(int amount, bool isPremium, string menuName = "")
    {
        dailySales += amount;
        satisfiedCustomerCount++;
        if (isPremium) premiumDishCount++;

        // 메뉴별 판매량 집계
        if (!string.IsNullOrEmpty(menuName))
        {
            if (salesByMenu.ContainsKey(menuName))
                salesByMenu[menuName]++;
            else
                salesByMenu[menuName] = 1;
        }

        OnSettlementUpdated?.Invoke();
    }

    /// <summary>재료 구매 시 호출 (MarketManager.BuyListing에서 호출)</summary>
    public void AddExpense(int amount)
    {
        if (amount <= 0) return;
        dailyExpenses += amount;
        OnSettlementUpdated?.Invoke();
    }

    private void ResetDailyStats()
    {
        dailySales = 0;
        dailyExpenses = 0;
        satisfiedCustomerCount = 0;
        premiumDishCount = 0;
        lostCustomerCount = 0;
        salesByMenu.Clear();
        OnSettlementUpdated?.Invoke();
    }

    // ===== History =====

    private void SaveDailyRecord()
    {
        int day = GameTimeManager.Instance != null ? GameTimeManager.Instance.GetCurrentDay() : 0;

        DailyRecord record = new DailyRecord
        {
            day          = day,
            grossSales   = dailySales,
            expenses     = dailyExpenses,
            netProfit    = GetNetProfit(),
            customerCount = satisfiedCustomerCount,
            premiumCount = premiumDishCount,
            topMenu      = GetTopMenu()
        };

        _dailyHistory.Add(record);

        // 최대 보관 일수 초과 시 오래된 기록 제거
        if (_dailyHistory.Count > MAX_HISTORY_DAYS)
        {
            _dailyHistory.RemoveAt(0);
        }
    }

    // ===== Getters =====

    public int GetDailySales()            => dailySales;
    public int GetDailyExpenses()         => dailyExpenses;
    public int GetNetProfit()             => dailySales - dailyExpenses;
    public int GetSatisfiedCustomerCount() => satisfiedCustomerCount;
    public int GetPremiumDishCount()      => premiumDishCount;
    public int GetLostCustomerCount()     => lostCustomerCount;
    public IReadOnlyList<DailyRecord> GetDailyHistory() => _dailyHistory;

    public void AddLostCustomer()
    {
        lostCustomerCount++;
    }

    /// <summary>오늘 가장 많이 팔린 메뉴 이름을 반환합니다.</summary>
    public string GetTopMenu()
    {
        string topMenu = "-";
        int maxCount = 0;

        // LINQ 미사용 — Dictionary를 직접 순회
        foreach (var kvp in salesByMenu)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                topMenu = kvp.Key;
            }
        }
        return topMenu;
    }

    /// <summary>특정 메뉴의 오늘 판매량을 반환합니다.</summary>
    public int GetMenuSalesCount(string menuName)
    {
        if (salesByMenu.TryGetValue(menuName, out int count)) return count;
        return 0;
    }
}
