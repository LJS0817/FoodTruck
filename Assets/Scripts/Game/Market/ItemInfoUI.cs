using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 상점, 인벤토리, 레시피북 등에서 아이템이나 레시피의 상세 정보를 표시하는 범용 UI 클래스입니다.
/// </summary>
public class ItemInfoUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _ownedAmountText; // 현재 보유량 표시
    [SerializeField] private TMP_Text _expirationText;  // 유통기한 표시
    [SerializeField] private Button _submitButton;
    [SerializeField] private Button _discardButton;
    [SerializeField] private TMP_Text _submitButtonText; // Submit 버튼 텍스트
    [SerializeField] private AmountSetter _amountSetter; // 💡 수량 조절
    [SerializeField] private TMP_Text _valueText; // 가치(가격) 표시

    [Header("Recipe Details")]
    [SerializeField] private GameObject _recipeDetailsArea; // 레시피 전용 영역
    [SerializeField] private Transform _requirementsContainer; // 재료/도구 아이콘이 생성될 부모
    [SerializeField] private RecipeRequirementUI _requirementPrefab;

    private StoreItem _currentItem;
    private bool _isStoreMode = true;
    private CanvasGroup _canvasGroup;
    RectTransform _rectTransform;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = transform.GetChild(1).GetComponent<RectTransform>();

        if (_submitButton != null)
            _submitButton.onClick.AddListener(OnClickSubmit);
        else
            Debug.LogError("[ItemInfoUI] _submitButton이 할당되지 않았습니다! 인스펙터에서 버튼을 연결해주세요.");

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
    private InventoryItem _currentInventoryItem;

    public void OpenInfo(StoreItem item, bool isStoreMode = true, System.Action<StoreItem, int> onBuy = null, InventoryItem inventoryItem = null)
    {
        if (item == null || item.data == null) return;

        _currentItem = item;
        _currentInventoryItem = inventoryItem;
        _isStoreMode = isStoreMode;
        onBuyAction = onBuy;

        // 기본 정보 설정
        _nameText.text = item.itemName;
        
        Sprite displaySprite = item.icon;
        if (!_isStoreMode && _currentInventoryItem != null && _currentInventoryItem.processType != ProcessType.None && _currentInventoryItem.data != null)
        {
            ProcessMethodData method = _currentInventoryItem.data.GetProcessMethod(_currentInventoryItem.processType);
            if (method != null)
            {
                var stateEntry = method.stateSteps.Find(s => s.state == _currentInventoryItem.state);
                if (stateEntry != null && stateEntry.stateSprite != null)
                    displaySprite = stateEntry.stateSprite;
            }
        }
        _iconImage.sprite = displaySprite;

        // 설명 및 영역 활성화 처리
        SetupDescription(item.data);

        // 레시피 상세 정보 처리
        if (item.data is FoodData foodData && item.itemType != StoreItemType.RecipeIngredientSet)
        {
            bool isOwned = StoreManager.Instance.RecipeStore.IsRecipeUnlocked(foodData.foodName);
            if (isOwned)
            {
                if (_recipeDetailsArea != null) _recipeDetailsArea.SetActive(true);
                PopulateRecipeRequirements(foodData);
            }
            else
            {
                if (_recipeDetailsArea != null) _recipeDetailsArea.SetActive(false);
            }
        }
        else
        {
            if (_recipeDetailsArea != null) _recipeDetailsArea.SetActive(false);
        }

        // 유통기한 표시 처리
        if (_expirationText != null)
        {
            if (item.data is IngredientData ingredient)
            {
                _expirationText.transform.parent.gameObject.SetActive(true);
                if (!_isStoreMode && _currentInventoryItem != null)
                {
                    _expirationText.text = $"남은 유통기한: {_currentInventoryItem.remainingDays}일";
                }
                else
                {
                    _expirationText.text = $"유통기한: {ingredient.maxShelfLifeDays}일";
                }
            }
            else
            {
                _expirationText.transform.parent.gameObject.SetActive(false);
            }
        }

        int baseValue = item.finalCost;
        if (item.data is IngredientData ingredientData) baseValue = ingredientData.basePrice;
        else if (item.data is EquipmentData equipmentData) baseValue = equipmentData.price;
        else if (item.data is FoodData foodDataContent)
        {
            if (item.itemType == StoreItemType.RecipeIngredientSet)
                baseValue = item.finalCost; // 세트 가격은 finalCost에 이미 설정되어 있음
            else
                baseValue = foodDataContent.basePrice;
        }

        if (_valueText != null)
        {
            _valueText.gameObject.SetActive(true);
            _valueText.text = baseValue.ToString("N0");
        }

        if(_discardButton.gameObject.activeSelf && isStoreMode) _discardButton.gameObject.SetActive(false);
        else if(!_discardButton.gameObject.activeSelf && !isStoreMode) _discardButton.gameObject.SetActive(true);
        UpdateOwnedAmount();

        if (_amountSetter != null)
        {
            if (item.data is IngredientData ing)
            {
                _amountSetter.gameObject.SetActive(true);
                int maxAmount = _isStoreMode ? _currentItem.maxPurchaseAmount : 99;
                
                if (!isStoreMode)
                {
                    maxAmount = InventoryManager.Instance.GetTotalAmount(ing.ingredientID, false);
                    if (maxAmount < 1) maxAmount = 1;
                }
                else
                {
                    // 💡 유저 피드백 반영: 상점 모드일 때 현재 돈으로 살 수 있는 최대 수량으로 Max 설정
                    if (baseValue > 0)
                    {
                        int affordable = PlayerManager.Instance.CurrentMoney / baseValue;
                        if (affordable < 1) affordable = 1;
                        if (maxAmount > affordable) maxAmount = affordable;
                    }
                }
                _amountSetter.SetAmountInfo(baseValue, maxAmount, _isStoreMode);
            }
            else if (item.itemType == StoreItemType.RecipeIngredientSet)
            {
                _amountSetter.gameObject.SetActive(true);
                int maxAmount = _isStoreMode ? _currentItem.maxPurchaseAmount : 99;
                if (maxAmount <= 0) maxAmount = 99; // Set default max amount for sets
                
                if (_isStoreMode && baseValue > 0)
                {
                    int affordable = PlayerManager.Instance.CurrentMoney / baseValue;
                    if (affordable < 1) affordable = 1;
                    if (maxAmount > affordable) maxAmount = affordable;
                }

                _amountSetter.SetAmountInfo(baseValue, maxAmount, _isStoreMode);
            }
            else
            {
                _amountSetter.gameObject.SetActive(false);
            }
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    
        StartCoroutine(RebuildLayoutRoutine());
    }

    private IEnumerator RebuildLayoutRoutine()
    {
        // 1. 첫 프레임 대기 (UI 텍스트 및 자식 객체 할당 반영)
        yield return null; 
        
        // 2. 모든 캔버스 강제 업데이트 (TMPro 메쉬 생성 및 기본 레이아웃 계산)
        Canvas.ForceUpdateCanvases();
        
        // 3. 타겟 레이아웃 강제 갱신
        if (_rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        // 4. 중첩된 LayoutGroup의 크기 계산 지연(버그)을 방지하기 위해 한 프레임 더 대기 후 최종 갱신
        yield return null;
        if (_rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }

    private void SetupDescription(ScriptableObject data)
    {
        if (data is IngredientData ingredient) 
        {
            int boxAmount = 0;
            if (IngredientManager.Instance != null)
            {
                foreach (var box in IngredientManager.Instance.GetAllBoxes())
                {
                    if (box.currentAmount > 0 && box.GetCurrentData() != null && box.GetCurrentData().ingredientID == ingredient.ingredientID)
                    {
                        if (!_isStoreMode && _currentInventoryItem != null)
                        {
                            ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                            if (box.targetState == _currentInventoryItem.state && 
                                box.targetProcess == _currentInventoryItem.processType &&
                                boxGrade == _currentInventoryItem.grade)
                            {
                                boxAmount += box.currentAmount;
                            }
                        }
                        else
                        {
                            boxAmount += box.currentAmount;
                        }
                    }
                }
            }

            if (boxAmount > 0)
            {
                _descText.text = $"<color=#00FF00>[현재 상자에 {boxAmount}개 세팅됨]</color>\n\n{ingredient.description}";
            }
            else
            {
                _descText.text = ingredient.description;
            }
        }
        else if (data is EquipmentData equipment) _descText.text = equipment.description;
        else if (data is FoodData food) 
        {
            if (_currentItem != null && _currentItem.itemType == StoreItemType.RecipeIngredientSet)
            {
                _descText.text = $"<color=#FFD700>[레시피 재료 세트]</color>\n\n{food.foodName} 요리에 필요한 모든 재료를 한 번에 구매합니다.\n(한 세트당 10회 분량)";
            }
            else
            {
                _descText.text = string.IsNullOrEmpty(food.description) ? "조리에 필요한 재료와 도구를 확인하세요." : food.description;
            }
        }
        else _descText.text = "";
    }

    private void PopulateRecipeRequirements(FoodData food)
    {
        if (_requirementsContainer == null || _requirementPrefab == null) return;

        foreach (Transform child in _requirementsContainer)
        {
            Destroy(child.gameObject);
        }

        if (food.ingredientConfigs != null)
        {
            foreach (var config in food.ingredientConfigs)
            {
                if (config.rawIngredient == null) continue;
                var req = Instantiate(_requirementPrefab, _requirementsContainer);
                
                // 💡 유저 피드백 반영: 가공해야 하는 재료면 앞에 [ 상태 ] 를 붙여서 표기
                string reqName = config.processType != ProcessType.None 
                                ? $"[ {config.processType} ] {config.rawIngredient.ingredientName}" 
                                : config.rawIngredient.ingredientName;
                
                req.Setup(config.rawIngredient.ingredientSprite, reqName);
            }
        }

        if (food.requiredEquipments != null)
        {
            foreach (var eqType in food.requiredEquipments)
            {
                if (eqType == EquipmentType.None) continue;
                
                EquipmentData eqData = EquipmentStoreManager.Instance.GetAllEquipments().Find(x => x.type == eqType);
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
            int amount = 0;
            if (!_isStoreMode && _currentInventoryItem != null)
            {
                // 인벤토리 모드에서는 상태, 가공방식, 품질, 남은 유통기한이 모두 동일한 항목만 계산
                foreach (var invItem in InventoryManager.Instance.inventoryItems)
                {
                    if (invItem.data.ingredientID == ingredient.ingredientID &&
                        invItem.state == _currentInventoryItem.state &&
                        invItem.processType == _currentInventoryItem.processType &&
                        invItem.grade == _currentInventoryItem.grade &&
                        invItem.remainingDays == _currentInventoryItem.remainingDays)
                    {
                        amount += invItem.amount;
                    }
                }
            }
            else
            {
                amount = InventoryManager.Instance.GetTotalAmount(ingredient.ingredientID, false);
            }
            _ownedAmountText.text = $"X {amount}";
        }
        else if (_currentItem.data is EquipmentData equipment)
        {
            isOwned = EquipmentStoreManager.Instance.HasEquipment(equipment);
            _ownedAmountText.text = isOwned ? "보유 중" : "미보유";
        }
        else if (_currentItem.data is FoodData food)
        {
            isOwned = StoreManager.Instance.RecipeStore.IsRecipeUnlocked(food.foodName);
            if (_currentItem.itemType == StoreItemType.RecipeIngredientSet)
                _ownedAmountText.text = "재료 세트";
            else
                _ownedAmountText.text = isOwned ? "해금됨" : "잠김";
        }

        // 💡 텍스트 길이 변경 시 레이아웃(ContentSizeFitter 등) 즉시 갱신
        if (_ownedAmountText != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_ownedAmountText.rectTransform);
            if (_ownedAmountText.transform.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_ownedAmountText.transform.parent.GetComponent<RectTransform>());
            }
        }

        if (_isStoreMode)
        {
            if (_discardButton != null) _discardButton.gameObject.SetActive(false);

            if (_submitButton != null)
            {
                if (_currentItem.data is EquipmentData || (_currentItem.data is FoodData && _currentItem.itemType != StoreItemType.RecipeIngredientSet))
                {
                    _submitButton.gameObject.SetActive(true);
                    _submitButton.interactable = !isOwned;
                    if (_submitButtonText != null)
                    {
                        _submitButtonText.text = isOwned ? "이미 보유함" : "구매";
                    }
                }
                else
                {
                    _submitButton.gameObject.SetActive(true);
                    _submitButton.interactable = true;
                    if (_submitButtonText != null) _submitButtonText.text = "구매";
                }
            }
        }
        else
        {
            if (_currentItem.data is IngredientData ing)
            {
                int currentTotal = InventoryManager.Instance.GetTotalAmount(ing.ingredientID, true);
                
                if (_submitButton != null)
                {
                    // 일반 인벤토리에서는 적용 버튼을 숨깁니다.
                    _submitButton.gameObject.SetActive(false);
                }

                if (_discardButton != null)
                {
                    _discardButton.gameObject.SetActive(currentTotal > 0);
                    _discardButton.interactable = true;
                }
            }
            else
            {
                if (_submitButton != null) _submitButton.gameObject.SetActive(false);
                if (_discardButton != null) _discardButton.gameObject.SetActive(false);
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
        Debug.Log("[ItemInfoUI] OnClickSubmit 호출됨");
        if (_currentItem == null) 
        {
            Debug.LogWarning("[ItemInfoUI] _currentItem이 null이라 구매가 취소되었습니다.");
            return;
        }

        if (_amountSetter != null && _amountSetter.gameObject.activeSelf)
        {
            ExecuteSubmitAction(_amountSetter.CurrentAmount);
        }
        else
        {
            ExecuteSubmitAction(1);
        }
    }

    private void ExecuteSubmitAction(int amount)
    {
        Debug.Log($"[ItemInfoUI] ExecuteSubmitAction 호출됨 (amount: {amount})");
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
        if (_amountSetter != null && _currentItem != null)
        {
            if (_currentItem.data is IngredientData ingredient)
            {
                int amountToDiscard = _amountSetter.CurrentAmount;

                if (!_isStoreMode && _currentInventoryItem != null)
                {
                    if (amountToDiscard > _currentInventoryItem.amount) amountToDiscard = _currentInventoryItem.amount;
                    
                    InventoryManager.Instance.DiscardItem(_currentInventoryItem, amountToDiscard);
                    Debug.Log($"[ItemInfoUI] {_currentInventoryItem.data.ingredientName} {amountToDiscard}개 폐기 완료.");
                    
                    if (_currentInventoryItem.amount <= 0)
                    {
                        CloseUI();
                    }
                    else
                    {
                        UpdateOwnedAmount();
                        _amountSetter.SetAmountInfo(ingredient.basePrice, _currentInventoryItem.amount, _isStoreMode);
                    }
                }
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
}