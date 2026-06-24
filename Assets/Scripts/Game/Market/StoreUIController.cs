using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreUIController : MonoBehaviour, MarketUIInterface
{
    [SerializeField] CanvasGroup storeUIPanel;
    [SerializeField] private ScrollRect scrollRect;
    private CanvasGroup _marketGroup;
    private CanvasGroup _recipeGroup;
    private CanvasGroup _decorationGroup;
    private CanvasGroup _marketingGroup;
    private CanvasGroup _equipmentGroup; // 에디터에서 할당 필요
    private CanvasGroup _recipeSetGroup; // 에디터에서 할당 필요

    [Header("Slot Content Parents (ScrollView Content)")]
    [SerializeField] private Transform _marketContent;
    [SerializeField] private Transform _recipeContent;
    [SerializeField] private Transform _decorationContent;
    [SerializeField] private Transform _marketingContent;
    [SerializeField] private Transform _equipmentContent; // 에디터에서 할당 필요
    [SerializeField] private Transform _recipeSetContent; // 에디터에서 할당 필요

    [SerializeField] private Button _marketTabButton;
    [SerializeField] private Button _recipeTabButton;
    [SerializeField] private Button _decorationTabButton;
    [SerializeField] private Button _marketingTabButton;
    [SerializeField] private Button _equipmentTabButton;
    [SerializeField] private Button _recipeSetTabButton;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoUI _itemInfoUI;
    [SerializeField] private TradeInPopupUI _tradeInPopupUI;

    private CanvasGroup[] _categoryGroups;
    private Transform[] _contentParents;

    private int _currentCategoryIndex = -1;
    private List<StoreItemSlotUI> _slotPool = new List<StoreItemSlotUI>();

    public void OpenTradeInPopup(EquipmentData equipment, int normalCost, int tradeInCost)
    {
        if (_tradeInPopupUI != null)
        {
            _tradeInPopupUI.OpenPopup(equipment, normalCost, tradeInCost);
        }
        else
        {
            Debug.LogWarning("[StoreUIController] TradeInPopupUI가 연결되어 있지 않습니다. 기본적으로 일반 구매를 진행합니다.");
            StoreManager.Instance.ExecuteEquipmentPurchase(equipment, false);
        }
    }

    private void Awake()
    {
        _marketGroup = _marketContent != null ? _marketContent.GetComponent<CanvasGroup>() : null;
        _recipeGroup = _recipeContent != null ? _recipeContent.GetComponent<CanvasGroup>() : null;
        _decorationGroup = _decorationContent != null ? _decorationContent.GetComponent<CanvasGroup>() : null;
        _marketingGroup = _marketingContent != null ? _marketingContent.GetComponent<CanvasGroup>() : null;
        _equipmentGroup = _equipmentContent != null ? _equipmentContent.GetComponent<CanvasGroup>() : null;
        _recipeSetGroup = _recipeSetContent != null ? _recipeSetContent.GetComponent<CanvasGroup>() : null;

        _categoryGroups = new CanvasGroup[] { 
            _marketGroup, _recipeGroup, _equipmentGroup, _decorationGroup, _marketingGroup, _recipeSetGroup
        };
        _contentParents = new Transform[] { 
            _marketContent, _recipeContent, _equipmentContent, _decorationContent, _marketingContent, _recipeSetContent
        };

        if (_marketTabButton != null) _marketTabButton.onClick.AddListener(() => ChangeCategory(0));
        if (_recipeTabButton != null) _recipeTabButton.onClick.AddListener(() => ChangeCategory(1));
        if (_equipmentTabButton != null) _equipmentTabButton.onClick.AddListener(() => ChangeCategory(2));
        if (_decorationTabButton != null) _decorationTabButton.onClick.AddListener(() => ChangeCategory(3));
        if (_marketingTabButton != null) _marketingTabButton.onClick.AddListener(() => ChangeCategory(4));
        if (_recipeSetTabButton != null) _recipeSetTabButton.onClick.AddListener(() => ChangeCategory(5));

        CloseUI();
    }

    public void OpenUI()
    {
        if(_currentCategoryIndex == -1)
        {
            for (int i = 0; i < _categoryGroups.Length; i++)
            {
                if (_categoryGroups[i] == null) continue;

                _categoryGroups[i].alpha = 0f;
                _categoryGroups[i].interactable = false;
                _categoryGroups[i].blocksRaycasts = false;
            }
        }

        storeUIPanel.alpha = 1f;
        storeUIPanel.interactable = true;
        storeUIPanel.blocksRaycasts = true;

        // Time.timeScale 제어는 MainUINavigationController에서 일괄 수행합니다.

        // 기본 카테고리(시장)로 시작
        ChangeCategory(0);
    }

    public void CloseUI()
    {
        storeUIPanel.alpha = 0f;
        storeUIPanel.interactable = false;
        storeUIPanel.blocksRaycasts = false;

        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
        if (_tradeInPopupUI != null)
        {
            _tradeInPopupUI.ClosePopup();
        }
    }

    public void SetVisibleCategory(int categoryIndex, bool isActive)
    {
        if(categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return;
        _categoryGroups[categoryIndex].alpha = isActive ? 1f : 0f;
        _categoryGroups[categoryIndex].interactable = isActive;
        _categoryGroups[categoryIndex].blocksRaycasts = isActive;
    }

    public void ChangeCategory(int categoryIndex)
    {
        if (_currentCategoryIndex == categoryIndex) return;

        SetVisibleCategory(_currentCategoryIndex, false);
        _currentCategoryIndex = categoryIndex;
        SetVisibleCategory(_currentCategoryIndex, true);
        scrollRect.content = _contentParents[categoryIndex] as RectTransform;

        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
    }

    public void ShowItemInfo(StoreItem item, bool isStoreMode = true)
    {
        if (_itemInfoUI == null || item == null) return;
        _itemInfoUI.OpenInfo(item, isStoreMode, StoreManager.Instance.TryBuyItem);
    }

    public void RefreshUI()
    {
        StoreManager.Instance.PopulateAllCategories();
    }

    public Transform GetContentParent(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= _contentParents.Length) return null;
        return _contentParents[categoryIndex];
    }

    public StoreItemSlotUI GetOrCreateSlot(StoreItemSlotUI prefab, Transform parent)
    {
        for (int i = 0; i < _slotPool.Count; i++)
        {
            if (!_slotPool[i].gameObject.activeSelf)
            {
                _slotPool[i].transform.SetParent(parent, false);
                _slotPool[i].transform.SetAsLastSibling();
                _slotPool[i].gameObject.SetActive(true);
                return _slotPool[i];
            }
        }

        StoreItemSlotUI newSlot = Instantiate(prefab, parent);
        _slotPool.Add(newSlot);
        return newSlot;
    }

    public void ClearSlots(Transform parent)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            StoreItemSlotUI slot = child.GetComponent<StoreItemSlotUI>();
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
