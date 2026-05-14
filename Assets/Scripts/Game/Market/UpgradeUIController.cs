using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIController : MonoBehaviour, MarketUIInterface
{
    [SerializeField] CanvasGroup upgradeUIPanel;
    [SerializeField] CanvasGroup _equipmentGroup;
    [SerializeField] CanvasGroup _workerGroup;
    [SerializeField] CanvasGroup _districtGroup;
    [SerializeField] CanvasGroup _upgradeGroup;

    [Header("Slot Content Parents (ScrollView Content)")]
    [SerializeField] private Transform _equipmentContent;
    [SerializeField] private Transform _workerContent;
    [SerializeField] private Transform _districtContent;
    [SerializeField] private Transform _upgradeContent;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoUI _itemInfoUI;

    private CanvasGroup[] _categoryGroups;
    private Transform[] _contentParents;

    private int _currentCategoryIndex = -1;
    private List<StoreItemSlotUI> _slotPool = new List<StoreItemSlotUI>();

    private void Awake()
    {
        _categoryGroups = new CanvasGroup[] { 
            _equipmentGroup, _workerGroup, _districtGroup, _upgradeGroup
        };
        _contentParents = new Transform[] { 
            _equipmentContent, _workerContent, _districtContent, _upgradeContent
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
        

        upgradeUIPanel.alpha = 1f;
        upgradeUIPanel.interactable = true;
        upgradeUIPanel.blocksRaycasts = true;

        Time.timeScale = 0f;

        // 기본 카테고리(장비)로 시작
        ChangeCategory(0);
    }

    public void CloseUI()
    {
        upgradeUIPanel.alpha = 0f;
        upgradeUIPanel.interactable = false;
        upgradeUIPanel.blocksRaycasts = false;
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
        if (categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return;
        if (_currentCategoryIndex == categoryIndex) return;

        SetVisibleCategory(_currentCategoryIndex, false);
        _currentCategoryIndex = categoryIndex;
        SetVisibleCategory(_currentCategoryIndex, true);
        // for (int i = 0; i < _categoryGroups.Length; i++)
        // {
        //     if (_categoryGroups[i] == null) continue;

        //     bool isActive = (i == categoryIndex);
        //     _categoryGroups[i].alpha = isActive ? 1f : 0f;
        //     _categoryGroups[i].interactable = isActive;
        //     _categoryGroups[i].blocksRaycasts = isActive;
        // }

        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
    }

    public void ShowItemInfo(StoreItem item, bool isStoreMode = true)
    {
        if (_itemInfoUI == null || item == null) return;
        _itemInfoUI.OpenInfo(item, isStoreMode, UpgradeManager.Instance.TryBuyUpgrade);
    }

    public void RefreshUI()
    {
        UpgradeManager.Instance.PopulateAllCategories();
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
