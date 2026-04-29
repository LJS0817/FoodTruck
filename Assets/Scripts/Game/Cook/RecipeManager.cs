using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    // 미리 정의된 모든 요리 데이터
    public List<FoodData> allFoodDatabase;

    // Key: 재료 해시값, Value: 완성된 요리 데이터
    private Dictionary<int, FoodData> recipeBook = new Dictionary<int, FoodData>(64);

    public void InitializeRecipeBook()
    {
        for (int i = 0; i < allFoodDatabase.Count; i++)
        {
            FoodData food = allFoodDatabase[i];
            int hash = CalculateHash(food.requiredIngredients);

            if (!recipeBook.ContainsKey(hash))
            {
                recipeBook.Add(hash, food);
            }
            else
            {
                Debug.LogWarning($"[RecipeManager] 중복된 레시피 해시 발견: {food.foodName}");
            }
        }
    }

    public FoodData CheckRecipe(IReadOnlyList<IngredientData> currentBowl)
    {
        int currentHash = CalculateHash(currentBowl);

        if (recipeBook.TryGetValue(currentHash, out FoodData resultFood))
        {
            return resultFood;
        }

        return null;
    }

    // 💡 추가: 요리가 완성되었을 때 도감 데이터를 업데이트하는 함수
    public void RecordCookedDish(Dish completedDish)
    {
        if (completedDish == null || completedDish.foodData == null) return;

        string targetName = completedDish.foodData.foodName;
        List<RecipeSaveData> unlockedList = DataManager.Instance.CurrentData.unlockedRecipes;

        // 1. 이미 도감에 있는지 확인 (LINQ 대신 for문 사용)
        RecipeSaveData existingRecord = null;
        for (int i = 0; i < unlockedList.Count; i++)
        {
            if (unlockedList[i].foodName == targetName)
            {
                existingRecord = unlockedList[i];
                break;
            }
        }

        bool isDirty = false; // 세이브가 필요한지 확인하는 플래그

        if (existingRecord != null)
        {
            // 2. 이미 해금된 요리라면, 프리미엄 달성 여부만 새로 체크
            if (completedDish.isPremium && !existingRecord.hasPremium)
            {
                existingRecord.hasPremium = true;
                isDirty = true;
                Debug.Log($"<color=yellow>[도감] {targetName} 프리미엄 달성 기록!</color>");
            }
        }
        else
        {
            // 3. 처음 만든 요리라면 도감에 새로 추가
            RecipeSaveData newRecord = new RecipeSaveData();
            newRecord.foodName = targetName;
            newRecord.isUnlocked = true;
            newRecord.hasPremium = completedDish.isPremium;

            unlockedList.Add(newRecord);
            isDirty = true;
            Debug.Log($"<color=cyan>[도감] 신규 레시피 해금: {targetName}</color>");
        }

        // 변화가 있을 때만 저장하여 모바일 기기 부하 감소
        if (isDirty)
        {
            DataManager.Instance.SaveGameData();
        }
    }

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
}