using UnityEngine;

/// <summary>
/// 웨이팅존에 설치 가능한 아이템 하나의 데이터를 정의하는 ScriptableObject입니다.
/// (예: 난로, 파라솔, 네온사인, 음악 스피커 등)
/// </summary>
[CreateAssetMenu(fileName = "New WaitingZoneItem", menuName = "FoodTruck/WaitingZoneItemData")]
public class WaitingZoneItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Shop")]
    public int purchasePrice;   // 구매 가격

    [Header("Effect")]
    [Tooltip("인내심 감소 배율. 1.0 = 변화 없음, 0.7 = 30% 느리게 감소 (유리한 방향)")]
    [Range(0.1f, 1.0f)]
    public float drainRateMultiplier = 1.0f;

    [Tooltip("특정 날씨에서만 효과를 발휘하는 경우 설정. None이면 항상 적용.")]
    public WeatherType effectiveWeather = (WeatherType)(-1); // -1 = 모든 날씨

    /// <summary>현재 날씨에서 이 아이템이 효과가 있는지 확인합니다.</summary>
    public bool IsEffectiveInCurrentWeather()
    {
        // effectiveWeather가 유효한 enum 값이 아니면 (= -1 세팅) 항상 효과 있음
        if ((int)effectiveWeather < 0) return true;

        if (WeatherTrendManager.Instance == null) return true;
        return WeatherTrendManager.Instance.currentWeather == effectiveWeather;
    }
}
