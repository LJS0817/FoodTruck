using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손님 1명의 인내심(Patience) 감소 로직과 UI를 함께 관리합니다.
/// CustomerController의 컴포넌트로 부착됩니다.
/// </summary>
[RequireComponent(typeof(CustomerController))]
public class CustomerPatienceController : MonoBehaviour
{
    // ===== References =====
    private CustomerController _owner;

    [Header("UI")]
    [SerializeField] private CustomerPatienceUI patienceUI; // 이 손님 위에 붙는 UI

    // ===== Runtime State =====
    private float _maxPatience;
    private float _currentPatience;
    private bool _isDecreasing = false; // WaitState 진입 시 true로 설정

    // ===== Constants =====
    /// <summary>기본 인내심 감소 속도 배율 (1.0 = 정상)</summary>
    private const float BASE_DRAIN_RATE = 1.0f;

    // ===== Properties =====
    public float CurrentPatience => _currentPatience;
    public float MaxPatience => _maxPatience;
    public bool IsOut => _currentPatience <= 0f;

    // ===== Unity Lifecycle =====

    private void Awake()
    {
        _owner = GetComponent<CustomerController>();
    }

    private void Update()
    {
        if (!_isDecreasing) return;

        // 웨이팅존 버프를 반영한 감소량 계산
        float drainRate = BASE_DRAIN_RATE * GetWaitingZoneMultiplier();
        
        // 💡 날씨/유행 트렌드 패널티 (오늘 유행하는 맛이 메뉴에 하나도 없을 경우)
        if (WeatherTrendManager.Instance != null && MenuManager.Instance != null)
        {
            FlavorTag todayTrend = WeatherTrendManager.Instance.currentTrend;
            if (todayTrend != FlavorTag.None && !MenuManager.Instance.HasTrendFlavor(todayTrend))
            {
                drainRate *= 1.5f; // 유행에 뒤떨어지면 1.5배 빨리 화냄
            }
        }

        // 💡 트럭 청결도 페널티
        if (HygieneManager.Instance != null)
            drainRate *= HygieneManager.Instance.GetPatiencePenaltyMultiplier();

        // 💡 돌발 이벤트 패널티 (예: 축제 시 인내심 급감)
        if (RandomEventManager.Instance != null)
            drainRate *= RandomEventManager.Instance.GetPatienceMultiplier();
        
        // 💡 알바생(엔터테이너 등) 인내심 감소 속도 저하 버프 적용
        if (UpgradeManager.Instance.Worker != null)
        {
            float workerBonus = UpgradeManager.Instance.Worker.GetAbilityTotalValue(WorkerAbility.PatienceBoost);
            drainRate *= Mathf.Max(0.1f, 1f - workerBonus);
        }

        _currentPatience -= Time.deltaTime * drainRate;
        _currentPatience = Mathf.Max(0f, _currentPatience);

        // UI 갱신
        patienceUI?.UpdatePatience(_currentPatience);
    }

    // ===== Public API =====

    /// <summary>
    /// WaitState 진입 시 호출. 인내심 감소를 시작하고 UI를 초기화합니다.
    /// </summary>
    public void StartDecreasing()
    {
        _maxPatience = _owner.currentData != null ? _owner.currentData.maxPatience : 30f;
        _currentPatience = _maxPatience;
        _isDecreasing = true;

        patienceUI?.Init(_maxPatience);
        patienceUI?.gameObject.SetActive(true);
    }

    /// <summary>
    /// WaitState 종료(서빙 완료 또는 이탈) 시 호출. 감소를 멈추고 UI를 숨깁니다.
    /// </summary>
    public void StopDecreasing()
    {
        _isDecreasing = false;
        patienceUI?.gameObject.SetActive(false);
    }

    /// <summary>
    /// 풀 반환 시 상태를 초기화합니다.
    /// </summary>
    public void ResetState()
    {
        _isDecreasing = false;
        _currentPatience = 0f;
        patienceUI?.gameObject.SetActive(false);
    }

    // ===== Waiting Zone Buff =====

    /// <summary>
    /// 웨이팅존에 설치된 아이템의 종류에 따라 인내심 감소 속도 배율을 반환합니다.
    /// 값이 낮을수록 감소 속도가 느려집니다. (버프가 강할수록 손님이 오래 기다림)
    /// </summary>
    private float GetWaitingZoneMultiplier()
    {
        if (StoreManager.Instance.WaitingZone == null) return BASE_DRAIN_RATE;

        return StoreManager.Instance.WaitingZone.GetPatienceDrainMultiplier();
    }
}
