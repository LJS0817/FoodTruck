using System.Collections.Generic;
using UnityEngine;

public class MarketingManager : MonoBehaviour
{
    [Header("Catalog")]
    public List<MarketingData> allMarketingCampaigns;

    // 오늘 활성화된 마케팅
    private MarketingData _activeCampaign;
    
    public MarketingData ActiveCampaign => _activeCampaign;

    private void Awake()
    {
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += ResetDailyCampaign;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= ResetDailyCampaign;
        }
    }

    // ===== 기능 =====

    public bool StartCampaign(MarketingData campaign)
    {
        if (_activeCampaign != null)
        {
            Debug.LogWarning("[마케팅] 하루에 하나의 캠페인만 진행할 수 있습니다.");
            return false;
        }

        if (DayCycleManager.Instance != null && DayCycleManager.Instance.CurrentPhase != DayPhase.Preparation)
        {
            Debug.LogWarning("[마케팅] 마케팅은 영업 전(Preparation)에만 실행할 수 있습니다.");
            return false;
        }

        if (PlayerManager.Instance.SpendMoney(campaign.cost))
        {
            _activeCampaign = campaign;
            SettlementManager.Instance?.AddExpense(campaign.cost);
            Debug.Log($"<color=cyan>[마케팅] '{campaign.campaignName}' 캠페인 시작! 오늘 하루 동안 효과가 적용됩니다.</color>");
            return true;
        }
        return false;
    }

    private void ResetDailyCampaign()
    {
        _activeCampaign = null;
    }

    // ===== 적용 헬퍼 (CustomerManager 등에서 호출) =====

    public float GetSpawnBoostMultiplier()
    {
        if (_activeCampaign != null && _activeCampaign.type == MarketingType.SpawnBoost)
            return _activeCampaign.effectMultiplier;
        return 1.0f;
    }

    public float GetVIPBoostBonus()
    {
        if (_activeCampaign != null && _activeCampaign.type == MarketingType.VIPBoost)
            return _activeCampaign.effectMultiplier; // 예: 기존 0.05 + 0.10
        return 0f;
    }
}
