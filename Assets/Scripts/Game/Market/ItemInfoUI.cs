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
    [SerializeField] private TMP_Text _ownedAmountText; // 현재 보유량 표시
    [SerializeField] private Button _submitButton;
    [SerializeField] private Button _discardButton;
    [SerializeField] private TMP_Text _submitButtonText; // Submit 버튼 텍스트
    [SerializeField] private AmountSetter _amountSetter; // 💡 수량 조절 팝업

    [Header("Recipe Details")]
    [SerializeField] private GameObject _recipeDetailsArea; // 레시피 전용 영역
    [SerializeField] private Transform _requirementsContainer; // 재료/도구 아이콘이 생성될 부모
    [SerializeField] private RecipeRequirementUI _requirementPrefab;

    private StoreItem _currentItem;
    private bool _isStoreMode = true;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_submitButton != null)
            _submitButton.onClick.AddListener(OnClickSubmit);

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
    public System.Action<StoreItem, int> onBuyAction;

    public void OpenInfo(StoreItem item, bool isStoreMode = true, System.Action<StoreItem, int> onBuy = null)
    {
        if (item == null || item.data == null) return;

        _currentItem = item;
        _isStoreMode = isStoreMode;
        onBuyAction = onBuy;

        // 기본 정보 설정
        _nameText.text = item.itemName;
        if (item.icon != null) _iconImage.sprite = item.icon;

        // 설명 및 영역 활성화 처리
        SetupDescription(item.data);

        // 제출 버튼 텍스트 및 활성화 설정
        if (_submitButtonText != null)
        {
            _submitButtonText.text = _isStoreMode ? "구매" : "적용";
        }

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

        if(_discardButton.gameObject.activeSelf && isStoreMode) _discardButton.gameObject.SetActive(false);
        else if(!_discardButton.gameObject.activeSelf && !isStoreMode) _discardButton.gameObject.SetActive(true);

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
                
                EquipmentData eqData = UpgradeManager.Instance.EquipmentStore.GetAllEquipments().Find(x => x.type == eqType);
                if (eqData != null)
                {
                    var req = Instantiate(_requirementPrefab, _requirementsContainer);
                    req.Setup(eqData.equipmentSprite, eqData.equipmentName);
                }
            }
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
        }
        else if (_currentItem.data is EquipmentData equipment)
        {
            isOwned = UpgradeManager.Instance.EquipmentStore.HasEquipment(equipment.type);
            _ownedAmountText.text = isOwned ? "보유 중" : "미보유";
        }
        else if (_currentItem.data is FoodData food)
        {
            isOwned = StoreManager.Instance.RecipeStore.IsRecipeUnlocked(food.foodName);
            _ownedAmountText.text = isOwned ? "해금됨" : "잠김";
        }

        // 상점 모드일 때 이미 보유한 장비나 해금된 레시피는 구매 버튼 비활성화
        if (_submitButton != null && _isStoreMode)
        {
            // 재료는 여러 번 구매 가능하므로 제외, 장비와 레시피만 체크
            if(_discardButton.gameObject.activeSelf) _discardButton.gameObject.SetActive(false);
            if (_currentItem.data is EquipmentData || _currentItem.data is FoodData)
            {
                _submitButton.interactable = !isOwned;
                if (isOwned && _submitButtonText != null) _submitButtonText.text = "이미 보유함";
            }
            else
            {
                _submitButton.interactable = true;
                if (_submitButtonText != null) _submitButtonText.text = "구매";
            }
        }
        else if (_discardButton != null && !_isStoreMode)
        {
            if(!_discardButton.gameObject.activeSelf) _discardButton.gameObject.SetActive(true);
            // 인벤토리 모드(폐기)일 때는 보유량이 0보다 클 때만 버튼 활성화
            if (_currentItem.data is IngredientData ing)
            {
                int currentTotal = InventoryManager.Instance.GetTotalAmount(ing.ingredientID);
                _discardButton.interactable = currentTotal > 0;
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

        if (_amountSetter != null)
        {
            int basePrice = _isStoreMode ? _currentItem.finalCost : 0;
            _amountSetter.Open(_currentItem.maxPurchaseAmount, basePrice, (amount) => {
                ExecuteSubmitAction(amount);
            });
        }
        else
        {
            ExecuteSubmitAction(1);
        }
    }

    private void ExecuteSubmitAction(int amount)
    {
        if (_isStoreMode)
        {
            // 상점 모드: 구매
            OnClickBuy(amount);
        }
        else
        {
            // 인벤토리 모드: 적용
            InventoryManager.Instance.OnClickApply(amount);
        }
        UpdateOwnedAmount();
        CloseUI();
    }

    public void OnClickDiscard()
    {
        if (_amountSetter != null)
        {
            if (_currentItem.data is IngredientData ingredient)
            {
                int currentTotal = InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID);
                _amountSetter.Open(currentTotal, 0, (amount) => {
                    InventoryManager.Instance.DiscardIngredients(ingredient.ingredientID, amount);
                    Debug.Log($"[ItemInfoUI] {ingredient.ingredientName} {amount}개 폐기 완료.");
                    
                    // 수량이 0이 되면 닫기 (선택 사항)
                    if (InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID) <= 0)
                    {
                        CloseUI();
                    }
                });
            }
        }
    }

    void OnClickBuy(int amount)
    {
        if (_currentItem == null) return;
        
        if (onBuyAction != null) 
            onBuyAction(_currentItem, amount);
        else 
            StoreManager.Instance.TryBuyItem(_currentItem, amount);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 배경 클릭 시 닫기
    }
}