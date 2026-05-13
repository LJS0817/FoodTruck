using System;
using UnityEngine;

public enum DayPhase
{
    Preparation,    // 준비 단계 (장사 전)
    Business,       // 장사 중
    Settlement      // 정산 단계 (장사 종료 후)
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
        // 처음 시작 시 준비 단계로 설정
        ChangePhase(DayPhase.Preparation);
    }

    public void ChangePhase(DayPhase newPhase)
    {
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

        OnNewDayStarted?.Invoke();
        Debug.Log("<color=green>[DayCycle] 새로운 하루가 시작되었습니다!</color>");
    }
}
