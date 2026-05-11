using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPreviewItemUI : MonoBehaviour
{
    [SerializeField] private Image foodIcon;
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject trendingBadge; // 💡 '인기/HOT' 표시용 오브젝트

    public void SetInfo(FoodData data)
    {
        if (data == null) return;

        if (foodIcon != null)
            foodIcon.sprite = data.iconSprite;
        
        if (foodNameText != null)
            foodNameText.text = data.foodName;

        // 💡 유행 여부에 따른 UI 강조 및 가격 표시
        float multiplier = 1f;
        if (WeatherTrendManager.Instance != null)
        {
            multiplier = WeatherTrendManager.Instance.GetTrendMultiplier(data);
        }

        if (trendingBadge != null)
            trendingBadge.SetActive(multiplier > 1.0f);

        if (priceText != null)
        {
            int finalPrice = Mathf.RoundToInt(data.basePrice * multiplier);
            priceText.text = $"{finalPrice}원";
            priceText.color = (multiplier > 1.0f) ? Color.yellow : Color.white;
        }
    }
}
