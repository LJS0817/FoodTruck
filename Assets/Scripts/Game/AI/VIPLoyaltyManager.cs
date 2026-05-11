using System.Collections.Generic;
using UnityEngine;

public class VIPLoyaltyManager : MonoBehaviour
{
    public static VIPLoyaltyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ===== 단골 시스템 =====

    public int GetLoyaltyLevel(string vipName)
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return 0;

        var loyalties = DataManager.Instance.CurrentData.vipLoyalties;
        var data = loyalties.Find(v => v.vipName == vipName);
        return data != null ? data.loyaltyLevel : 0;
    }

    public void AddLoyalty(string vipName, int amount = 1)
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;

        var loyalties = DataManager.Instance.CurrentData.vipLoyalties;
        var data = loyalties.Find(v => v.vipName == vipName);

        if (data == null)
        {
            data = new VIPLoyaltyData { vipName = vipName, loyaltyLevel = 0 };
            loyalties.Add(data);
        }

        data.loyaltyLevel += amount;
        Debug.Log($"<color=magenta>[단골] VIP '{vipName}'님의 호감도가 올랐습니다! (현재 레벨: {data.loyaltyLevel})</color>");
    }

    /// <summary>
    /// 단골 레벨에 따른 팁 보너스 배율을 반환합니다.
    /// </summary>
    public float GetLoyaltyTipMultiplier(string vipName)
    {
        int level = GetLoyaltyLevel(vipName);
        
        // 예: 5레벨 이상이면 1.5배, 10레벨 이상이면 2.0배 추가 보너스
        if (level >= 10) return 2.0f;
        if (level >= 5) return 1.5f;
        return 1.0f; // 기본
    }
}
