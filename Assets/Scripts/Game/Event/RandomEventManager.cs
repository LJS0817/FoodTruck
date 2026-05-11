using System.Collections.Generic;
using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager Instance { get; private set; }

    [Header("Event Settings")]
    [Tooltip("매일 아침 이벤트가 발생할 확률 (0~1)")]
    public float eventChance = 0.35f; 
    public List<RandomEventData> allEvents;

    // 오늘 활성화된 이벤트 (지속형)
    private RandomEventData _todayEvent;
    public RandomEventData TodayEvent => _todayEvent;

    // UI 등을 통해 이벤트를 알리기 위한 델리게이트
    public event System.Action<RandomEventData, string> OnEventTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted += ClearDailyEvent;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted -= ClearDailyEvent;
        }
    }

    private void ClearDailyEvent()
    {
        _todayEvent = null;
    }

    private void OnPhaseChanged(DayPhase phase)
    {
        // 아침 준비 페이즈 진입 시 주사위 굴림
        if (phase == DayPhase.Preparation)
        {
            if (Random.value <= eventChance && allEvents != null && allEvents.Count > 0)
            {
                TriggerRandomEvent();
            }
        }
    }

    private void TriggerRandomEvent()
    {
        int index = Random.Range(0, allEvents.Count);
        RandomEventData evt = allEvents[index];

        string resultMessage = "";

        // 이벤트 타입별 로직 처리
        switch (evt.eventType)
        {
            case RandomEventType.HealthInspector:
                int currentRep = ReputationManager.Instance.CurrentReputation;
                if (currentRep >= 80)
                {
                    PlayerManager.Instance.AddMoney(2000);
                    resultMessage = "평판이 훌륭하여 모범 음식점으로 선정! 포상금 2,000원을 받았습니다.";
                }
                else if (currentRep < 30)
                {
                    PlayerManager.Instance.SpendMoney(1000); // 벌금
                    resultMessage = "위생 상태 불량으로 벌금 1,000원을 냈습니다...";
                }
                else
                {
                    resultMessage = "무사히 단속을 통과했습니다.";
                }
                break;

            case RandomEventType.Thief:
                int stolenAmount = Mathf.RoundToInt(PlayerManager.Instance.CurrentMoney * 0.1f);
                if (stolenAmount > 0)
                {
                    PlayerManager.Instance.SpendMoney(stolenAmount);
                    resultMessage = $"좀도둑이 들어 {stolenAmount}원을 훔쳐갔습니다!";
                }
                else
                {
                    resultMessage = "가져갈 돈이 없어서 도둑이 불쌍해하며 돌아갔습니다.";
                }
                break;

            case RandomEventType.GoodKarma:
                ReputationManager.Instance.OnServed(true); // 임시로 프리미엄 판정 함수를 5번 호출해 평판 대폭 상승
                ReputationManager.Instance.OnServed(true);
                ReputationManager.Instance.OnServed(true);
                resultMessage = "사장님의 선행이 SNS에 퍼져 평판이 크게 올랐습니다!";
                break;

            // 아래는 지속형 이벤트들
            case RandomEventType.Festival:
            case RandomEventType.Typhoon:
            case RandomEventType.MarketShortage:
            case RandomEventType.MarketSurplus:
            case RandomEventType.VIPRush:
            case RandomEventType.FatigueSpike:
                _todayEvent = evt;
                resultMessage = $"오늘 하루 동안 [{evt.eventName}] 효과가 지속됩니다.";
                break;
        }

        Debug.Log($"<color=orange>[돌발 이벤트] {evt.eventName} 발생! - {resultMessage}</color>");
        OnEventTriggered?.Invoke(evt, resultMessage);
    }

    // ===== 외부 매니저에서 효과를 조회하는 헬퍼 함수들 =====

    public float GetSpawnMultiplier()
    {
        if (_todayEvent == null) return 1f;
        if (_todayEvent.eventType == RandomEventType.Festival) return 1.5f; // 손님 1.5배
        if (_todayEvent.eventType == RandomEventType.Typhoon) return 0.2f;  // 손님 1/5토막
        return 1f;
    }

    public float GetPatienceMultiplier()
    {
        if (_todayEvent == null) return 1f;
        if (_todayEvent.eventType == RandomEventType.Festival) return 2.0f; // 인내심이 2배 빨리 닳음 (숫자가 클수록 빠름)
        return 1f;
    }

    public float GetVIPChanceBonus()
    {
        if (_todayEvent == null) return 0f;
        if (_todayEvent.eventType == RandomEventType.VIPRush) return 0.3f; // 30% 증가
        return 0f;
    }

    public float GetMarketPriceMultiplier()
    {
        if (_todayEvent == null) return 1f;
        if (_todayEvent.eventType == RandomEventType.MarketShortage) return 2.0f; // 물가 2배
        if (_todayEvent.eventType == RandomEventType.MarketSurplus) return 0.5f;  // 물가 반값
        return 1f;
    }

    public float GetStaminaDrainMultiplier()
    {
        if (_todayEvent == null) return 1f;
        if (_todayEvent.eventType == RandomEventType.FatigueSpike) return 2.0f; // 체력 2배 빨리 닳음
        return 1f;
    }

    public float GetPremiumTipMultiplier()
    {
        if (_todayEvent == null) return 1f;
        if (_todayEvent.eventType == RandomEventType.Typhoon) return 3.0f; // 태풍 올 땐 프리미엄 요리 팁 3배
        return 1f;
    }
}
