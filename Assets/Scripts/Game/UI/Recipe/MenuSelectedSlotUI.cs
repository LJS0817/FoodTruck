using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MenuSelectedSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _recipeNameText;
    [SerializeField] private Image _recipeImage;
    [SerializeField] private Button _removeButton;

    private FoodData _foodData;
    private Action<FoodData> _onRemoveClicked;

    public FoodData FoodData => _foodData;

    private void Awake()
    {
        if (_removeButton != null)
        {
            _removeButton.onClick.AddListener(OnRemoveClicked);
        }
        else
        {
            // 만약 삭제 버튼이 따로 없고 슬롯 자체를 버튼처럼 쓴다면 자체 Button 컴포넌트에 연결
            Button selfButton = GetComponent<Button>();
            if (selfButton != null)
            {
                selfButton.onClick.AddListener(OnRemoveClicked);
            }
        }
    }

    public void Init(FoodData food, Action<FoodData> onRemove)
    {
        _foodData = food;
        _onRemoveClicked = onRemove;

        if (_recipeNameText != null) _recipeNameText.text = food.foodName;
        if (_recipeImage != null)
        {
            _recipeImage.sprite = food.iconSprite;
            _recipeImage.gameObject.SetActive(food.iconSprite != null);
        }
    }

    private void OnRemoveClicked()
    {
        _onRemoveClicked?.Invoke(_foodData);
    }
}
