using UnityEngine;
using System.Collections.Generic;

public class Dish
{
    public FoodData foodData;
    public float qualityScore;
    public bool isPremium;

    public string finalFlavorTags;

    // 초기화 시 프리미엄 여부를 받을 수 있도록 수정
    public void Initialize(FoodData data, bool premium, float quality)
    {
        this.foodData = data;
        this.isPremium = premium;
        this.qualityScore = quality;
    }
}

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("References")]
    public CookingPot currentPot;
    public RecipeManager recipeManager;
    public RecipeNamePopupUI recipeNamingUI;
    public PackageController packageController;

    Dish currentCompletedDish;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OnClickCompleteCooking()
    {
        if (currentCompletedDish != null)
        {
            Debug.LogWarning("조리대에 이미 완성된 요리가 있습니다! 먼저 서빙하세요.");
            return;
        }

        var ingredients = currentPot.GetContents();
        if (ingredients.Count == 0) return;

        FoodData resultFood = recipeManager.CheckRecipe(ingredients);

        if (resultFood != null)
        {
            // 💡 냄비에게 최종적으로 프리미엄 요리인지 판별을 요청합니다.
            bool isPremium = currentPot.IsPremiumDish();

            currentCompletedDish = new Dish();
            // 판별된 isPremium 값을 Dish에 저장합니다.
            currentCompletedDish.Initialize(resultFood, isPremium, 1.0f);
            recipeManager.RecordCookedDish(currentCompletedDish.foodData.foodName, currentCompletedDish.isPremium);

            string qualityText = isPremium ? "✨프리미엄✨ " : "일반 ";
            Debug.Log($"<color=green>[요리 완성] {qualityText}{resultFood.foodName}이(가) 조리대에 대기 중입니다.</color>");
            
            // 💡 ResetPot 이전에 포장 로직 호출
            if (packageController != null)
            {
                packageController.PackageDish(resultFood.foodPackageType);
            }

            currentPot.ResetPot();
        }
        else
        {
            Debug.Log("<color=yellow>[레시피 연구] 새로운 조합입니다! 메뉴 이름 짓기 UI를 호출합니다.</color>");
            recipeNamingUI.ShowPopup(ingredients);
            currentPot.ResetPot();
        }
    }

    public Dish GetCompletedDish() => currentCompletedDish;

    public void ClearDish() => currentCompletedDish = null;

    // 💡 Mid-Day Save: 냄비 상태 저장
    public void SavePotState(List<int> ingredientIDs, out int outPremiumCount)
    {
        ingredientIDs.Clear();
        outPremiumCount = 0;
        
        if (currentPot == null) return;

        var contents = currentPot.GetContents();
        foreach (var data in contents)
        {
            ingredientIDs.Add(data.ingredientID);
        }
        
        // Reflection 또는 구조적 한계로 premiumCount 자체는 직접 가져올 수 없지만, 
        // 냄비의 내용물이 전부 프리미엄인지 여부는 알 수 있습니다.
        // 현재는 단순히 IsPremiumDish 판정 여부를 이용하거나 임시로 세팅할 수 있습니다.
        // 완벽 복구를 위해 IngredientBox처럼 품질 점수를 따로 빼진 않으므로 임시로 0 처리 후 필요시 확장 권장
    }

    // 💡 Mid-Day Load: 냄비 상태 복원
    public void RestorePotState(List<int> ingredientIDs, int premiumCount)
    {
        if (currentPot == null || recipeManager == null) return;

        currentPot.ResetPot();

        int currentPremiumApplied = 0;
        foreach (int id in ingredientIDs)
        {
            IngredientData data = recipeManager.GetIngredientById(id);
            if (data != null)
            {
                bool isPremium = currentPremiumApplied < premiumCount;
                currentPot.ReceiveIngredient(data, isPremium);
                if (isPremium) currentPremiumApplied++;
            }
        }
        Debug.Log($"<color=cyan>[CookingManager] 냄비 상태({ingredientIDs.Count}개 재료) 복원 완료.</color>");
    }
}