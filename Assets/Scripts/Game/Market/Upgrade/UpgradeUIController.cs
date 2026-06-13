using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIController : MonoBehaviour, MarketUIInterface
{
    [SerializeField] CanvasGroup upgradeUIPanel;
    [SerializeField] CanvasGroup _equipmentGroup;
    [SerializeField] CanvasGroup _upgradeGroup;
    [SerializeField] ScrollRect _scrollView;

    [Header("Category Buttons")]
    [SerializeField] private Button _equipmentCategoryBtn;
    [SerializeField] private Button _upgradeCategoryBtn;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoUI _itemInfoUI;
    [SerializeField] private UpgradeInfoUI _upgradeInfoUI;

    private CanvasGroup[] _categoryGroups;

    private int _currentCategoryIndex = -1;
    private List<UpgradeItemSlotUI> _slotPool = new List<UpgradeItemSlotUI>();

    private void Awake()
    {
        _categoryGroups = new CanvasGroup[] { 
            _equipmentGroup, _upgradeGroup
        };

        if (_equipmentCategoryBtn != null) _equipmentCategoryBtn.onClick.AddListener(() => ChangeCategory(0));
        if (_upgradeCategoryBtn != null) _upgradeCategoryBtn.onClick.AddListener(() => ChangeCategory(1));

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

        // Time.timeScale 제어는 MainUINavigationController에서 일괄 수행합니다.

        // 기본 카테고리(장비)로 시작
        ChangeCategory(0);
    }

    public void CloseUI()
    {
        upgradeUIPanel.alpha = 0f;
        upgradeUIPanel.interactable = false;
        upgradeUIPanel.blocksRaycasts = false;
        // Time.timeScale 제어는 MainUINavigationController에서 일괄 수행합니다.

        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
        if (_upgradeInfoUI != null)
        {
            _upgradeInfoUI.CloseUI();
        }
    }

    public void SetVisibleCategory(int categoryIndex, bool isActive)
    {
        if(categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return;
        if (_categoryGroups[categoryIndex] != null)
        {
            _categoryGroups[categoryIndex].alpha = isActive ? 1f : 0f;
            _categoryGroups[categoryIndex].interactable = isActive;
            _categoryGroups[categoryIndex].blocksRaycasts = isActive;
        }
    }

    public void ChangeCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return;
        if (_currentCategoryIndex == categoryIndex) return;

        SetVisibleCategory(_currentCategoryIndex, false);
        _currentCategoryIndex = categoryIndex;
        SetVisibleCategory(_currentCategoryIndex, true);

        _scrollView.content = _categoryGroups[categoryIndex].transform as RectTransform;

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

    public void ShowUpgradeInfo(StoreItem item)
    {
        if (_upgradeInfoUI == null || item == null) return;
        _upgradeInfoUI.OpenInfo(item);
    }

    public void AddEquipmentSlot(StoreItem item)
    {
        Transform parent = GetContentParent(0);
        if (parent == null) return;
        UpgradeItemSlotUI slot = GetOrCreateSlot(UpgradeManager.Instance.SlotPrefab, parent);
        slot.Setup(item, (i) => ShowUpgradeInfo(i));
    }

    public void UpdateEquipmentSlot(EquipmentData equipmentData)
    {
        Transform parent = GetContentParent(0);
        if (parent == null || equipmentData == null) return;

        int newLevel = EquipmentStoreManager.Instance.GetEquipmentLevel(equipmentData);
        StoreItem updatedItem = StoreItem.FromEquipmentLevel(equipmentData, newLevel);

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            UpgradeItemSlotUI slot = child.GetComponent<UpgradeItemSlotUI>();
            if (slot != null && slot.gameObject.activeSelf && slot.Item != null && slot.Item.data is EquipmentData eq && eq.type == equipmentData.type)
            {
                slot.Setup(updatedItem, (i) => ShowUpgradeInfo(i));
                break;
            }
        }
    }

    public void RefreshUI()
    {
        UpgradeManager.Instance.PopulateAllCategories();
    }

    public Transform GetContentParent(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return null;
        return _categoryGroups[categoryIndex] != null ? _categoryGroups[categoryIndex].transform : null;
    }

    public UpgradeItemSlotUI GetOrCreateSlot(UpgradeItemSlotUI prefab, Transform parent)
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

        UpgradeItemSlotUI newSlot = Instantiate(prefab, parent);
        _slotPool.Add(newSlot);
        return newSlot;
    }

    public void ClearSlots(Transform parent)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            UpgradeItemSlotUI slot = child.GetComponent<UpgradeItemSlotUI>();
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}

