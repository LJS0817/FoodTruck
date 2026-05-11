using System;
using UnityEngine;

/// <summary>
/// 사장님의 피로도(Stamina) 시스템.
/// 영업(Business) 중에만 감소하며, 0이 되면 자동으로 셔터를 닫습니다.
/// Preparation/DawnMarket 페이즈에서는 서서히 회복됩니다.
/// </summary>
public class PlayerStaminaManager : MonoBehaviour
{
    public static PlayerStaminaManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 1.5f;      // 영업 중 초당 감소량
    [SerializeField] private float recoveryRate = 5f;      // 비영업 시 초당 회복량

    private float _currentStamina;
    private bool _isDraining = false;
    private bool _isRecovering = false;

    // UI 갱신용 이벤트 (현재 피로도, 최대 피로도)
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaDepleted;

    // Properties (업그레이드 반영)
    public float CurrentStamina => _currentStamina;
    public float MaxStamina 
    {
        get 
        {
            float bonus = PlayerUpgradeManager.Instance != null ? PlayerUpgradeManager.Instance.GetCurrentValue("MaxStamina") : 0f;
            return maxStamina + bonus;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _currentStamina = MaxStamina;
        OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

        // DayCycle 이벤트 구독
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted += OnNewDay;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            DayCycleManager.Instance.OnNewDayStarted -= OnNewDay;
        }
    }

    private void Update()
    {
        if (_isDraining)
        {
            // 💡 피로도 소모 속도 계산 (기본값 - 업그레이드 보너스) * (1 - 알바생 절약 보너스)
            float currentDrainRate = drainRate;
            
            if (PlayerUpgradeManager.Instance != null)
                currentDrainRate -= PlayerUpgradeManager.Instance.GetCurrentValue("DrainRate");
            
            if (WorkerManager.Instance != null)
            {
                float saverBonus = WorkerManager.Instance.GetAbilityTotalValue(WorkerAbility.StaminaSaver);
                currentDrainRate *= Mathf.Max(0.1f, 1f - saverBonus); // 최소 10% 속도는 보장
            }

            // 💡 돌발 이벤트 패널티 (예: 폭염 시 체력 소모량 2배)
            if (RandomEventManager.Instance != null)
            {
                currentDrainRate *= RandomEventManager.Instance.GetStaminaDrainMultiplier();
            }

            // 0 이하로 떨어지지 않게 방어
            currentDrainRate = Mathf.Max(0.1f, currentDrainRate);

            _currentStamina -= currentDrainRate * Time.deltaTime;
            _currentStamina = Mathf.Max(0f, _currentStamina);
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

            if (_currentStamina <= 0f)
            {
                _isDraining = false;
                OnStaminaDepleted?.Invoke();
                ForceCloseBusiness();
            }
        }
        else if (_isRecovering)
        {
            float currentMax = MaxStamina;
            if (_currentStamina < currentMax)
            {
                _currentStamina += recoveryRate * Time.deltaTime;
                _currentStamina = Mathf.Min(currentMax, _currentStamina);
                OnStaminaChanged?.Invoke(_currentStamina, currentMax);
            }
        }
    }

    // ===== Phase Events =====

    private void OnPhaseChanged(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Business:
                // 영업 시작 시 자동으로 감소 시작
                _isDraining = true;
                _isRecovering = false;
                break;

            case DayPhase.DawnMarket:
            case DayPhase.Preparation:
                // 준비/새벽 시장에서 회복
                _isDraining = false;
                _isRecovering = true;
                break;

            case DayPhase.Settlement:
                // 정산 시 모든 감소/회복 정지
                _isDraining = false;
                _isRecovering = false;
                break;
        }
    }

    private void OnNewDay()
    {
        // 새 날이 시작되면 피로도 완전 회복
        _currentStamina = MaxStamina;
        OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
    }

    // ===== Public API =====

    /// <summary>
    /// 장사가 시작될 때 호출 (BusinessManager.ToggleBusiness → Open)
    /// </summary>
    public void StartDraining()
    {
        _isDraining = true;
        _isRecovering = false;
    }

    /// <summary>
    /// 장사가 종료될 때 호출 (BusinessManager.ToggleBusiness → Close)
    /// </summary>
    public void StopDraining()
    {
        _isDraining = false;
    }

    /// <summary>
    /// 피로도가 바닥나서 자동으로 장사를 종료합니다.
    /// </summary>
    private void ForceCloseBusiness()
    {
        if (BusinessManager.Instance != null && BusinessManager.Instance.IsBusinessOpen)
        {
            Debug.Log("<color=red>[피로도] 사장님이 지쳤습니다! 자동으로 장사를 종료합니다.</color>");
            BusinessManager.Instance.ToggleBusiness();
        }
    }
}
