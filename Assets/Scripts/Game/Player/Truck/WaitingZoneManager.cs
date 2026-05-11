using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웨이팅존에 설치된 아이템(난로, 파라솔, 네온사인 등)을 관리하고,
/// 설치된 아이템 조합에 따라 손님 인내심 감소 속도 배율을 계산합니다.
/// </summary>
public class WaitingZoneManager : MonoBehaviour
{
    public static WaitingZoneManager Instance { get; private set; }

    [Header("Installed Items")]
    [SerializeField] private List<WaitingZoneItemData> installedItems = new List<WaitingZoneItemData>();

    // 캐싱: 아이템 변경 시 재계산
    private float _cachedMultiplier = 1.0f;
    private bool _isDirty = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 날씨 변경 시 배율 재계산 필요
        if (WeatherTrendManager.Instance != null)
            WeatherTrendManager.Instance.OnWeatherTrendUpdated += MarkDirty;
    }

    private void OnDestroy()
    {
        if (WeatherTrendManager.Instance != null)
            WeatherTrendManager.Instance.OnWeatherTrendUpdated -= MarkDirty;
    }

    private void MarkDirty() { _isDirty = true; }

    // ===== Public API =====

    /// <summary>
    /// 인내심 감소 배율을 반환합니다. 1.0이 기본, 값이 낮을수록 느리게 감소합니다.
    /// </summary>
    public float GetPatienceDrainMultiplier()
    {
        if (_isDirty) RecalculateMultiplier();
        return _cachedMultiplier;
    }

    /// <summary>
    /// 웨이팅존 아이템을 설치합니다.
    /// </summary>
    public void InstallItem(WaitingZoneItemData item)
    {
        if (item == null) return;
        installedItems.Add(item);
        _isDirty = true;
        Debug.Log($"<color=cyan>[웨이팅존] {item.itemName} 설치 완료!</color>");
    }

    /// <summary>
    /// 웨이팅존 아이템을 제거합니다.
    /// </summary>
    public void RemoveItem(WaitingZoneItemData item)
    {
        if (installedItems.Remove(item))
        {
            _isDirty = true;
            Debug.Log($"<color=orange>[웨이팅존] {item.itemName} 제거됨.</color>");
        }
    }

    public IReadOnlyList<WaitingZoneItemData> GetInstalledItems() => installedItems;

    // ===== Internal =====

    private void RecalculateMultiplier()
    {
        // 각 아이템의 감소 배율을 누적 곱연산 (GC 없이 순수 루프)
        float result = 1.0f;
        for (int i = 0; i < installedItems.Count; i++)
        {
            if (installedItems[i] != null)
            {
                // 💡 날씨 조건이 맞을 때만 효과 적용
                if (!installedItems[i].IsEffectiveInCurrentWeather()) continue;
                result *= installedItems[i].drainRateMultiplier;
            }
        }

        // 0.1 이하로는 떨어지지 않도록 하한선 보정 (손님이 영원히 기다리는 버그 방지)
        _cachedMultiplier = Mathf.Max(0.1f, result);
        _isDirty = false;

        Debug.Log($"[웨이팅존] 인내심 감소 배율 재계산: {_cachedMultiplier:F2}");
    }
}
