using System.Collections.Generic;
using UnityEngine;

public class StoreUIController : MonoBehaviour, MarketUIInterface
{
    [SerializeField] CanvasGroup storeUIPanel;
    [SerializeField] CanvasGroup _marketGroup;
    [SerializeField] CanvasGroup _recipeGroup;
    [SerializeField] CanvasGroup _decorationGroup;
    [SerializeField] CanvasGroup _marketingGroup;

    [Header("Slot Content Parents (ScrollView Content)")]
    [SerializeField] private Transform _marketContent;
    [SerializeField] private Transform _recipeContent;
    [SerializeField] private Transform _decorationContent;
    [SerializeField] private Transform _marketingContent;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoUI _itemInfoUI;

    private CanvasGroup[] _categoryGroups;
    private Transform[] _contentParents;

    private int _currentCategoryIndex = -1;
    private List<StoreItemSlotUI> _slotPool = new List<StoreItemSlotUI>();

    private void Awake()
    {
        _categoryGroups = new CanvasGroup[] { 
            _marketGroup, _recipeGroup, _decorationGroup, _marketingGroup
        };
        _contentParents = new Transform[] { 
            _marketContent, _recipeContent, _decorationContent, _marketingContent
        };
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

        Time.timeScale = 0f;

        // 기본 카테고리(시장)로 시작
        ChangeCategory(0);
    }

    public void CloseUI()
    {
        storeUIPanel.alpha = 0f;
        storeUIPanel.interactable = false;
        storeUIPanel.blocksRaycasts = false;
        Time.timeScale = 1f;

        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
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
