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
    [SerializeField] AmountSetter _amountSetter; // 💡 수량 조절기
    
    SortBy _currentSortBy;
    OrderBy _currentOrderBy;

    private List<InventoryUISlot> spawnedSlots = new List<InventoryUISlot>();
    private List<InventoryItem> currentItems = new List<InventoryItem>();

    void Start()
    {
        CloseInventory();
    }

    public void OpenInventory(IngredientData targetData = null)
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.alpha = 1f;
            _inventoryUI.interactable = true;
            _inventoryUI.blocksRaycasts = true;
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
    public void OnClickDiscard()
    {
        if (_selectedSlot != null)
        {
            if (_amountSetter != null)
            {
                int maxAvailable = _selectedSlot.Item.amount;
                _amountSetter.Open(maxAvailable, 0, (amount) => {
                    InventoryManager.Instance.DiscardItem(_selectedSlot.Item, amount);
                    SetSelectedSlot(null); // 삭제 후 선택 해제 및 버튼 비활성화
                });
            }
            else
            {
                InventoryManager.Instance.DiscardItem(_selectedSlot.Item);
                SetSelectedSlot(null); // 삭제 후 선택 해제 및 버튼 비활성화
            }
        }
        else
        {
            Debug.LogWarning("[InventoryUIController] 폐기할 아이템이 선택되지 않았습니다.");
        }
    }

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