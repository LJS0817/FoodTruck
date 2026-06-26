using System;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI")]
    [SerializeField] private Button restButton;
    [SerializeField] PlayerStaminaUI _staminaUI;

    // UI 갱신용 이벤트 (현재 피로도, 최대 피로도)
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaDepleted;

    private float _cachedMaxStamina;

    // Properties (업그레이드 반영)
    public float CurrentStamina => _currentStamina;
    public float MaxStamina => _cachedMaxStamina;

    private void UpdateCachedMaxStamina()
    {
        float bonus = 0f;
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.Upgrade != null)
        {
            bonus = UpgradeManager.Instance.Upgrade.GetCurrentValue("MaxStamina");
        }
        _cachedMaxStamina = maxStamina + bonus;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        OnStaminaChanged += _staminaUI.UpdateUI;
    }

    private void Start()
    {
        UpdateCachedMaxStamina();

        // 1. DataManager에서 저장된 체력 로드 (오프라인 회복 로직 제거)
        if (DataManager.Instance != null && DataManager.Instance.CurrentData != null)
        {
            _currentStamina = DataManager.Instance.CurrentData.currentStamina;
        }
        else
        {
            _currentStamina = MaxStamina;
        }

        _currentStamina = Mathf.Clamp(_currentStamina, 0f, MaxStamina);
        OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

        // DayCycle 이벤트 구독
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }
        
        // 업그레이드 이벤트 구독
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.Upgrade != null)
        {
            UpgradeManager.Instance.Upgrade.OnUpgradePurchased += OnUpgradePurchased;
        }

        // 휴식 버튼 스크립트 연결
        if (restButton != null)
        {
            restButton.onClick.RemoveAllListeners();
            restButton.onClick.AddListener(() => RestAndRecover(3000));
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
        
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.Upgrade != null)
        {
            UpgradeManager.Instance.Upgrade.OnUpgradePurchased -= OnUpgradePurchased;
        }
        OnStaminaChanged -= _staminaUI.UpdateUI;
    }

    private void Update()
    {
        if (_isDraining)
        {
            // 💡 피로도 소모 속도 계산 (기본값 - 업그레이드 보너스) * (1 - 알바생 절약 보너스)
            float currentDrainRate = drainRate;
            
            if (UpgradeManager.Instance.Upgrade != null)
                currentDrainRate -= UpgradeManager.Instance.Upgrade.GetCurrentValue("DrainRate");
            
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
                // 💡 인게임 6시간(21,600초) 기준 최대 체력만큼 회복 = 인게임 1초당 currentMax / 21600 회복 (업그레이드 반영)
                float inGameSecondsPassed = Time.deltaTime * 96f; // TIME_MULTIPLIER = 96f
                if (GameTimeManager.Instance != null)
                {
                    inGameSecondsPassed *= GameTimeManager.Instance.timeScaleMultiplier;
                }

                float recoveredAmount = (currentMax / 21600f) * inGameSecondsPassed;

                _currentStamina += recoveredAmount;
                _currentStamina = Mathf.Min(currentMax, _currentStamina);
                OnStaminaChanged?.Invoke(_currentStamina, currentMax);
            }
        }
    }

    // ===== Phase & Upgrade Events =====
    
    private void OnUpgradePurchased(string upgradeID)
    {
        if (upgradeID == "MaxStamina")
        {
            UpdateCachedMaxStamina();
            // 업그레이드 전의 최대 체력을 알 수 없으므로, 현재 체력을 MaxStamina로 설정하거나, UI만 갱신할 수 있습니다.
            // 체력을 꽉 채우지 않고 UI만 즉각 반영합니다.
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }
    }

    private void OnPhaseChanged(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Business:
                // 영업 시작 시 자동으로 감소 시작
                _isDraining = true;
                _isRecovering = false;
                break;

            case DayPhase.Preparation:
                // 준비 단계에서 회복
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
    /// 피로도를 즉시 소모합니다. (예: 청소 등)
    /// </summary>
    public void DrainStamina(float amount)
    {
        _currentStamina -= amount;
        _currentStamina = Mathf.Max(0f, _currentStamina);
        OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

        if (_currentStamina <= 0f)
        {
            _isDraining = false;
            OnStaminaDepleted?.Invoke();
            ForceCloseBusiness();
        }
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
    /// <summary>
    /// 돈을 지불하고 즉시 체력을 100% 회복합니다. (휴식 버튼용)
    /// </summary>
    public bool RestAndRecover(int cost = 3000)
    {
        if (PlayerManager.Instance.SpendMoney(cost))
        {
            _currentStamina = MaxStamina;
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            Debug.Log($"<color=green>[휴식] {cost}원을 지불하고 {MaxStamina} 체력을 즉시 회복했습니다!</color>");
            
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=red>[휴식 실패] 돈이 부족합니다. (필요 금액: {cost}원)</color>");
            return false;
        }
    }
}
