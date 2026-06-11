using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 상점, 보유 관리, 장착 스왑, 보상 판매 시스템을 통합 관리합니다.
/// </summary>
public class EquipmentStoreManager : MonoBehaviour
{
    public static EquipmentStoreManager Instance { get; private set; }

    [Header("Equipment Catalog")]
    [SerializeField] public List<EquipmentData> allEquipments;

    // 💡 모든 보유 장비 목록 (동일 카테고리 여러 개 보관 가능)
    private List<EquipmentData> ownedEquipmentList = new List<EquipmentData>(16);

    // 💡 현재 트럭에 장착된 장비 (타입당 1개만 장착 가능)
    private Dictionary<EquipmentType, EquipmentData> equippedEquipments = new Dictionary<EquipmentType, EquipmentData>(8);
    
    // 💡 개별 장비(EquipmentData)별 레벨 추적
    private Dictionary<EquipmentData, int> equipmentLevels = new Dictionary<EquipmentData, int>(16);

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ===== 보유 및 장착 상태 확인 =====

    public bool HasEquipment(EquipmentData equipment)
    {
        return ownedEquipmentList.Contains(equipment);
    }

    public EquipmentData GetEquippedEquipment(EquipmentType type)
    {
        equippedEquipments.TryGetValue(type, out EquipmentData data);
        return data;
    }

    public List<EquipmentData> GetOwnedEquipmentsList()
    {
        return new List<EquipmentData>(ownedEquipmentList);
    }

    public int GetEquipmentLevel(EquipmentData equipment)
    {
        if (equipmentLevels.TryGetValue(equipment, out int level)) return level;
        return 0;
    }

    public bool IsEquipped(EquipmentData equipment)
    {
        if (equipment == null) return false;
        return GetEquippedEquipment(equipment.type) == equipment;
    }

    // ===== 장착(Equip) 스왑 =====

    /// <summary>
    /// 보유 중인 장비를 해당 카테고리 슬롯에 장착합니다.
    /// </summary>
    public bool EquipEquipment(EquipmentData equipment)
    {
        if (!HasEquipment(equipment)) return false;
        
        equippedEquipments[equipment.type] = equipment;
        Debug.Log($"<color=yellow>[EquipmentStoreManager] {equipment.equipmentName} 장착 완료!</color>");
        
        OnEquipmentChanged?.Invoke();
        return true;
    }

    // ===== 장비 구매/보상판매 =====

    /// <summary>
    /// 장비를 구매합니다. (보상 판매 여부 선택 가능)
    /// </summary>
    public bool BuyEquipment(EquipmentData newEquipment, bool isTradeIn)
    {
        if (HasEquipment(newEquipment))
        {
            Debug.LogWarning($"[EquipmentStoreManager] 이미 {newEquipment.equipmentName}을(를) 보유 중입니다.");
            return false;
        }

        int finalCost = newEquipment.price;
        EquipmentData currentEquipped = GetEquippedEquipment(newEquipment.type);

        if (isTradeIn && currentEquipped != null)
        {
            finalCost -= currentEquipped.tradeInValue;
            if (finalCost < 0) finalCost = 0;
        }

        if (PlayerManager.Instance.SpendMoney(finalCost))
        {
            // 보상 판매 시 기존 장착 장비 소멸
            if (isTradeIn && currentEquipped != null)
            {
                ownedEquipmentList.Remove(currentEquipped);
                equipmentLevels.Remove(currentEquipped);
                Debug.Log($"<color=orange>[장비 교체] {currentEquipped.equipmentName} 보상 판매 (가치: {currentEquipped.tradeInValue})</color>");
            }

            ownedEquipmentList.Add(newEquipment);
            equipmentLevels[newEquipment] = 1;

            // 보상 판매이거나, 빈 슬롯이거나 상관없이 우선 장착 시도 (새로 샀으니 바로 장착해주는 것이 일반적)
            EquipEquipment(newEquipment); 

            Debug.Log($"<color=cyan>[장비 구매] {newEquipment.equipmentName} 획득! ({finalCost}원 지불)</color>");
            return true; // EquipEquipment 내부에서 OnEquipmentChanged 호출됨
        }
        else
        {
            Debug.LogWarning($"<color=red>[장비 구매 실패] 잔액 부족! (필요: {finalCost})</color>");
            return false;
        }
    }

    /// <summary>
    /// UI 표시용: 보상 판매 적용 시 예상 비용 반환
    /// </summary>
    public int CalculateTradeInCost(EquipmentData newEquipment)
    {
        int finalCost = newEquipment.price;
        EquipmentData current = GetEquippedEquipment(newEquipment.type);
        if (current != null)
        {
            finalCost -= current.tradeInValue;
        }
        return Mathf.Max(0, finalCost);
    }

    // ===== 레벨 업그레이드 =====

    public int GetUpgradeCost(EquipmentData equipment)
    {
        int currentLevel = GetEquipmentLevel(equipment);
        if (currentLevel == 0) return 0;
        return equipment.price * currentLevel;
    }

    public bool LevelUpEquipment(EquipmentData equipment)
    {
        int currentLevel = GetEquipmentLevel(equipment);
        if (currentLevel == 0) return false;

        int cost = GetUpgradeCost(equipment);
        if (PlayerManager.Instance.SpendMoney(cost))
        {
            equipmentLevels[equipment] = currentLevel + 1;
            Debug.Log($"<color=green>[장비 레벨업] {equipment.equipmentName} Lv.{currentLevel} -> Lv.{currentLevel + 1}</color>");
            OnEquipmentChanged?.Invoke();
            return true;
        }
        return false;
    }

    public List<EquipmentData> GetAllEquipments() { return allEquipments; }
}
