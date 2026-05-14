using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeatherTrendUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text weatherText;
    [SerializeField] private Image weatherIcon;
    [SerializeField] private TMP_Text trendText;
    [SerializeField] private Image trendIcon;
    [SerializeField] private TMP_Text effectDescriptionText; // 효과 설명 (예: "매출 1.2배!")

    [Header("Icon Assets (Optional)")]
    [SerializeField] private Sprite sunnySprite;
    [SerializeField] private Sprite rainySprite;
    [SerializeField] private Sprite hotSprite;
    [SerializeField] private Sprite coldSprite;

    private void Start()
    {
        if (WeatherTrendManager.Instance != null)
        {
            WeatherTrendManager.Instance.OnWeatherTrendUpdated += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDestroy()
    {
        if (WeatherTrendManager.Instance != null)
        {
            WeatherTrendManager.Instance.OnWeatherTrendUpdated -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (WeatherTrendManager.Instance == null) return;

        // 1. 날씨 표시
        WeatherType weather = WeatherTrendManager.Instance.currentWeather;
        if (weatherText != null)
        {
            weatherText.text = GetWeatherNameKR(weather);
        }

        if (weatherIcon != null)
        {
            weatherIcon.sprite = GetWeatherSprite(weather);
        }

        // 2. 유행 표시
        FlavorTag trend = WeatherTrendManager.Instance.currentTrend;
        if (trendText != null)
        {
            trendText.text = $"{GetFlavorNameKR(trend)}";
        }

        // 3. 효과 설명 업데이트
        if (effectDescriptionText != null)
        {
            string weatherEff = (weather == WeatherType.Rainy) ? "손님 유입 감소" : "손님 유입 활발";
            effectDescriptionText.text = $"{weatherEff} / {GetFlavorNameKR(trend)} 메뉴 매출 1.2배!";
        }
    }

    private string GetWeatherNameKR(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny: return "맑음";
            case WeatherType.Rainy: return "비";
            case WeatherType.Hot: return "폭염";
            case WeatherType.Cold: return "한파";
            default: return "맑음";
        }
    }

    private string GetFlavorNameKR(FlavorTag tag)
    {
        switch (tag)
        {
            case FlavorTag.Spicy: return "매운맛";
            case FlavorTag.Sweet: return "단맛";
            case FlavorTag.Salty: return "짠맛";
            case FlavorTag.Sour: return "신맛";
            case FlavorTag.Warm: return "따뜻한";
            case FlavorTag.Cold: return "차가운";
            case FlavorTag.Greasy: return "기름진맛";
            case FlavorTag.Healthy: return "건강한맛";
            default: return "일반";
        }
    }

    private Sprite GetWeatherSprite(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny: return sunnySprite;
            case WeatherType.Rainy: return rainySprite;
            case WeatherType.Hot: return hotSprite;
            case WeatherType.Cold: return coldSprite;
            default: return sunnySprite;
        }
    }
}
