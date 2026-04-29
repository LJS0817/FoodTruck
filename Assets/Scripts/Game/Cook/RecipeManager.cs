using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public List<FoodData> allFoodDatabase;

    // 기본 레시피 캐싱용
    private Dictionary<int, FoodData> recipeBook = new Dictionary<int, FoodData>(64);
    // 유저가 새롭게 개발한 커스텀 레시피 캐싱용
    private Dictionary<int, CustomRecipeData> customRecipeBook = new Dictionary<int, CustomRecipeData>(64);

    public void InitializeRecipeBook()
    {
        // 1. 기본 레시피 등록
        for (int i = 0; i < allFoodDatabase.Count; i++)
        {
            FoodData food = allFoodDatabase[i];
            int hash = CalculateHash(food.requiredIngredients);

            if (!recipeBook.ContainsKey(hash))
            {
                recipeBook.Add(hash, food);
            }
        }

        // 2. 세이브 데이터에서 커스텀 레시피 불러와서 등록
        List<CustomRecipeData> savedCustomRecipes = DataManager.Instance.CurrentData.customRecipes;
        for (int i = 0; i < savedCustomRecipes.Count; i++)
        {
            CustomRecipeData custom = savedCustomRecipes[i];
            int hash = CalculateHashFromIDs(custom.ingredientIDs);

            if (!customRecipeBook.ContainsKey(hash))
            {
                customRecipeBook.Add(hash, custom);
            }
        }
    }

    // 💡 신규 레시피 개발 및 저장 로직
    public bool TryDevelopNewRecipe(IReadOnlyList<IngredientData> currentBowl, string newMenuName, out CustomRecipeData developedRecipe)
    {
        int currentHash = CalculateHash(currentBowl);
        developedRecipe = null;

        // 이미 존재하는 레시피인지 확인 (기본/커스텀 모두 체크)
        if (recipeBook.ContainsKey(currentHash) || customRecipeBook.ContainsKey(currentHash))
        {
            return false;
        }

        developedRecipe = new CustomRecipeData();
        developedRecipe.customFoodName = newMenuName;

        int calculatedPrice = 0;
        for (int i = 0; i < currentBowl.Count; i++)
        {
            developedRecipe.ingredientIDs.Add(currentBowl[i].ingredientID);
            calculatedPrice += 100; // 기획에 맞춰 재료별 가치 합산 로직 추가 가능
        }

        developedRecipe.basePrice = calculatedPrice + 500;

        // 세이브 데이터에 추가 및 딕셔너리 캐싱
        DataManager.Instance.CurrentData.customRecipes.Add(developedRecipe);
        customRecipeBook.Add(currentHash, developedRecipe);

        // 모바일 PlayerPrefs 즉시 저장
        DataManager.Instance.SaveGameData();

        Debug.Log($"<color=magenta>[연금술] 새로운 레시피 개발 성공: {newMenuName}</color>");
        return true;
    }

    // 그릇에 담긴 재료가 유효한 레시피인지 확인
    public bool CheckIfRecipeExists(IReadOnlyList<IngredientData> currentBowl, out string recipeName)
    {
        int currentHash = CalculateHash(currentBowl);
        recipeName = string.Empty;

        if (recipeBook.TryGetValue(currentHash, out FoodData resultFood))
        {
            recipeName = resultFood.foodName;
            return true;
        }

        if (customRecipeBook.TryGetValue(currentHash, out CustomRecipeData resultCustom))
        {
            recipeName = resultCustom.customFoodName;
            return true;
        }

        return false;
    }

    // 💡 추가: 요리가 완성되거나 신규 개발되었을 때 도감 세이브 데이터를 업데이트하는 함수
    public void RecordCookedDish(string targetName, bool isPremium)
    {
        List<RecipeSaveData> unlockedList = DataManager.Instance.CurrentData.unlockedRecipes;

        RecipeSaveData existingRecord = null;
        for (int i = 0; i < unlockedList.Count; i++)
        {
            if (unlockedList[i].foodName == targetName)
            {
                existingRecord = unlockedList[i];
                break;
            }
        }

        bool isDirty = false;

        if (existingRecord != null)
        {
            // 이미 해금된 요리라면 프리미엄 달성 여부만 체크
            if (isPremium && !existingRecord.hasPremium)
            {
                existingRecord.hasPremium = true;
                isDirty = true;
                Debug.Log($"<color=yellow>[도감] {targetName} 프리미엄 달성 기록!</color>");
            }
        }
        else
        {
            // 처음 만든 요리(또는 방금 개발한 신메뉴)라면 새로 추가
            RecipeSaveData newRecord = new RecipeSaveData();
            newRecord.foodName = targetName;
            newRecord.isUnlocked = true;
            newRecord.hasPremium = isPremium;

            unlockedList.Add(newRecord);
            isDirty = true;
            Debug.Log($"<color=cyan>[도감] 신규 레시피 해금: {targetName}</color>");
        }

        // 변화가 있을 때만 저장
        if (isDirty)
        {
            DataManager.Instance.SaveGameData();
        }
    }

    // FoodData용 해시 계산
    private int CalculateHash(IReadOnlyList<IngredientData> ingredients)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < ingredients.Count; i++)
            {
                hash = hash * 31 + ingredients[i].ingredientID;
            }
            return hash;
        }
    }

    // 커스텀 레시피 ID 전용 해시 계산
    private int CalculateHashFromIDs(List<int> ingredientIDs)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < ingredientIDs.Count; i++)
            {
                hash = hash * 31 + ingredientIDs[i];
            }
            return hash;
        }
    }
}