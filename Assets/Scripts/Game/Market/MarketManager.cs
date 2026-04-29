using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    [Header("Market Settings")]
    public List<IngredientData> marketCatalog; // 도매시장에서 취급하는 재료 목록
    public float priceFluctuationRate = 0.3f;  // 가격 변동폭 (기본가의 ±30%)

    // 💡 Key: 재료 ID, Value: 오늘의 변동된 가격
    // O(1) 탐색을 위해 딕셔너리 사용
    private Dictionary<int, int> todayPrices = new Dictionary<int, int>(32);

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 테스트를 위해 시작 시 시세 1회 갱신 (실제로는 '하루 영업 시작' 시점에 호출)
        GenerateDailyPrices();
    }

    // 1. 매일 새벽(하루의 시작)에 호출되어 시세를 갱신합니다.
    public void GenerateDailyPrices()
    {
        todayPrices.Clear();

        for (int i = 0; i < marketCatalog.Count; i++)
        {
            IngredientData data = marketCatalog[i];

            // 기본가 설정이 없다고 가정하고 임의의 기본가를 100~500원으로 부여 (추후 Data에 추가 권장)
            int basePrice = 200;

            // ±30% 범위에서 랜덤 가격 생성 (Mathf.RoundToInt로 정수화)
            float randomModifier = Random.Range(1f - priceFluctuationRate, 1f + priceFluctuationRate);
            int finalPrice = Mathf.RoundToInt(basePrice * randomModifier);

            todayPrices.Add(data.ingredientID, finalPrice);

            Debug.Log($"[새벽시장] {data.ingredientName}의 오늘 시세: {finalPrice}원 ({(randomModifier * 100):F1}%)");
        }

        // TODO: UI 갱신을 위한 Action 이벤트 호출 (OnPricesGenerated 등)
    }

    // 2. 재료 구매 로직
    public void BuyIngredient(int ingredientID, int amount)
    {
        if (amount <= 0) return;

        if (todayPrices.TryGetValue(ingredientID, out int currentPrice))
        {
            int totalCost = currentPrice * amount;

            // PlayerManager를 통해 돈이 충분한지 확인하고 지불
            if (PlayerManager.Instance.SpendMoney(totalCost))
            {
                // 지불에 성공하면 인벤토리에 재료 추가
                InventoryManager.Instance.AddIngredient(ingredientID, amount);
                Debug.Log($"<color=cyan>[구매 성공] {amount}개의 재료를 {totalCost}원에 구매했습니다!</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>[구매 실패] 잔액이 부족합니다! (필요 금액: {totalCost}원)</color>");
            }
        }
        else
        {
            Debug.LogError($"[MarketManager] 오류: ID {ingredientID}인 재료는 오늘 시장에 없습니다.");
        }
    }

    // 외부(UI 등)에서 특정 재료의 오늘 시세를 확인할 때 사용
    public int GetTodayPrice(int ingredientID)
    {
        return todayPrices.TryGetValue(ingredientID, out int price) ? price : 0;
    }
}