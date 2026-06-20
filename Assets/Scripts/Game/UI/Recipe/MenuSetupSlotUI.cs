using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MenuSetupSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _recipeNameText;
    [SerializeField] private TMP_Text _recipePriceText;
    [SerializeField] private Image _recipeImage;
    [SerializeField] private GameObject _selectedCheckmark; // 토글 대신 선택 상태를 표시할 UI
    [SerializeField] private GameObject _lockedOverlay; // 선택 불가/재료 부족 시 표시할 오버레이
    [SerializeField] private TMP_Text _unavailableReasonText; // 선택 불가 이유를 표시할 텍스트
    [SerializeField] TMP_Text _countText;

    private bool _isOn;
    private bool _isInteractable = true;
    private int _baseIngredientCount;

    public System.Collections.Generic.List<int> UniqueIngredientIDs { get; private set; } = new System.Collections.Generic.List<int>();

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

        _isOn = isOn;
        if (_selectedCheckmark != null)
        {
            _selectedCheckmark.SetActive(_isOn);
        }

        UniqueIngredientIDs.Clear();
        if (food.ingredientConfigs != null)
        {
            for (int i = 0; i < food.ingredientConfigs.Length; i++)
            {
                if (food.ingredientConfigs[i].rawIngredient != null)
                {
                    int id = food.ingredientConfigs[i].rawIngredient.ingredientID;
                    if (!UniqueIngredientIDs.Contains(id))
                    {
                        UniqueIngredientIDs.Add(id);
                    }
                }
            }
        }
        _baseIngredientCount = UniqueIngredientIDs.Count;

        if (_countText != null)
        {
            _countText.text = $"재료 {_baseIngredientCount}칸";
        }

        SetInteractable(true);
    }

    public void UpdateAdditionalCount(int additionalCount, bool isSelected)
    {
        if (_countText == null) return;

        if (isSelected)
        {
            _countText.text = $"재료 {_baseIngredientCount}칸 (선택됨)";
        }
        else
        {
            if (additionalCount == 0)
            {
                _countText.text = "추가 +0칸 (중복)";
            }
            else
            {
                _countText.text = $"추가 +{additionalCount}칸";
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;

        _isOn = !_isOn;
        if (_selectedCheckmark != null)
        {
            _selectedCheckmark.SetActive(_isOn);
        }

        _onToggleValueChanged?.Invoke(this, _isOn);
    }

    public void SetToggleWithoutNotify(bool isOn)
    {
        _isOn = isOn;
        if (_selectedCheckmark != null)
        {
            _selectedCheckmark.SetActive(_isOn);
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
        if (_lockedOverlay != null)
        {
            _lockedOverlay.SetActive(!isInteractable);
        }
    }

    public void SetUnavailable(bool isUnavailable, string reason = "")
    {
        if (_lockedOverlay != null)
        {
            _lockedOverlay.SetActive(isUnavailable);
        }

        if (_unavailableReasonText != null && isUnavailable)
        {
            _unavailableReasonText.text = reason;
        }
    }
}
