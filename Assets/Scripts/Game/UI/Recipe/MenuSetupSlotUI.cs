using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MenuSetupSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _recipeNameText;
    [SerializeField] private TMP_Text _recipePriceText;
    [SerializeField] private Image _recipeImage;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameObject _lockedOverlay; // 선택 불가 시 표시할 오버레이 (옵션)

    private FoodData _foodData;
    private Action<MenuSetupSlotUI, bool> _onToggleValueChanged;

    public FoodData FoodData => _foodData;

    public void Init(FoodData food, bool isOn, Action<MenuSetupSlotUI, bool> onToggle)
    {
        _foodData = food;
        _onToggleValueChanged = onToggle;

        if (_recipeNameText != null) _recipeNameText.text = food.foodName;
        if (_recipePriceText != null) _recipePriceText.text = $"{food.basePrice}원";
        if (_recipeImage != null)
        {
            _recipeImage.sprite = food.iconSprite;
            _recipeImage.gameObject.SetActive(food.iconSprite != null);
        }

        // 콜백 임시 해제 후 값 변경
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.isOn = isOn;
        _toggle.onValueChanged.AddListener(OnToggleChanged);

        SetInteractable(true);
    }

    private void OnToggleChanged(bool isOn)
    {
        _onToggleValueChanged?.Invoke(this, isOn);
    }

    public void SetToggleWithoutNotify(bool isOn)
    {
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.isOn = isOn;
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void SetInteractable(bool isInteractable)
    {
        _toggle.interactable = isInteractable;
        if (_lockedOverlay != null)
        {
            _lockedOverlay.SetActive(!isInteractable);
        }
    }
}
