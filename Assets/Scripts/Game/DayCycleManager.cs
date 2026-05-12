using System;
using UnityEngine;

public enum DayPhase
{
    DawnMarket,     // 06:00 - 09:00
    Preparation,    // 09:00 - 12:00
    Business,       // 12:00 - 22:00
    Settlement      // 22:00 - 06:00 (정산 및 다음날 대기)
}

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    public DayPhase CurrentPhase { get; private set; }

    public event Action<DayPhase> OnPhaseChanged;
    public event Action OnNewDayStarted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        UpdatePhase();
    }

    private void Update()
    {
        UpdatePhase();
    }

    private void UpdatePhase()
    {
        int hour = GameTimeManager.Instance.GetCurrentHour();
        DayPhase newPhase;

        if (hour >= 6 && hour < 9)
            newPhase = DayPhase.DawnMarket;
        else if (hour >= 9 && hour < 12)
            newPhase = DayPhase.Preparation;
        else if (hour >= 12 && hour < 22)
            newPhase = DayPhase.Business;
        else
            newPhase = DayPhase.Settlement;

        if (newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(CurrentPhase);
            Debug.Log($"[DayCycle] Phase Changed: {CurrentPhase}");
        }
    }

    /// <summary>
    /// 다음 날로 넘어갈 때 호출되는 핵심 로직
    /// </summary>
    public void StartNextDay()
    {
        // 1. 인벤토리 유통기한 처리
        InventoryManager.Instance.ProcessDailyExpiry();

        // 2. 시장 품목 갱신
        StoreManager.Instance.Market.GenerateAllListings();

        // 3. 날짜 증가 및 저장
        GameTimeManager.Instance.AdvanceDay();

        // 4. 시간 초기화 (아침 6시)
        GameTimeManager.Instance.SetTime(6, 0);

        OnNewDayStarted?.Invoke();
        Debug.Log("<color=green>[DayCycle] 새로운 하루가 시작되었습니다!</color>");
    }
}
