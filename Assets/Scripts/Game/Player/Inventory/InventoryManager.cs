using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public IngredientData data;
    public int amount;
    public int remainingDays;
    
    // 새롭게 추가되는 메타데이터
    public IngredientState state = IngredientState.Raw;
    public ProcessType processType = ProcessType.None;
    public ItemGrade grade = ItemGrade.Normal;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 💡 유통기한별 분리 보관을 위해 Dictionary 대신 List를 사용합니다.
    public List<InventoryItem> inventoryItems = new List<InventoryItem>(32);

    public event Action OnInventoryUpdated;

    [SerializeField] InventoryUIController _controller;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 시장에서 재료를 사 오거나 보상을 얻었을 때 호출
    public void AddIngredient(IngredientData data, int amount, int remainingDays, IngredientState state = IngredientState.Raw, ProcessType processType = ProcessType.None, ItemGrade grade = ItemGrade.Normal)
    {
        bool found = false;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            // ID와 남은 유통기한, 상태, 가공방식, 품질등급이 모두 같은 슬롯이 있으면 개수만 합칩니다.
            if (inventoryItems[i].data.ingredientID == data.ingredientID && 
                inventoryItems[i].remainingDays == remainingDays &&
                inventoryItems[i].state == state &&
                inventoryItems[i].processType == processType &&
                inventoryItems[i].grade == grade)
            {
                inventoryItems[i].amount += amount;
                found = true;
                break;
            }
        }

        if (!found)
        {
            inventoryItems.Add(new InventoryItem { 
                data = data, 
                amount = amount, 
                remainingDays = remainingDays,
                state = state,
                processType = processType,
                grade = grade
            });
        }

        Debug.Log($"[인벤토리] {data.ingredientName} {amount}개 추가됨. (남은 유통기한: {remainingDays}일)");
        UpdateUI();
    }

    // 요리를 위해 재료통에서 재료를 하나 꺼낼 때 호출 (유통기한 임박한 것부터 차감)
    public int UseIngredient(int ingredientID)
    {
        int targetIndex = -1;
        int closestExpiration = int.MaxValue;

        // 가장 유통기한이 임박한(남은 일수가 적은) 재고 탐색
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID && inventoryItems[i].amount > 0)
            {
                if (inventoryItems[i].remainingDays < closestExpiration)
                {
                    closestExpiration = inventoryItems[i].remainingDays;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex != -1)
        {
            int days = inventoryItems[targetIndex].remainingDays;
            inventoryItems[targetIndex].amount--;
            if (inventoryItems[targetIndex].amount <= 0)
            {
                inventoryItems.RemoveAt(targetIndex); // 개수가 0이면 슬롯 삭제
            }
            UpdateUI();
            
            if (IngredientManager.Instance != null)
                IngredientManager.Instance.CheckAndEmptyBoxesWithoutStock();
                
            return days;
        }

        Debug.LogWarning($"[인벤토리] 재료 ID {ingredientID}의 재고가 부족합니다!");
        return -1;
    }

    // 💡 특정 레시피나 가공 조건을 모두 만족하는 재료 중 가장 유통기한 임박한 것을 하나 소비합니다.
    public int UseSpecificIngredient(int ingredientID, IngredientState state, ProcessType processType, ItemGrade grade)
    {
        int targetIndex = -1;
        int closestExpiration = int.MaxValue;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID && 
                inventoryItems[i].amount > 0 &&
                inventoryItems[i].state == state &&
                inventoryItems[i].processType == processType &&
                inventoryItems[i].grade == grade)
            {
                if (inventoryItems[i].remainingDays < closestExpiration)
                {
                    closestExpiration = inventoryItems[i].remainingDays;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex != -1)
        {
            int days = inventoryItems[targetIndex].remainingDays;
            inventoryItems[targetIndex].amount--;
            if (inventoryItems[targetIndex].amount <= 0)
            {
                inventoryItems.RemoveAt(targetIndex);
            }
            UpdateUI();
            
            if (IngredientManager.Instance != null)
                IngredientManager.Instance.CheckAndEmptyBoxesWithoutStock();
                
            return days;
        }
        return -1;
    }

    // 요리를 위해 재료통에 재료를 채울 때 호출 (최대 보유량 확인)
    public List<int> FillIngredient(int ingredientID, int amount)
    {
        List<int> filledDays = new List<int>();
        
        // 💡 UseIngredient를 호출하여 실제로 인벤토리에서 차감 (유통기한 임박한 것부터 자동 차감됨)
        for (int i = 0; i < amount; i++)
        {
            int days = UseIngredient(ingredientID);
            if (days != -1)
            {
                filledDays.Add(days);
            }
            else
            {
                break;
            }
        }

        if (filledDays.Count == 0)
        {
            Debug.LogWarning($"[인벤토리] 재료 ID {ingredientID}의 재고가 부족하여 채울 수 없습니다!");
        }
        else
        {
            Debug.Log($"[인벤토리] 재료 ID {ingredientID}를 {filledDays.Count}개 상자에 채웠습니다.");
        }
        
        return filledDays;
    }

    // 💡 특정 레시피에 필요한 원재료들을 모두 보유하고 있는지 확인 (상태 무관)
    public bool HasIngredients(FoodIngredientConfig[] configs)
    {
        Dictionary<int, int> requiredCounts = new Dictionary<int, int>();
        for (int i = 0; i < configs.Length; i++)
        {
            if (configs[i].rawIngredient == null) continue;
            int id = configs[i].rawIngredient.ingredientID;
            if (requiredCounts.ContainsKey(id)) requiredCounts[id]++;
            else requiredCounts[id] = 1;
        }

        foreach (var kvp in requiredCounts)
        {
            int requiredAmount = kvp.Value;
            int currentAmount = 0;
            
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                // 유저의 요청: State와 무관하게 Base Ingredient ID만 같으면 사용할 수 있도록 허용
                if (inventoryItems[i].data.ingredientID == kvp.Key)
                {
                    currentAmount += inventoryItems[i].amount;
                }
            }

            if (currentAmount < requiredAmount)
                return false;
        }

        return true;
    }

    // 💡 특정 레시피에 필요한 재료들을 한 번에 소비 (유통기한 임박한 것부터 차감)
    public void ConsumeIngredients(FoodIngredientConfig[] configs)
    {
        for (int i = 0; i < configs.Length; i++)
        {
            if (configs[i].rawIngredient == null) continue;
            UseIngredient(configs[i].rawIngredient.ingredientID);
        }
    }

    // 💡 재료 폐기: 재화 반환 없이 인벤토리에서 영구 삭제
    public void DiscardItem(InventoryItem item, int discardAmount = -1)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i] == item)
            {
                int actualDiscard = (discardAmount == -1) ? item.amount : Mathf.Min(discardAmount, item.amount);
                
                inventoryItems[i].amount -= actualDiscard;
                Debug.Log($"<color=orange>[폐기] {item.data.ingredientName} {actualDiscard}개 폐기됨. (남은 유통기한: {item.remainingDays}일)</color>");

                if (inventoryItems[i].amount <= 0)
                {
                    inventoryItems.RemoveAt(i);
                }
                UpdateUI();
                
                if (IngredientManager.Instance != null)
                    IngredientManager.Instance.CheckAndEmptyBoxesWithoutStock();
                    
                return;
            }
        }
    }

    public void OnClickApply(int amount)
    {
        _controller.OnClickApply(amount);
    }

    // 💡 유통기한 차감 및 만료된 재료 자동 폐기 (하루가 바뀔 때 호출)
    public void ProcessDailyExpiry()
    {
        bool hasExpired = false;
        for (int i = inventoryItems.Count - 1; i >= 0; i--)
        {
            inventoryItems[i].remainingDays--;

            if (inventoryItems[i].remainingDays <= 0)
            {
                Debug.Log($"<color=red>[유통기한 만료] {inventoryItems[i].data.ingredientName} {inventoryItems[i].amount}개 자동 폐기!</color>");
                inventoryItems.RemoveAt(i);
                hasExpired = true;
            }
        }
        UpdateUI();
        
        if (hasExpired && IngredientManager.Instance != null)
        {
            IngredientManager.Instance.CheckAndEmptyBoxesWithoutStock();
        }
    }

    private void UpdateUI()
    {
        if (_controller != null)
        {
            _controller.UpdateUI(inventoryItems);
        }
        OnInventoryUpdated?.Invoke();
    }

    public void ChangeSortBy(int idx)
    {
        _controller.ChangeSortBy(idx);
    }

    public void ChangeOrderBy(int idx)
    {
        _controller.ChangeOrderBy(idx);
    }

    public void OpenUI()
    {
        _controller.OpenInventory(false);
    }
    public void OpenUIWithApplyBtn(IngredientData targetData = null)
    {
        _controller.OpenInventory(true, targetData);
    }

    public void CloseUI()
    {
        _controller.CloseInventory();
    }

    /// <summary>
    /// 특정 재료 ID의 총 보유 수량을 반환합니다.
    /// </summary>
    public int GetTotalAmount(int ingredientID, bool includeBoxItems = false)
    {
        int total = 0;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID)
            {
                total += inventoryItems[i].amount;
            }
        }
        
        if (includeBoxItems && IngredientManager.Instance != null)
        {
            // 이제 IngredientBox는 가상으로만 배치되고 인벤토리에 실물이 있으므로, 
            // includeBoxItems가 true여도 별도로 더할 필요가 없습니다. (전체 수량은 이미 inventoryItems에 포함됨)
        }
        
        return total;
    }

    /// <summary>
    /// 특정 상세 조건(상태, 가공, 등급)이 모두 일치하는 재료의 총 수량을 반환합니다.
    /// </summary>
    public int GetTotalSpecificAmount(int ingredientID, IngredientState state, ProcessType processType, ItemGrade grade)
    {
        int total = 0;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID &&
                inventoryItems[i].state == state &&
                inventoryItems[i].processType == processType &&
                inventoryItems[i].grade == grade)
            {
                total += inventoryItems[i].amount;
            }
        }
        return total;
    }

    /// <summary>
    /// 특정 재료 ID를 지정된 수량만큼 인벤토리에서 제거(폐기)합니다.
    /// </summary>
    public void DiscardIngredients(int ingredientID, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (UseIngredient(ingredientID) == -1) break;
        }
    }

    public void ClearAllIngredients()
    {
        inventoryItems.Clear();
        UpdateUI();
    }
}