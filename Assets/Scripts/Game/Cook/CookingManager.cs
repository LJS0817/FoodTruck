using UnityEngine;

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
            recipeManager.RecordCookedDish(currentCompletedDish);

            string qualityText = isPremium ? "✨프리미엄✨ " : "일반 ";
            Debug.Log($"<color=green>[요리 완성] {qualityText}{resultFood.foodName}이(가) 조리대에 대기 중입니다.</color>");
            currentPot.ResetPot();
        }
        else
        {
            currentPot.ResetPot();
        }
    }

    public Dish GetCompletedDish() => currentCompletedDish;

    public void ClearDish() => currentCompletedDish = null;
}