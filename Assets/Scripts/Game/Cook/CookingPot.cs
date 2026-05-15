using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 요리를 위해 재료를 담는 냄비(조리 공간).
/// 미니게임은 더 이상 여기서 실행되지 않습니다.
/// 재료 가공(Process)은 ProcessManager에서 사전에 처리된 후, 결과물 IngredientData를 이 냄비에 넣습니다.
/// 프리미엄 여부는 인벤토리에서 꺼낼 때 태그된 품질 정보에 따라 결정됩니다.
/// </summary>
public class CookingPot : MonoBehaviour
{
    [Header("Settings")]
    public Transform ingredientStackParent;
    public float stackOffset = 0.2f;

    private List<IngredientData> contents = new List<IngredientData>(10);
    private List<GameObject> visualStack = new List<GameObject>(10);
    private int premiumCount = 0;

    // ─── 재료 투입 ────────────────────────────────────────

    /// <summary>
    /// 외부(인벤토리 드래그, IngredientBox 등)에서 재료를 냄비에 넣을 때 호출합니다.
    /// 가공이 필요한 재료라면 ProcessManager.ExecuteProcess 를 먼저 거쳐야 합니다.
    /// </summary>
    /// <param name="data">이미 가공이 완료된 IngredientData</param>
    /// <param name="isPremium">가공 미니게임 결과로 판정된 프리미엄 여부</param>
    public void ReceiveIngredient(IngredientData data, bool isPremium = false)
    {
        AddIngredientToPot(data, isPremium);
    }

    private void AddIngredientToPot(IngredientData data, bool isPremium)
    {
        contents.Add(data);
        if (isPremium) premiumCount++;

        UpdateVisualStack(data);

        string qualityMark = isPremium ? "✨" : "";
        Debug.Log($"[CookingPot] {qualityMark}{data.ingredientName} 추가됨. 현재 재료 수: {contents.Count}");

        CheckCurrentRecipe();
    }

    // ─── 리셋 ────────────────────────────────────────────

    public void ResetPot()
    {
        contents.Clear();
        premiumCount = 0;

        for (int i = 0; i < visualStack.Count; i++)
        {
            Destroy(visualStack[i]);
        }
        visualStack.Clear();
    }

    // ─── 비주얼 ─────────────────────────────────────────

    private void UpdateVisualStack(IngredientData data)
    {
        GameObject visual = new GameObject(data.ingredientName + "_Visual");
        visual.transform.SetParent(ingredientStackParent);

        float yPos = visualStack.Count * stackOffset;
        visual.transform.localPosition = new Vector3(0, yPos, 0);

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = data.ingredientSprite;
        renderer.sortingOrder = 10 + visualStack.Count;

        visualStack.Add(visual);
    }

    // ─── 레시피 판별 ─────────────────────────────────────

    public void CheckCurrentRecipe()
    {
        if (contents.Count == 0) return;

        FoodData result = GameManager.Instance.recipeManager.CheckRecipe(contents);

        if (result != null)
            Debug.Log($"<color=green>[판별 완료] 현재 요리: {result.foodName}</color>");
        else
            Debug.Log("<color=red>[판별 실패] 일치하는 레시피가 없습니다.</color>");
    }

    // ─── 공개 쿼리 ──────────────────────────────────────

    public IReadOnlyList<IngredientData> GetContents() => contents;

    /// <summary>
    /// 요리 완성 시 프리미엄 여부를 판별합니다.
    /// 모든 재료가 프리미엄이거나, 업그레이드 확률 보너스가 발동되면 프리미엄입니다.
    /// </summary>
    public bool IsPremiumDish()
    {
        if (contents.Count == 0) return false;

        if (premiumCount == contents.Count) return true;

        if (UpgradeManager.Instance.Upgrade != null)
        {
            float bonusChance = UpgradeManager.Instance.Upgrade.GetCurrentValue("PremiumChance");
            if (bonusChance > 0f && Random.value < bonusChance)
            {
                Debug.Log($"<color=yellow>[손재주 발동] 업그레이드 효과로 프리미엄 판정! ({bonusChance * 100}% 확률)</color>");
                return true;
            }
        }

        return false;
    }
}