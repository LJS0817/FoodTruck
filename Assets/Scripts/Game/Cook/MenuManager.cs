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

    private void Start()
    {
        UpdateAvailableRecipes();
    }

    /// <summary>
    /// 현재 재료통에 세팅된 재료들을 기반으로 판매 가능한 모든 레시피를 추출합니다.
    /// </summary>
    public void UpdateAvailableRecipes()
    {
        List<int> ingredientIDs = new List<int>();
        foreach (var box in boxes)
        {
            var data = box.GetCurrentData();
            if (data != null)
            {
                ingredientIDs.Add(data.ingredientID);
            }
        }

        if (ingredientIDs.Count == 0)
        {
            availableRecipes.Clear();
            Debug.Log("[MenuManager] 세팅된 재료가 없어 판매 가능한 레시피가 없습니다.");
            return;
        }

        availableRecipes = CookingManager.Instance.recipeManager.GetRecipesByIngredients(ingredientIDs);
        
        OnMenuUpdated?.Invoke();
        
        Debug.Log($"[MenuManager] 현재 재료로 판매 가능한 레시피: {availableRecipes.Count}개");
        foreach (var recipe in availableRecipes)
        {
            Debug.Log($"- {recipe.foodName}");
        }
    }

    public List<FoodData> GetAvailableRecipes()
    {
        return availableRecipes;
    }

    /// <summary>
    /// 현재 판매 중인 메뉴 중에 특정 유행(FlavorTag)을 만족하는 메뉴가 하나라도 있는지 검사합니다.
    /// </summary>
    public bool HasTrendFlavor(FlavorTag trend)
    {
        if (trend == FlavorTag.None) return true; // 유행이 없다면 항상 통과
        
        foreach (var recipe in availableRecipes)
        {
            if (recipe.flavorTags != null && recipe.flavorTags.Contains(trend))
            {
                return true;
            }
        }
        return false;
    }
}
