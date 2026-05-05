using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 상점 및 교환 시스템을 관리합니다.
/// 장비 구매 시: (새 장비 가격 - 기존 장비 보상가) = 최종 결제 금액
/// </summary>
public class EquipmentStoreManager : MonoBehaviour
{
    [Header("Equipment Catalog")]
    [SerializeField] private List<EquipmentData> allEquipments; // 게임 내 모든 장비 목록

    // 💡 현재 트럭에 장착된 장비 (타입당 1개만 보유 가능)
    private Dictionary<EquipmentType, EquipmentData> ownedEquipments = new Dictionary<EquipmentType, EquipmentData>(8);

    public event Action OnEquipmentChanged;

    // ===== 장비 보유 확인 =====

    /// <summary>
    /// 특정 타입의 장비를 보유하고 있는지 확인합니다.
    /// </summary>
    public bool HasEquipment(EquipmentType type)
    {
        return ownedEquipments.ContainsKey(type);
    }

    /// <summary>
    /// 특정 타입의 현재 보유 장비 데이터를 반환합니다. 없으면 null.
    /// </summary>
    public EquipmentData GetOwnedEquipment(EquipmentType type)
    {
        ownedEquipments.TryGetValue(type, out EquipmentData data);
        return data;
    }

    // ===== 장비 구매/교환 =====

    /// <summary>
    /// 장비를 구매합니다. 기존에 같은 타입의 장비를 보유 중이면 보상 판매(교환)가 적용됩니다.
    /// </summary>
    public bool BuyEquipment(EquipmentData newEquipment)
    {
        int finalCost = newEquipment.price;

        // 기존 장비가 있으면 교환 가치 차감
        if (ownedEquipments.TryGetValue(newEquipment.type, out EquipmentData currentEquipment))
        {
            finalCost -= currentEquipment.tradeInValue;

            // 이미 같은 장비를 소유 중이면 구매 불가
            if (currentEquipment == newEquipment)
            {
                Debug.LogWarning($"[EquipmentStoreManager] 이미 {newEquipment.equipmentName}을(를) 보유 중입니다.");
                return false;
            }

            // 더 낮은 등급으로의 다운그레이드 방지 (선택적)
            if (newEquipment.tier <= currentEquipment.tier)
            {
                Debug.LogWarning($"[EquipmentStoreManager] {currentEquipment.equipmentName}보다 낮은 등급의 장비로는 교체할 수 없습니다.");
                return false;
            }
        }

        // 최종 비용이 음수가 되지 않도록 보정
        if (finalCost < 0) finalCost = 0;

        // 결제
        if (PlayerManager.Instance.SpendMoney(finalCost))
        {
            ownedEquipments[newEquipment.type] = newEquipment;

            if (currentEquipment != null)
            {
                Debug.Log($"<color=cyan>[장비 교환] {currentEquipment.equipmentName} → {newEquipment.equipmentName} (보상 판매: {currentEquipment.tradeInValue}원, 추가 결제: {finalCost}원)</color>");
            }
            else
            {
                Debug.Log($"<color=cyan>[장비 구매] {newEquipment.equipmentName} 구매 완료! ({finalCost}원)</color>");
            }

            OnEquipmentChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=red>[장비 구매 실패] 잔액이 부족합니다! (필요 금액: {finalCost}원)</color>");
            return false;
        }
    }

    /// <summary>
    /// 특정 장비 구매 시 필요한 최종 비용을 계산합니다 (UI 표시용).
    /// </summary>
    public int CalculateFinalCost(EquipmentData newEquipment)
    {
        int finalCost = newEquipment.price;

        if (ownedEquipments.TryGetValue(newEquipment.type, out EquipmentData current))
        {
            finalCost -= current.tradeInValue;
        }

        return Mathf.Max(0, finalCost);
    }

    /// <summary>
    /// 전체 장비 카탈로그를 반환합니다 (UI 표시용).
    /// </summary>
    public List<EquipmentData> GetAllEquipments() { return allEquipments; }
}
