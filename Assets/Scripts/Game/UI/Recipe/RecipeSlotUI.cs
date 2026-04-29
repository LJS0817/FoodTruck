using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeSlotUI : MonoBehaviour
{
    public Image foodIcon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public GameObject premiumCrownIcon;
    public GameObject unknownCover;

    [Header("Custom Recipe Settings")]
    public Sprite customRecipeDefaultIcon; // 💡 커스텀 레시피용 기본 아이콘 (인스펙터에서 설정)

    // 1. 기본 레시피용 설정 (에셋 기반)
    public void SetupSlot(FoodData data, bool isUnlocked, bool hasPremium)
    {
        if (isUnlocked)
        {
            unknownCover.SetActive(false);
            foodIcon.sprite = data.iconSprite;
            nameText.text = data.foodName;
            priceText.text = data.basePrice.ToString() + "원";
            premiumCrownIcon.SetActive(hasPremium);
        }
        else
        {
            unknownCover.SetActive(true);
            nameText.text = "???";
            priceText.text = "? 원";
            premiumCrownIcon.SetActive(false);
        }
    }

    // 💡 2. 추가: 커스텀 레시피용 설정 (데이터 기반)
    public void SetupCustomSlot(string customName, int price)
    {
        // 유저가 개발한 것이므로 항상 해금 상태입니다.
        unknownCover.SetActive(false);

        // 커스텀 레시피는 전용 아이콘이 없으므로 기본 연구 아이콘 등을 할당합니다.
        if (customRecipeDefaultIcon != null)
        {
            foodIcon.sprite = customRecipeDefaultIcon;
        }

        nameText.text = customName;
        priceText.text = price.ToString() + "원";

        // 커스텀 레시피는 기본적으로 프리미엄 마크를 끕니다. (추후 로직에 따라 변경 가능)
        premiumCrownIcon.SetActive(false);
    }
}