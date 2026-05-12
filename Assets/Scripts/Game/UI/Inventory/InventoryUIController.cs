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
    [SerializeField] CanvasGroup _inventoryUI;
    [SerializeField] InventoryUISlot slotPrefab;
    [SerializeField] Transform slotContainer;
    [SerializeField] GameObject _applyBtn;
    
    SortBy _currentSortBy;
    OrderBy _currentOrderBy;

    private List<InventoryUISlot> spawnedSlots = new List<InventoryUISlot>();
    private List<InventoryItem> currentItems = new List<InventoryItem>();

    void Start()
    {
        CloseInventory();
    }

    public void OpenInventory(bool trig, IngredientData targetData = null)
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.alpha = 1f;
            _inventoryUI.interactable = true;
            _inventoryUI.blocksRaycasts = true;
        }
        if(!trig)
        {
            if(_applyBtn.activeSelf) _applyBtn.SetActive(false);
        } else
        {
             if(!_applyBtn.activeSelf) _applyBtn.SetActive(true);
        }

        // 💡 인벤토리 오픈 시 게임 시간 정지
        Time.timeScale = 0f;

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

        // 💡 인벤토리를 닫을 때 선택 상태 초기화
        if (_selectedSlot != null)
        {
            _selectedSlot.SetFocus(false);
            _selectedSlot = null;
        }

        // 💡 인벤토리 종료 시 게임 시간 재개
        Time.timeScale = 1f;
    }

    public void ChangeSortBy(int idx) { _currentSortBy = (SortBy)idx; UpdateUI(currentItems); }
    public void ChangeOrderBy(int idx) { _currentOrderBy = (OrderBy)idx; UpdateUI(currentItems); }

    private InventoryUISlot _selectedSlot;

    public void OnSlotClicked(InventoryUISlot slot)
    {
        SetSelectedSlot(slot);
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
                SetSelectedSlot(spawnedSlots[i]);
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

        // 💡 Apply 버튼 활성화/비활성화
        if (_applyBtn != null)
        {
            UnityEngine.UI.Button btn = _applyBtn.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.interactable = (_selectedSlot != null);
            }
        }
    }

    public void OnClickApply()
    {
        if (_selectedSlot != null)
        {
            // 선택된 아이템의 데이터를 가지고 IngredientManager에 상자 세팅 요청
            IngredientManager.Instance.SetupBox(_selectedSlot.Item.data);
            // 세팅 완료 후 인벤토리 닫기
            InventoryManager.Instance.CloseUI();
        }
        else
        {
            Debug.LogWarning("[InventoryUIController] 선택된 아이템이 없습니다.");
        }
    }

    // 💡 폐기 버튼: 선택된 아이템을 재화 반환 없이 영구 삭제
    public void OnClickDiscard()
    {
        if (_selectedSlot != null)
        {
            InventoryManager.Instance.DiscardItem(_selectedSlot.Item);
            SetSelectedSlot(null); // 삭제 후 선택 해제 및 버튼 비활성화
        }
        else
        {
            Debug.LogWarning("[InventoryUIController] 폐기할 아이템이 선택되지 않았습니다.");
        }
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