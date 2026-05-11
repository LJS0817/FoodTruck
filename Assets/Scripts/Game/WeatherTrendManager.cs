using System;
using UnityEngine;

public enum WeatherType
{
    Sunny,
    Rainy,
    Hot,
    Cold
}

public class WeatherTrendManager : MonoBehaviour
{
    public static WeatherTrendManager Instance { get; private set; }

    [Header("Current State")]
    public WeatherType currentWeather;
    public FlavorTag currentTrend;

    public event Action OnWeatherTrendUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += RandomizeWeatherAndTrend;
        }
        
        // 첫날 초기화
        RandomizeWeatherAndTrend();
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= RandomizeWeatherAndTrend;
        }
    }

    public void RandomizeWeatherAndTrend()
    {
        // 1. 날씨 랜덤 결정
        Array weatherValues = Enum.GetValues(typeof(WeatherType));
        currentWeather = (WeatherType)weatherValues.GetValue(UnityEngine.Random.Range(0, weatherValues.Length));

        // 2. 유행(맛 태그) 랜덤 결정 (None 제외)
        Array flavorValues = Enum.GetValues(typeof(FlavorTag));
        // 1번(None) 이후부터 선택
        currentTrend = (FlavorTag)flavorValues.GetValue(UnityEngine.Random.Range(1, flavorValues.Length));

        Debug.Log($"<color=cyan>[WeatherTrend] 오늘의 날씨: {currentWeather}, 오늘의 유행: {currentTrend}</color>");
        OnWeatherTrendUpdated?.Invoke();
    }

    /// <summary>
    /// 요리가 현재 유행에 맞는지 확인하여 보너스 배율을 반환합니다.
    /// </summary>
    public float GetTrendMultiplier(FoodData food)
    {
        if (food == null || food.flavorTags == null) return 1.0f;

        if (food.flavorTags.Contains(currentTrend))
        {
            return 1.2f; // 유행하는 맛일 경우 20% 보너스
        }

        // 날씨에 따른 선호 맛 체크
        if (currentWeather == WeatherType.Rainy || currentWeather == WeatherType.Cold)
        {
            if (food.flavorTags.Contains(FlavorTag.Warm)) return 1.1f;
        }
        else if (currentWeather == WeatherType.Hot)
        {
            if (food.flavorTags.Contains(FlavorTag.Cold)) return 1.1f;
        }

        return 1.0f;
    }
    
    /// <summary>
    /// 날씨에 따른 손님 스폰 확률 보정치
    /// </summary>
    public float GetSpawnRateMultiplier()
    {
        switch (currentWeather)
        {
            case WeatherType.Rainy: return 0.7f; // 비오면 손님 적음
            case WeatherType.Hot: return 1.2f;   // 더우면 시원한거 찾으러 많이 옴 (가정)
            default: return 1.0f;
        }
    }
}
