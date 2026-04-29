using UnityEngine;

// 💡 수정 1: Dish 클래스가 기본/커스텀 레시피를 모두 호환하도록 FoodData 객체 대신 '이름(string)'을 가지도록 변경
public class Dish
{
    public string foodName;
    public float qualityScore;
    public bool isPremium;

    public string finalFlavorTags;

    public void Initialize(string name, bool premium, float quality)
    {
        this.foodName = name;
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

        // 💡 수정 2: 개편된 레시피 매니저의 CheckIfRecipeExists() 사용 (이름 반환)
        if (recipeManager.CheckIfRecipeExists(ingredients, out string recipeName))
        {
            // 기존 도감에 있는 요리 (기본 or 이미 개발한 커스텀 요리)
            bool isPremium = currentPot.IsPremiumDish();

            currentCompletedDish = new Dish();
            currentCompletedDish.Initialize(recipeName, isPremium, 1.0f);

            // 💡 수정 3: 개편된 도감 기록 함수에 맞춰 이름과 프리미엄 여부만 전달
            recipeManager.RecordCookedDish(recipeName, isPremium);

            string qualityText = isPremium ? "✨프리미엄✨ " : "일반 ";
            Debug.Log($"<color=green>[요리 완성] {qualityText}{recipeName}이(가) 조리대에 대기 중입니다.</color>");

            currentPot.ResetPot();
        }
        else
        {
            // 💡 수정 4: 신규 레시피 조합 발견! (냄비를 비우지 않고 대기)
            Debug.Log("<color=yellow>[레시피 연구] 등록되지 않은 새로운 조합입니다! 메뉴 이름 짓기 UI를 호출합니다.</color>");

            // TODO: UI 매니저를 통해 "메뉴 이름 입력 팝업"을 띄웁니다.
            recipeNamingUI.ShowPopup(ingredients);
            // 팝업에서 확인 버튼을 누르면 RecipeManager.TryDevelopNewRecipe()를 호출하고 냄비를 비우도록 연결합니다.
        }
    }

    public Dish GetCompletedDish() => currentCompletedDish;

    public void ClearDish() => currentCompletedDish = null;
}