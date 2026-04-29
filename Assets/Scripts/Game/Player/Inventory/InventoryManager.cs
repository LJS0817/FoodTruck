using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 💡 Key: 재료의 고유 ID, Value: 현재 보유 수량
    // Dictionary를 사용하여 O(1)의 속도로 즉시 재고를 파악하고 가비지(GC)를 방지합니다.
    private Dictionary<int, int> ingredientStock = new Dictionary<int, int>(32);

    // 재고가 변동될 때 UI에 알림을 보내는 이벤트 (인수: 재료ID, 남은수량)
    public event Action<int, int> OnStockChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 시장에서 재료를 사 오거나 보상을 얻었을 때 호출
    public void AddIngredient(int ingredientID, int amount)
    {
        if (ingredientStock.ContainsKey(ingredientID))
        {
            ingredientStock[ingredientID] += amount;
        }
        else
        {
            ingredientStock.Add(ingredientID, amount);
        }

        OnStockChanged?.Invoke(ingredientID, ingredientStock[ingredientID]);
        Debug.Log($"[인벤토리] 재료 ID {ingredientID} 추가됨. 현재 수량: {ingredientStock[ingredientID]}");
    }

    // 요리를 위해 재료통에서 재료를 꺼낼 때 호출
    public bool UseIngredient(int ingredientID)
    {
        if (ingredientStock.TryGetValue(ingredientID, out int currentAmount) && currentAmount > 0)
        {
            ingredientStock[ingredientID]--;
            OnStockChanged?.Invoke(ingredientID, ingredientStock[ingredientID]);
            return true; // 재고가 있어서 사용 성공
        }

        Debug.LogWarning($"[인벤토리] 재료 ID {ingredientID}의 재고가 부족합니다!");
        return false; // 재고 부족
    }

    // 💡 (선택) 특정 재료의 현재 수량을 반환하는 함수 (LINQ 방어용)
    public int GetStock(int ingredientID)
    {
        return ingredientStock.TryGetValue(ingredientID, out int amount) ? amount : 0;
    }
}