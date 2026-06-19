using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Ingredient Boxes")]
    public List<IngredientBox> boxes;

    private List<FoodData> availableRecipes = new List<FoodData>();

    public event System.Action OnMenuUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // 씬에 배치된 모든 재료 상자를 자동으로 찾아 등록합니다.
        if (boxes == null || boxes.Count == 0)
        {
            boxes = new List<IngredientBox>(FindObjectsByType<IngredientBox>(FindObjectsSortMode.None));
        }
    }


    /// <summary>
    /// 장사 시작 전 팝업에서 확정된 오늘의 메뉴 리스트를 받아와 세팅합니다.
    /// </summary>
    public void SetTodayMenu(List<FoodData> selectedRecipes)
    {
        availableRecipes.Clear();

        if (selectedRecipes == null || selectedRecipes.Count == 0)
        {
            Debug.LogWarning("[MenuManager] 전달받은 메뉴가 없습니다.");
        }
        else
        {
            availableRecipes.AddRange(selectedRecipes);
        }

        _cachedTrend = FlavorTag.None;
        
        OnMenuUpdated?.Invoke();
        
        Debug.Log($"<color=cyan>[MenuManager] 오늘의 판매 메뉴 세팅 완료: {availableRecipes.Count}개</color>");
        foreach (var recipe in availableRecipes)
        {
            Debug.Log($"- {recipe.foodName}");
        }
    }

    public List<FoodData> GetAvailableRecipes()
    {
        return availableRecipes;
    }

    private FlavorTag _cachedTrend = FlavorTag.None;
    private bool _hasCachedTrend = false;

    /// <summary>
    /// 현재 판매 중인 메뉴 중에 특정 유행(FlavorTag)을 만족하는 메뉴가 하나라도 있는지 검사합니다.
    /// </summary>
    public bool HasTrendFlavor(FlavorTag trend)
    {
        if (trend == FlavorTag.None) return true; // 유행이 없다면 항상 통과
        
        if (_cachedTrend == trend) return _hasCachedTrend;

        _cachedTrend = trend;
        _hasCachedTrend = false;

        for (int i = 0; i < availableRecipes.Count; i++)
        {
            var recipe = availableRecipes[i];
            if (recipe.flavorTags != null && recipe.flavorTags.Contains(trend))
            {
                _hasCachedTrend = true;
                break;
            }
        }
        return _hasCachedTrend;
    }
}
