using System.Collections.Generic;
using UnityEngine;

public class DistrictManager : MonoBehaviour
{
    [Header("Catalog")]
    public List<DistrictData> allDistricts;

    private DistrictData _currentDistrict;
    private List<int> _unlockedDistrictIDs = new List<int>();

    public DistrictData CurrentDistrict => _currentDistrict;

    public event System.Action<DistrictData> OnDistrictChanged;

    private void Awake()
    {
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += PayDailyRent;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= PayDailyRent;
        }
    }

    // ===== 해금 및 이동 =====

    public bool IsUnlocked(int districtID) => _unlockedDistrictIDs.Contains(districtID);

    public bool UnlockDistrict(DistrictData district)
    {
        if (IsUnlocked(district.districtID)) return false;

        // 평판 조건 체크
        if (ReputationManager.Instance.CurrentReputation < district.requiredReputation)
        {
            Debug.LogWarning($"[구역] 해금 실패: 평판 {district.requiredReputation} 필요.");
            return false;
        }

        if (PlayerManager.Instance.SpendMoney(district.unlockCost))
        {
            _unlockedDistrictIDs.Add(district.districtID);
            SettlementManager.Instance?.AddExpense(district.unlockCost);
            Debug.Log($"<color=cyan>[구역] {district.districtName} 해금 완료!</color>");

            SyncToSaveData();
            return true;
        }
        return false;
    }

    public void MoveToDistrict(DistrictData district)
    {
        if (!IsUnlocked(district.districtID)) return;

        _currentDistrict = district;
        Debug.Log($"<color=green>[구역 이동] 트럭이 {district.districtName}로 이동했습니다.</color>");
        
        OnDistrictChanged?.Invoke(_currentDistrict);
        SyncToSaveData();
    }

    private void PayDailyRent()
    {
        if (_currentDistrict != null && _currentDistrict.dailyRent > 0)
        {
            PlayerManager.Instance.SpendMoney(_currentDistrict.dailyRent);
            SettlementManager.Instance?.AddExpense(_currentDistrict.dailyRent);
            Debug.Log($"<color=red>[자릿세] {_currentDistrict.districtName} 자릿세 {_currentDistrict.dailyRent}원이 차감되었습니다.</color>");
        }
    }

    // ===== 저장 연동 =====

    public void LoadFromSaveData(List<int> unlockedIDs, int currentID)
    {
        _unlockedDistrictIDs = unlockedIDs;

        if (_unlockedDistrictIDs.Count == 0 && allDistricts.Count > 0)
        {
            // 최초 실행 시 첫 번째 구역 기본 해금
            _unlockedDistrictIDs.Add(allDistricts[0].districtID);
            currentID = allDistricts[0].districtID;
        }

        _currentDistrict = null;
        for (int i = 0; i < allDistricts.Count; i++)
        {
            if (allDistricts[i].districtID == currentID)
            {
                _currentDistrict = allDistricts[i];
                break;
            }
        }
        if (_currentDistrict == null && allDistricts.Count > 0)
            _currentDistrict = allDistricts[0];

        OnDistrictChanged?.Invoke(_currentDistrict);
    }

    private void SyncToSaveData()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;
        
        DataManager.Instance.CurrentData.unlockedDistrictIDs = _unlockedDistrictIDs;
        if (_currentDistrict != null)
            DataManager.Instance.CurrentData.currentDistrictID = _currentDistrict.districtID;
    }
}
