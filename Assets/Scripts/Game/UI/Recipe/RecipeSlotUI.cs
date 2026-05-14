using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RecipeSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image foodIcon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public GameObject premiumCrownIcon; // 프리미엄 달성 시 보여줄 왕관 아이콘
    public GameObject unknownCover;     // 아직 해금하지 않았을 때 가려줄 검은색/물음표 덮개

    private FoodData _foodData;
    private bool _isUnlocked;

    public void SetupSlot(FoodData data, bool isUnlocked, bool hasPremium)
    {
        _foodData = data;
        _isUnlocked = isUnlocked;
        if (isUnlocked)
        {
            // 해금된 상태: 정상 정보 표시
            unknownCover.SetActive(false);
            foodIcon.sprite = data.iconSprite; // FoodData에 아이콘 변수가 있다고 가정
            nameText.text = data.foodName;
            priceText.text = data.basePrice.ToString() + "원";
            premiumCrownIcon.SetActive(hasPremium);
        }
        else
        {
            // 미해금 상태: 물음표로 가리기
            unknownCover.SetActive(true);
            nameText.text = "???";
            priceText.text = "? 원";
            premiumCrownIcon.SetActive(false);
        }
    }

    public void SetupCustomSlot(string customName, int price)
    {
        _foodData = null; // 커스텀 레시피는 FoodData가 없음
        _isUnlocked = true;

        // 유저가 개발한 것이므로 항상 해금 상태입니다.
        unknownCover.SetActive(false);

        // 커스텀 레시피는 전용 아이콘이 없으므로 기본 연구 아이콘 등을 할당합니다.
        //if (customRecipeDefaultIcon != null)
        //{
        //    foodIcon.sprite = customRecipeDefaultIcon;
        //}

        nameText.text = customName;
        priceText.text = price.ToString() + "원";

        // 커스텀 레시피는 기본적으로 프리미엄 마크를 끕니다. (추후 로직에 따라 변경 가능)
        premiumCrownIcon.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_foodData != null && _isUnlocked)
        {
            // 상점 모드(true)로 띄우면 이미 해금된 레시피이므로 구매 버튼이 비활성화되며 상세 정보만 확인할 수 있습니다.
            StoreItem item = StoreItem.FromRecipe(_foodData, 0, 1);
            if (StoreManager.Instance != null && StoreManager.Instance.UIController != null)
            {
                StoreManager.Instance.UIController.ShowItemInfo(item, true);
            }
        }
    }
}