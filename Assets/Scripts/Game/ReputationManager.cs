using System;
using UnityEngine;

/// <summary>
/// 트럭의 평판을 관리합니다.
/// 서빙 성공/실패에 따라 평판이 오르내리고,
/// 평판 구간에 따라 손님 스폰율과 VIP 등장 확률이 달라집니다.
/// </summary>
public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxReputation = 100;
    [SerializeField] private int startingReputation = 30;

    [Header("Reward / Penalty")]
    [SerializeField] private int onServeSuccess = 2;
    [SerializeField] private int onServePremium = 5;
    [SerializeField] private int onCustomerAngryLeave = -3;

    private int _currentReputation;

    public event Action<int> OnReputationChanged;

    // Properties
    public int CurrentReputation => _currentReputation;
    public int MaxReputation => maxReputation;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // SaveData에서 로드
        if (DataManager.Instance != null && DataManager.Instance.CurrentData != null)
        {
            _currentReputation = DataManager.Instance.CurrentData.reputation;
            // 첫 실행 시 기본값 보정
            if (_currentReputation <= 0)
                _currentReputation = startingReputation;
        }
        else
        {
            _currentReputation = startingReputation;
        }

        OnReputationChanged?.Invoke(_currentReputation);
    }

    // ===== Public API =====

    /// <summary>서빙 성공 시 호출</summary>
    public void OnServed(bool isPremium)
    {
        int gain = isPremium ? onServePremium : onServeSuccess;
        ChangeReputation(gain);
    }

    /// <summary>손님이 화나서 이탈 시 호출</summary>
    public void OnCustomerLeft()
    {
        ChangeReputation(onCustomerAngryLeave);
    }

    /// <summary>직접 평판을 변경할 때 사용 (이벤트 등)</summary>
    public void ChangeReputation(int amount)
    {
        _currentReputation = Mathf.Clamp(_currentReputation + amount, 0, maxReputation);
        OnReputationChanged?.Invoke(_currentReputation);

        // DataManager에 즉시 반영 (저장은 하루 끝에)
        if (DataManager.Instance != null && DataManager.Instance.CurrentData != null)
        {
            DataManager.Instance.CurrentData.reputation = _currentReputation;
        }
    }

    // ===== 평판 구간별 보정치 =====

    /// <summary>
    /// 평판에 따른 손님 스폰 배율을 반환합니다.
    /// CustomerManager.Update()에서 사용됩니다.
    /// </summary>
    public float GetSpawnRateMultiplier()
    {
        if (_currentReputation >= 81) return 1.5f;
        if (_currentReputation >= 61) return 1.3f;
        if (_currentReputation >= 31) return 1.0f;
        return 0.7f; // 0~30: 손님이 적게 옴
    }

    /// <summary>
    /// 평판에 따른 VIP 등장 확률을 반환합니다 (0.0 ~ 1.0).
    /// CustomerManager.SpawnCustomer()에서 사용됩니다.
    /// </summary>
    public float GetVIPChance()
    {
        if (_currentReputation >= 81) return 0.20f;  // 20%
        if (_currentReputation >= 61) return 0.08f;  // 8%
        if (_currentReputation >= 31) return 0.03f;  // 3%
        return 0f; // 평판 30 이하면 VIP 미출현
    }
}
