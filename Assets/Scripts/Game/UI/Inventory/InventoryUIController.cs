using System;
using System.Collections.Generic;
using UnityEngine;

public enum SortBy
{
    Name,
    Expiration
}

public enum OrderBy
{
    Ascending,
    Descending
}

public class InventoryUIController : MonoBehaviour
{
    [Header("Popup & Info UI Settings")]
    [SerializeField] private RectTransform _inventoryPanel;
    CanvasGroup _inventoryPanelGroup;
    Canvas _panelCanvas;
    [Tooltip("팝업 모드일 때 인벤토리의 렌더링 순서 (다른 UI보다 뒤에 그리려면 낮게 설정)")]
    [SerializeField] private int _popupSortingOrder = 1;
    [SerializeField] private RectTransform _popupTransform;
    [SerializeField] CanvasGroup _popupCanvasGroup;
    [SerializeField] private ItemSimpleInfoUI _itemSimpleInfoUI;
    [SerializeField] private ItemInfoUI _itemInfoUI;
    
    private Vector2 _originalSize;
    private Vector2 _originalAnchoredPos;
    private Vector2 _originalAnchorMin;
    private Vector2 _originalAnchorMax;
    private Vector2 _originalPivot;
    private bool _originalSaved = false;
    private bool _isPopupMode = false;

    [SerializeField] CanvasGroup _inventoryUI;
    [SerializeField] InventoryUISlot slotPrefab;
    [SerializeField] Transform slotContainer;
    
    SortBy _currentSortBy;
    OrderBy _currentOrderBy;

    private List<InventoryUISlot> spawnedSlots = new List<InventoryUISlot>();
    private List<InventoryItem> currentItems = new List<InventoryItem>();

    void Start()
    {
        CloseInventory();
        _inventoryPanelGroup = _inventoryPanel.GetComponent<CanvasGroup>();
        _panelCanvas = _inventoryPanel.GetComponent<Canvas>();
        
        // 최적화를 위한 Sub-Canvas 자동 생성
        if (_panelCanvas == null)
        {
            _panelCanvas = _inventoryPanel.gameObject.AddComponent<Canvas>();
            _inventoryPanel.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    public void OpenInventory(IngredientData targetData = null)
    {
        OpenInventory(false, targetData);
    }

    public void OpenInventory(bool isPopup, IngredientData targetData = null)
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.alpha = 1f;
            _inventoryUI.interactable = true;
            _inventoryUI.blocksRaycasts = true;
        }

        _isPopupMode = isPopup;

        if (_inventoryPanel != null && _popupTransform != null)
        {
            if (!_originalSaved)
            {
                _originalSize = _inventoryPanel.sizeDelta;
                _originalAnchoredPos = _inventoryPanel.anchoredPosition;
                _originalAnchorMin = _inventoryPanel.anchorMin;
                _originalAnchorMax = _inventoryPanel.anchorMax;
                _originalPivot = _inventoryPanel.pivot;
                _originalSaved = true;
            }

            if (isPopup)
            {
                // 부모 계층이 달라도 위치와 크기가 정확히 일치하도록 월드 좌표와 Rect 크기를 사용합니다.
                _inventoryPanel.pivot = _popupTransform.pivot;
                _inventoryPanel.anchorMin = new Vector2(0.5f, 0.5f);
                _inventoryPanel.anchorMax = new Vector2(0.5f, 0.5f);
                _inventoryPanel.sizeDelta = new Vector2(_popupTransform.rect.width, _popupTransform.rect.height);
                _inventoryPanel.position = _popupTransform.position;
                _inventoryPanel.rotation = _popupTransform.rotation;
                
                _inventoryPanelGroup.ignoreParentGroups = true; // 팝업 모드에서는 부모 그룹의 영향을 받지 않도록 설정
                
                if (_panelCanvas != null)
                {
                    _panelCanvas.overrideSorting = true;
                    _panelCanvas.sortingOrder = _popupSortingOrder;
                }

                _popupCanvasGroup.alpha = 1f;
                _popupCanvasGroup.interactable = true;
                _popupCanvasGroup.blocksRaycasts = true;

            }
            else
            {
                _inventoryPanel.anchorMin = _originalAnchorMin;
                _inventoryPanel.anchorMax = _originalAnchorMax;
                _inventoryPanel.pivot = _originalPivot;
                _inventoryPanel.sizeDelta = _originalSize;
                _inventoryPanel.anchoredPosition = _originalAnchoredPos;
                _inventoryPanelGroup.ignoreParentGroups = false;
                
                if (_panelCanvas != null)
                {
                    _panelCanvas.overrideSorting = false; // 일반 모드에서는 Hierarchy 순서를 따름
                }

                _popupCanvasGroup.alpha = 0f;
                _popupCanvasGroup.interactable = false;
                _popupCanvasGroup.blocksRaycasts = false;
            }
        }

        // Time.timeScale 제어는 MainUINavigationController에서 일괄 수행합니다.

        // 💡 타겟 데이터에 해당하는 슬롯 포커스 및 Apply 버튼 상태 초기화
        FocusOnItem(targetData);
    }

    public void CloseInventory()
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.alpha = 0f;
            _inventoryUI.interactable = false;
            _inventoryUI.blocksRaycasts = false;
        }

        if (_popupCanvasGroup != null)
        {
            _popupCanvasGroup.alpha = 0f;
            _popupCanvasGroup.interactable = false;
            _popupCanvasGroup.blocksRaycasts = false;
        }

        // 💡 인벤토리를 닫을 때 선택 상태 초기화
        if (_selectedSlot != null)
        {
            _selectedSlot.SetFocus(false);
            _selectedSlot = null;
        }

        if (_itemSimpleInfoUI != null) _itemSimpleInfoUI.CloseUI();
        if (_itemInfoUI != null) _itemInfoUI.CloseUI();

        // Time.timeScale 제어는 MainUINavigationController에서 일괄 수행합니다.
    }

    public void ChangeSortBy(int idx) { _currentSortBy = (SortBy)idx; UpdateUI(currentItems); }
    public void ChangeOrderBy(int idx) { _currentOrderBy = (OrderBy)idx; UpdateUI(currentItems); }

    private InventoryUISlot _selectedSlot;

    public void OnSlotClicked(InventoryUISlot slot)
    {
        SetSelectedSlot(slot);
        
        if (_isPopupMode)
        {
            if (_itemSimpleInfoUI != null)
            {
                _itemSimpleInfoUI.OpenInfo(slot.Item);
            }
        }
        else
        {
            if (_itemInfoUI != null)
            {
                // 인벤토리 모드로 띄우기 (isStoreMode = false)
                StoreItem dummyStoreItem = StoreItem.FromIngredient(slot.Item.data, slot.Item.data.basePrice, slot.Item.amount);
                _itemInfoUI.OpenInfo(dummyStoreItem, false);
            }
        }
    }

    private void FocusOnItem(IngredientData targetData)
    {
        if (targetData == null) 
        {
            SetSelectedSlot(null);
            return;
        }

        // targetData와 일치하는 슬롯 찾기
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i].gameObject.activeSelf && spawnedSlots[i].Item.data.ingredientID == targetData.ingredientID)
            {
                OnSlotClicked(spawnedSlots[i]); // 선택 및 UI 오픈 동시 처리
                return; // 찾았으면 리턴
            }
        }

        // 찾지 못했다면 포커스 없음
        SetSelectedSlot(null);
    }

    private void SetSelectedSlot(InventoryUISlot slot)
    {
        if (_selectedSlot != null) _selectedSlot.SetFocus(false);
        _selectedSlot = slot;
        if (_selectedSlot != null) _selectedSlot.SetFocus(true);
    }

    public void OnClickApply(int amount)
    {
        if (_selectedSlot != null)
        {
            // 선택된 아이템의 데이터를 가지고 IngredientManager에 상자 세팅 요청
            IngredientManager.Instance.SetupBox(_selectedSlot.Item.data, amount);
            // 세팅 완료 후 인벤토리 닫기
            InventoryManager.Instance.CloseUI();
        }
        else
        {
            Debug.LogWarning("[InventoryUIController] 선택된 아이템이 없습니다.");
        }
    }

    public void OnClickEmptyBox()
    {
        // 💡 선택된 상자를 비우고 인벤토리 닫기
        if (IngredientManager.Instance != null)
        {
            IngredientManager.Instance.EmptyCurrentBox();
            InventoryManager.Instance.CloseUI();
        }
    }

    // 💡 폐기 버튼: 선택된 아이템을 재화 반환 없이 영구 삭제
    // public void OnClickDiscard()
    // {
    //     if (_selectedSlot != null)
    //     {
    //         if (_amountSetter != null)
    //         {
    //             int maxAvailable = _selectedSlot.Item.amount;
    //             // _amountSetter.Open(maxAvailable, 0, (amount) => {
    //             //     InventoryManager.Instance.DiscardItem(_selectedSlot.Item, amount);
    //             //     SetSelectedSlot(null); // 삭제 후 선택 해제 및 버튼 비활성화
    //             // });
    //         }
    //         else
    //         {
    //             InventoryManager.Instance.DiscardItem(_selectedSlot.Item);
    //             SetSelectedSlot(null); // 삭제 후 선택 해제 및 버튼 비활성화
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("[InventoryUIController] 폐기할 아이템이 선택되지 않았습니다.");
    //     }
    // }

    // 💡 가공 버튼 관련 액션
    public void OnClickProcessBake() { TryProcess(ProcessType.Bake); }
    public void OnClickProcessFry() { TryProcess(ProcessType.Fry); }
    public void OnClickProcessBlend() { TryProcess(ProcessType.Blend); }
    public void OnClickProcessCut() { TryProcess(ProcessType.Cut); }

    private void TryProcess(ProcessType type)
    {
        if (_selectedSlot == null)
        {
            Debug.LogWarning("[InventoryUIController] 가공할 아이템이 선택되지 않았습니다.");
            return;
        }

        if (ProcessManager.Instance == null)
        {
            Debug.LogWarning("[InventoryUIController] ProcessManager가 존재하지 않습니다.");
            return;
        }

        ProcessManager.Instance.ExecuteProcess(_selectedSlot.Item.data, type, (success, resultData) => {
            if (success)
            {
                // 가공 성공 시, UI 새로고침 또는 이펙트 처리
                // ExecuteProcess 내부에서 InventoryManager를 통해 결과물이 추가되고,
                // 차감도 이루어지며 InventoryManager.UpdateUI가 호출됨
                SetSelectedSlot(null);
            }
        });
    }

    public void UpdateUI(List<InventoryItem> items)
    {
        currentItems = items;
        _selectedSlot = null; // UI 갱신 시 선택 해제
        
        // 정렬 수행 (람다식 대신 전용 비교 메서드 사용하여 가비지 할당 방지)
        currentItems.Sort(CompareItems);

        // 💡 UI 오브젝트 풀링: 기존 슬롯을 Destroy하지 않고 재사용합니다.
        int requiredCount = currentItems.Count;
        
        // 1. 필요한 슬롯 수보다 부족하면 Instantiate로 추가
        while (spawnedSlots.Count < requiredCount)
        {
            if (slotPrefab != null && slotContainer != null)
            {
                InventoryUISlot slot = Instantiate(slotPrefab, slotContainer);
                spawnedSlots.Add(slot);
            }
            else
            {
                break; // 오류 방지
            }
        }

        // 2. 존재하는 슬롯에 데이터를 덮어씌우고 활성화 및 정렬 순서 적용
        for (int i = 0; i < requiredCount; i++)
        {
            spawnedSlots[i].gameObject.SetActive(true);
            spawnedSlots[i].SetInfo(currentItems[i], OnSlotClicked);
            spawnedSlots[i].transform.SetAsLastSibling(); // LayoutGroup 정렬 동기화
        }

        // 3. 사용하지 않는 잉여 슬롯들은 비활성화
        for (int i = requiredCount; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(false);
        }
    }

    // 💡 가비지 컬렉션(GC) 방지를 위한 전용 비교기
    private int CompareItems(InventoryItem a, InventoryItem b)
    {
        int result = 0;
        if (_currentSortBy == SortBy.Name)
        {
            result = a.data.ingredientName.CompareTo(b.data.ingredientName);
        }
        else if (_currentSortBy == SortBy.Expiration)
        {
            result = a.remainingDays.CompareTo(b.remainingDays);
        }

        if (_currentOrderBy == OrderBy.Descending)
        {
            result *= -1;
        }
        return result;
    }
}