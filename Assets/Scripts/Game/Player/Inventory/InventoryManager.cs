using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public IngredientData data;
    public int amount;
    public DateTime expirationDate;
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
    public void AddIngredient(IngredientData data, int amount, DateTime expiration)
    {
        bool found = false;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            // ID와 유통기한이 모두 같은 슬롯이 있으면 개수만 합칩니다.
            if (inventoryItems[i].data.ingredientID == data.ingredientID && 
                inventoryItems[i].expirationDate.Date == expiration.Date)
            {
                inventoryItems[i].amount += amount;
                found = true;
                break;
            }
        }

        if (!found)
        {
            inventoryItems.Add(new InventoryItem { data = data, amount = amount, expirationDate = expiration });
        }

        Debug.Log($"[인벤토리] {data.ingredientName} {amount}개 추가됨. (유통기한: {expiration:yyyy-MM-dd})");
        UpdateUI();
    }

    // 요리를 위해 재료통에서 재료를 하나 꺼낼 때 호출 (유통기한 임박한 것부터 차감)
    public bool UseIngredient(int ingredientID)
    {
        int targetIndex = -1;
        DateTime closestExpiration = DateTime.MaxValue;

        // 가장 유통기한이 임박한(가장 과거인) 재고 탐색
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID && inventoryItems[i].amount > 0)
            {
                if (inventoryItems[i].expirationDate < closestExpiration)
                {
                    closestExpiration = inventoryItems[i].expirationDate;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex != -1)
        {
            inventoryItems[targetIndex].amount--;
            if (inventoryItems[targetIndex].amount <= 0)
            {
                inventoryItems.RemoveAt(targetIndex); // 개수가 0이면 슬롯 삭제
            }
            UpdateUI();
            return true;
        }

        Debug.LogWarning($"[인벤토리] 재료 ID {ingredientID}의 재고가 부족합니다!");
        return false;
    }

    // 요리를 위해 재료통에 재료를 채울 때 호출 (최대 보유량 확인)
    public int FillIngredient(int ingredientID, int amount)
    {
        int totalAvailable = 0;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID)
            {
                totalAvailable += inventoryItems[i].amount;
            }
        }

        if (totalAvailable > 0)
        {
            if(amount < totalAvailable) totalAvailable = amount;
            return totalAvailable; 
		}

        Debug.LogWarning($"[인벤토리] 재료 ID {ingredientID}의 재고가 부족하여 채울 수 없습니다!");
        return 0;
    }

    // 💡 특정 레시피에 필요한 재료들을 모두 보유하고 있는지 확인
    public bool HasIngredients(IngredientData[] requiredIngredients)
    {
        Dictionary<int, int> requiredCounts = new Dictionary<int, int>();
        for (int i = 0; i < requiredIngredients.Length; i++)
        {
            int id = requiredIngredients[i].ingredientID;
            if (requiredCounts.ContainsKey(id)) requiredCounts[id]++;
            else requiredCounts[id] = 1;
        }

        foreach (var kvp in requiredCounts)
        {
            int requiredAmount = kvp.Value;
            int currentAmount = 0;
            
            for (int i = 0; i < inventoryItems.Count; i++)
            {
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

    // 💡 특정 레시피에 필요한 재료들을 한 번에 소비
    public void ConsumeIngredients(IngredientData[] requiredIngredients)
    {
        for (int i = 0; i < requiredIngredients.Length; i++)
        {
            UseIngredient(requiredIngredients[i].ingredientID);
        }
    }

    // 💡 재료 폐기: 재화 반환 없이 인벤토리에서 영구 삭제
    public void DiscardItem(InventoryItem item)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i] == item)
            {
                Debug.Log($"<color=orange>[폐기] {item.data.ingredientName} {item.amount}개 폐기됨. (유통기한: {item.expirationDate:yyyy-MM-dd})</color>");
                inventoryItems.RemoveAt(i);
                UpdateUI();
                return;
            }
        }
    }

    // 💡 유통기한 만료된 재료 자동 폐기 (하루가 바뀔 때 호출)
    public void DiscardExpiredItems()
    {
        DateTime now = DateTime.Now;
        for (int i = inventoryItems.Count - 1; i >= 0; i--)
        {
            if (inventoryItems[i].expirationDate < now)
            {
                Debug.Log($"<color=red>[유통기한 만료] {inventoryItems[i].data.ingredientName} {inventoryItems[i].amount}개 자동 폐기!</color>");
                inventoryItems.RemoveAt(i);
            }
        }
        UpdateUI();
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
    public int GetTotalAmount(int ingredientID)
    {
        int total = 0;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data.ingredientID == ingredientID)
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
            if (!UseIngredient(ingredientID)) break;
        }
    }
}