using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 상점, 인벤토리, 레시피북 등에서 아이템이나 레시피의 상세 정보를 표시하는 범용 UI 클래스입니다.
/// </summary>
public class ItemInfoUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _priceText; // 단가 표시
    [SerializeField] private TMP_Text _totalPriceText; // 총 가격 표시
    [SerializeField] private TMP_Text _ownedAmountText; // 현재 보유량 표시
    [SerializeField] TMP_Text _maxAmount;
    [SerializeField] private Button _buyButton; // Submit 버튼
    [SerializeField] private TMP_Text _buyButtonText; // Submit 버튼 텍스트

    [Header("Quantity Selection")]
    [SerializeField] private CanvasGroup _quantityCanvasGroup; // 수량 선택 영역
    [SerializeField] private TMP_InputField _quantityInput;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;

    [Header("Recipe Details")]
    [SerializeField] private GameObject _recipeDetailsArea; // 레시피 전용 영역
    [SerializeField] private Transform _requirementsContainer; // 재료/도구 아이콘이 생성될 부모
    [SerializeField] private RecipeRequirementUI _requirementPrefab;

    private StoreItem _currentItem;
    private int _selectedQuantity = 1;
    private int _maxSelectableQuantity = 99;
    private bool _isStoreMode = true;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_quantityInput != null)
            _quantityInput.onEndEdit.AddListener(OnQuantityInputEndEdit);
        
        if (_plusButton != null)
            _plusButton.onClick.AddListener(() => ChangeQuantity(1));
        
        if (_minusButton != null)
            _minusButton.onClick.AddListener(() => ChangeQuantity(-1));

        if (_buyButton != null)
            _buyButton.onClick.AddListener(OnClickSubmit);

        // 인벤토리 업데이트 시 보유량 UI 갱신을 위해 이벤트 구독
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryUpdated += UpdateOwnedAmount;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryUpdated -= UpdateOwnedAmount;
    }

    /// <summary>
    /// 아이템 정보를 받아 UI를 엽니다.
    /// </summary>
    public void OpenInfo(StoreItem item, bool isStoreMode = true)
    {
        if (item == null || item.data == null) return;

        _currentItem = item;
        _isStoreMode = isStoreMode;
        _selectedQuantity = 1;

        // 기본 정보 설정
        _nameText.text = item.itemName;
        if (item.icon != null) _iconImage.sprite = item.icon;

        // 설명 및 영역 활성화 처리
        SetupDescription(item.data);
        
        // 수량 선택 영역은 항상 표시
        if (_quantityCanvasGroup != null)
        {
            _quantityCanvasGroup.alpha = 1f;
            _quantityCanvasGroup.interactable = true;
            _quantityCanvasGroup.blocksRaycasts = true;
        }

        // 제출 버튼 텍스트 및 활성화 설정
        if (_buyButtonText != null)
        {
            _buyButtonText.text = _isStoreMode ? "구매" : "폐기";
        }

        // 인벤토리 모드일 때는 폐기할 수 있는 최대 수량을 현재 보유량으로 설정
        if (!_isStoreMode && item.data is IngredientData ingredient)
        {
            _maxSelectableQuantity = InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID);
        }
        else
        {
            _maxSelectableQuantity = item.maxPurchaseAmount;
        }
        _maxAmount.SetText("최대 : " + _maxSelectableQuantity);

        // 레시피 상세 정보 처리
        if (item.data is FoodData foodData)
        {
            if (_recipeDetailsArea != null) _recipeDetailsArea.SetActive(true);
            PopulateRecipeRequirements(foodData);
        }
        else
        {
            if (_recipeDetailsArea != null) _recipeDetailsArea.SetActive(false);
        }

        UpdateQuantityUI();
        UpdateTotalPrice();
        UpdateOwnedAmount();

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private void SetupDescription(ScriptableObject data)
    {
        if (data is IngredientData ingredient) _descText.text = ingredient.description;
        else if (data is EquipmentData equipment) _descText.text = equipment.description;
        else if (data is FoodData food) _descText.text = "조리에 필요한 재료와 도구를 확인하세요.";
        else _descText.text = "";
    }

    private void PopulateRecipeRequirements(FoodData food)
    {
        if (_requirementsContainer == null || _requirementPrefab == null) return;

        foreach (Transform child in _requirementsContainer)
        {
            Destroy(child.gameObject);
        }

        if (food.requiredIngredients != null)
        {
            foreach (var ingredient in food.requiredIngredients)
            {
                if (ingredient == null) continue;
                var req = Instantiate(_requirementPrefab, _requirementsContainer);
                req.Setup(ingredient.ingredientSprite, ingredient.ingredientName);
            }
        }

        if (food.requiredEquipments != null)
        {
            foreach (var eqType in food.requiredEquipments)
            {
                if (eqType == EquipmentType.None) continue;
                
                EquipmentData eqData = StoreManager.Instance.EquipmentStore.GetAllEquipments().Find(x => x.type == eqType);
                if (eqData != null)
                {
                    var req = Instantiate(_requirementPrefab, _requirementsContainer);
                    req.Setup(eqData.equipmentSprite, eqData.equipmentName);
                }
            }
        }
    }

    private void ChangeQuantity(int delta)
    {
        if (_currentItem == null) return;
        
        _selectedQuantity = Mathf.Clamp(_selectedQuantity + delta, 1, _maxSelectableQuantity);
        UpdateQuantityUI();
        UpdateTotalPrice();
    }

    private void OnQuantityInputEndEdit(string text)
    {
        if (_currentItem == null) return;

        if (int.TryParse(text, out int value))
        {
            _selectedQuantity = Mathf.Clamp(value, 1, _maxSelectableQuantity);
        }
        else
        {
            _selectedQuantity = 1;
        }
        UpdateQuantityUI();
        UpdateTotalPrice();
    }

    private void UpdateQuantityUI()
    {
        if (_quantityInput != null)
            _quantityInput.text = _selectedQuantity.ToString();

        // 수량 범위에 따른 버튼 활성화/비활성화
        if (_minusButton != null) _minusButton.interactable = (_selectedQuantity > 1);
        if (_plusButton != null) _plusButton.interactable = (_selectedQuantity < _maxSelectableQuantity);
    }

    private void UpdateTotalPrice()
    {
        if (_currentItem == null) return;

        if (_priceText != null) _priceText.text = _currentItem.finalCost.ToString("N0");
        
        if (_totalPriceText != null)
        {
            int total = _currentItem.finalCost * _selectedQuantity;
            _totalPriceText.text = total.ToString("N0");
        }
    }

    private void UpdateOwnedAmount()
    {
        if (_currentItem == null || _ownedAmountText == null) return;

        bool isOwned = false;

        if (_currentItem.data is IngredientData ingredient)
        {
            int amount = InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID);
            _ownedAmountText.text = $"보유 중: {amount}";
            
            // 인벤토리 모드에서 실시간으로 최대 선택 가능 수량 동기화
            if (!_isStoreMode)
            {
                _maxSelectableQuantity = amount;
                _selectedQuantity = Mathf.Min(_selectedQuantity, _maxSelectableQuantity);
                UpdateQuantityUI();
            }
        }
        else if (_currentItem.data is EquipmentData equipment)
        {
            isOwned = StoreManager.Instance.EquipmentStore.HasEquipment(equipment.type);
            _ownedAmountText.text = isOwned ? "보유 중" : "미보유";
        }
        else if (_currentItem.data is FoodData food)
        {
            isOwned = StoreManager.Instance.RecipeStore.IsRecipeUnlocked(food.foodName);
            _ownedAmountText.text = isOwned ? "해금됨" : "잠김";
        }

        // 상점 모드일 때 이미 보유한 장비나 해금된 레시피는 구매 버튼 비활성화
        if (_buyButton != null && _isStoreMode)
        {
            // 재료는 여러 번 구매 가능하므로 제외, 장비와 레시피만 체크
            if (_currentItem.data is EquipmentData || _currentItem.data is FoodData)
            {
                _buyButton.interactable = !isOwned;
                if (isOwned && _buyButtonText != null) _buyButtonText.text = "이미 보유함";
            }
            else
            {
                _buyButton.interactable = true;
                if (_buyButtonText != null) _buyButtonText.text = "구매";
            }
        }
        else if (_buyButton != null && !_isStoreMode)
        {
            // 인벤토리 모드(폐기)일 때는 보유량이 0보다 클 때만 버튼 활성화
            if (_currentItem.data is IngredientData ing)
            {
                int currentTotal = InventoryManager.Instance.GetTotalAmount(ing.ingredientID);
                _buyButton.interactable = currentTotal > 0;
            }
        }
    }

    public void CloseUI()
    {
        if(_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _currentItem = null;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnClickSubmit() 
    {
        if (_currentItem == null) return;

        if (_isStoreMode)
        {
            // 상점 모드: 구매
            OnClickBuy();
        }
        else
        {
            // 인벤토리 모드: 폐기
            if (_currentItem.data is IngredientData ingredient)
            {
                InventoryManager.Instance.DiscardIngredients(ingredient.ingredientID, _selectedQuantity);
                Debug.Log($"[ItemInfoUI] {ingredient.ingredientName} {_selectedQuantity}개 폐기 완료.");
                
                // 수량이 0이 되면 닫기 (선택 사항)
                if (InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID) <= 0)
                {
                    CloseUI();
                }
            }
        }
    }

    void OnClickBuy()
    {
        if (_currentItem == null) return;
        StoreManager.Instance.TryBuyItem(_currentItem, _selectedQuantity);
        UpdateOwnedAmount();
    }

    public void OnClickSetAllAmount()
    {
        if (_currentItem == null) return;
        _selectedQuantity = _maxSelectableQuantity;
        UpdateQuantityUI();
        UpdateTotalPrice();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 배경 클릭 시 닫기
    }
}