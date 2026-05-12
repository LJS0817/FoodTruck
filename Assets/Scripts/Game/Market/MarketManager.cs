using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시장에 진열될 아이템 하나의 정보 (가격, 남은 유통기한, 할인율 포함)
/// </summary>
[Serializable]
public class MarketListing
{
    public IngredientData data;
    public int displayPrice;        // 최종 표시 가격
    public int remainingShelfDays;  // 남은 유통기한(일)
    public float discountRate;      // 할인율 (0.0 ~ 1.0)
}

public class MarketManager : MonoBehaviour
{

    [Header("Market Settings")]
    public List<IngredientData> allIngredients;     // 게임 내 모든 재료 목록
    public float dawnMarketDiscount = 0.3f;         // 새벽 시장 기본 할인율 (30%)
    public int dawnMarketItemCount = 8;             // 새벽 시장에 나오는 품목 수

    [Header("Time Condition")]
    public int dawnOpenHour = 6;    // 새벽 시장 오픈 시간
    public int dawnCloseHour = 9;   // 새벽 시장 마감 시간

    // 💡 GC 방지 및 O(1) 탐색을 위해 딕셔너리로 캐싱
    private Dictionary<int, IngredientData> catalogDict = new Dictionary<int, IngredientData>(32);

    // 일반 마켓 진열 리스트 (항시 이용 가능)
    private List<MarketListing> generalListings = new List<MarketListing>(32);

    // 새벽 시장 진열 리스트 (매일 갱신, 시간 제한)
    private List<MarketListing> dawnListings = new List<MarketListing>(16);

    // UI 갱신용 이벤트
    public event Action OnListingsUpdated;

    private void Awake()
    {
        // 카탈로그 딕셔너리 캐싱
        for (int i = 0; i < allIngredients.Count; i++)
        {
            catalogDict[allIngredients[i].ingredientID] = allIngredients[i];
        }
    }

    private void Start()
    {
        GenerateAllListings();
    }

    // ===== 매일 새벽(하루의 시작)에 호출되어 시장 진열을 갱신합니다 =====

    public void GenerateAllListings()
    {
        GenerateGeneralListings();
        GenerateDawnListings();
        OnListingsUpdated?.Invoke();
    }

    // --- 일반 마켓: 모든 재료를 상시 정가로 판매, 유통기한이 짧은 아이템은 할인 ---
    private void GenerateGeneralListings()
    {
        generalListings.Clear();

        // 💡 이벤트 물가 배율
        float eventMult = RandomEventManager.Instance != null ? RandomEventManager.Instance.GetMarketPriceMultiplier() : 1f;

        for (int i = 0; i < allIngredients.Count; i++)
        {
            IngredientData data = allIngredients[i];
            int basePrice = Mathf.Max(1, Mathf.RoundToInt(data.basePrice * eventMult));

            // 정상 가격 아이템 (유통기한 꽉 찬 상태)
            MarketListing normalListing = new MarketListing
            {
                data = data,
                displayPrice = basePrice,
                remainingShelfDays = data.maxShelfLifeDays,
                discountRate = 0f,
            };
            generalListings.Add(normalListing);

            // 💡 마감 할인 아이템: 50% 확률로 유통기한이 짧은 할인 상품을 추가 진열
            if (UnityEngine.Random.value > 0.5f && data.maxShelfLifeDays > 1)
            {
                int shortShelf = UnityEngine.Random.Range(1, Mathf.Max(2, data.maxShelfLifeDays / 2));
                float shelfRatio = (float)shortShelf / data.maxShelfLifeDays; // 0.0 ~ 0.5
                float discount = Mathf.Clamp01(1f - shelfRatio) * 0.5f;       // 최대 50% 할인

                MarketListing discountListing = new MarketListing
                {
                    data = data,
                    displayPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f - discount))),
                    remainingShelfDays = shortShelf,
                    discountRate = discount,
                };
                generalListings.Add(discountListing);
            }
        }
    }

    // --- 새벽 시장: 무작위 품목, 30% 할인, 유통기한 최대치 ---
    private void GenerateDawnListings()
    {
        dawnListings.Clear();

        // Fisher-Yates 셔플 없이 간단히 무작위 선택 (중복 방지)
        List<int> indices = new List<int>(allIngredients.Count);
        for (int i = 0; i < allIngredients.Count; i++) indices.Add(i);

        // 💡 이벤트 물가 배율
        float eventMult = RandomEventManager.Instance != null ? RandomEventManager.Instance.GetMarketPriceMultiplier() : 1f;

        int count = Mathf.Min(dawnMarketItemCount, allIngredients.Count);
        for (int i = 0; i < count; i++)
        {
            int randomIdx = UnityEngine.Random.Range(i, indices.Count);

            // Swap
            int temp = indices[i];
            indices[i] = indices[randomIdx];
            indices[randomIdx] = temp;

            IngredientData data = allIngredients[indices[i]];
            int basePrice = Mathf.Max(1, Mathf.RoundToInt(data.basePrice * eventMult));
            int discountedPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f - dawnMarketDiscount)));

            dawnListings.Add(new MarketListing
            {
                data = data,
                displayPrice = discountedPrice,
                remainingShelfDays = data.maxShelfLifeDays, // 새벽 시장은 항상 최상급
                discountRate = dawnMarketDiscount,
            });
        }
    }

    // ===== 구매 로직 =====

    /// <summary>
    /// 일반 마켓 또는 새벽 시장에서 재료를 구매합니다.
    /// </summary>
    public bool BuyListing(MarketListing listing, int amount)
    {
        if (amount <= 0) return false;

        // 장비 조건 체크
        if (listing.data.requiredEquipment != EquipmentType.None)
        {
            if (UpgradeManager.Instance.EquipmentStore == null || !UpgradeManager.Instance.EquipmentStore.HasEquipment(listing.data.requiredEquipment))
            {
                Debug.LogWarning($"<color=red>[구매 실패] {listing.data.ingredientName}을(를) 구매하려면 {listing.data.requiredEquipment} 장비가 필요합니다!</color>");
                return false;
            }
        }

        int totalCost = listing.displayPrice * amount;

        if (PlayerManager.Instance.SpendMoney(totalCost))
        {
            // 구매 시점의 남은 유통기한을 그대로 전달
            InventoryManager.Instance.AddIngredient(listing.data, amount, listing.remainingShelfDays);

            // 💡 지출 추적: 정산 매니저에게 오늘 재료비 보고
            SettlementManager.Instance?.AddExpense(totalCost);

            Debug.Log($"<color=cyan>[구매 성공] {listing.data.ingredientName} {amount}개를 {totalCost}원에 구매! (유통기한: {listing.remainingShelfDays}일)</color>");
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=red>[구매 실패] 잔액이 부족합니다! (필요 금액: {totalCost}원)</color>");
            return false;
        }
    }

    // ===== 외부 접근용 =====

    public List<MarketListing> GetGeneralListings() { return generalListings; }
    public List<MarketListing> GetDawnListings() { return dawnListings; }

    /// <summary>
    /// 현재 새벽 시장이 열려있는 시간대인지 확인합니다.
    /// </summary>
    public bool IsDawnMarketOpen()
    {
        if (GameTimeManager.Instance == null) return false;
        int currentHour = GameTimeManager.Instance.GetCurrentHour();
        return currentHour >= dawnOpenHour && currentHour < dawnCloseHour;
    }

    /// <summary>
    /// 외부에서 특정 재료의 기본 정가를 확인할 때 사용합니다.
    /// </summary>
    public int GetBasePrice(int ingredientID)
    {
        if (catalogDict.TryGetValue(ingredientID, out IngredientData data))
        {
            return data.basePrice;
        }
        return 0;
    }
}