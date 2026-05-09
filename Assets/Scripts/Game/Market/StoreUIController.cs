using System.Collections.Generic;
using UnityEngine;

public class StoreUIController : MonoBehaviour
{
    [SerializeField] CanvasGroup storeUIPanel;
    [SerializeField] CanvasGroup _equipmentGroup;
    [SerializeField] CanvasGroup _marketGroup;
    [SerializeField] CanvasGroup _recipeGroup;

    [Header("Slot Content Parents (ScrollView Content)")]
    [SerializeField] private Transform _marketContent;
    [SerializeField] private Transform _equipmentContent;
    [SerializeField] private Transform _recipeContent;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoUI _itemInfoUI;

    // 💡 GC 방지: 배열로 캐싱하여 ChangeCategory에서 반복문 사용
    private CanvasGroup[] _categoryGroups;
    private Transform[] _contentParents;

    private int _currentCategoryIndex = -1;

    private void Awake()
    {
        _categoryGroups = new CanvasGroup[] { _marketGroup, _equipmentGroup, _recipeGroup };
        _contentParents = new Transform[] { _marketContent, _equipmentContent, _recipeContent };
        CloseUI();
    }

    public void OpenUI()
    {
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

        // 정보 패널도 함께 닫기
        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
    }

    /// <summary>
    /// 카테고리 탭 전환. 선택된 CanvasGroup만 보이도록 alpha를 제어합니다.
    /// 0: 시장(식재료), 1: 장비, 2: 레시피
    /// </summary>
    public void ChangeCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= _categoryGroups.Length) return;
        if (_currentCategoryIndex == categoryIndex) return;

        _currentCategoryIndex = categoryIndex;

        for (int i = 0; i < _categoryGroups.Length; i++)
        {
            if (_categoryGroups[i] == null) continue;

            bool isActive = (i == categoryIndex);
            _categoryGroups[i].alpha = isActive ? 1f : 0f;
            _categoryGroups[i].interactable = isActive;
            _categoryGroups[i].blocksRaycasts = isActive;
        }

        // 카테고리 변경 시 정보 패널 닫기
        if (_itemInfoUI != null)
        {
            _itemInfoUI.CloseUI();
        }
    }

    /// <summary>
    /// ShopItemSlotUI에서 슬롯 클릭 시 호출됩니다.
    /// 아이템 정보를 ItemInfoUI에 전달하여 표시합니다.
    /// </summary>
    public void ShowItemInfo(StoreItem item, bool isStoreMode = true)
    {
        if (_itemInfoUI == null || item == null) return;
        _itemInfoUI.OpenInfo(item, isStoreMode);
    }

    public void RefreshUI()
    {
        // StoreManager.PopulateAllCategories()에서 카테고리별 슬롯을 갱신합니다.
        StoreManager.Instance.PopulateAllCategories();
    }

    /// <summary>
    /// 카테고리 인덱스에 해당하는 Content Transform을 반환합니다.
    /// 0: 시장, 1: 장비, 2: 레시피
    /// </summary>
    public Transform GetContentParent(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= _contentParents.Length) return null;
        return _contentParents[categoryIndex];
    }

    /// <summary>
    /// 지정된 부모 Transform 하위의 모든 슬롯 오브젝트를 제거합니다.
    /// </summary>
    public void ClearSlots(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
