using UnityEngine;

public class StoreUIController : MonoBehaviour
{
    [SerializeField] CanvasGroup storeUIPanel;
    [SerializeField] CanvasGroup _equipmentGroup;
    [SerializeField] CanvasGroup _marketGroup;
    [SerializeField] CanvasGroup _recipeGroup;

    [Header("Info Panel")]
    [SerializeField] private StoreItemInfoUI _storeItemInfoUI;

    // 💡 GC 방지: 배열로 캐싱하여 ChangeCategory에서 반복문 사용
    private CanvasGroup[] _categoryGroups;

    private int _currentCategoryIndex = -1;

    private void Awake()
    {
        _categoryGroups = new CanvasGroup[] { _marketGroup, _equipmentGroup, _recipeGroup };
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
        if (_storeItemInfoUI != null)
        {
            _storeItemInfoUI.CloseUI();
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
        if (_storeItemInfoUI != null)
        {
            _storeItemInfoUI.CloseUI();
        }
    }

    /// <summary>
    /// ShopItemSlotUI에서 슬롯 클릭 시 호출됩니다.
    /// 아이템 정보를 StoreItemInfoUI에 전달하여 표시합니다.
    /// </summary>
    public void ShowItemInfo(StoreItem item)
    {
        if (_storeItemInfoUI == null || item == null) return;
        _storeItemInfoUI.OpenInfo(item);
    }

    public void RefreshUI()
    {
        // TODO: 현재 카테고리의 슬롯 목록을 다시 그리는 로직
    }
}
