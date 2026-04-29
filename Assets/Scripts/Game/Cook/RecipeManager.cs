using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    // 미리 정의된 모든 요리 데이터
    public List<FoodData> allFoodDatabase;
    public List<IngredientData> allIngredient;

    // Key: 재료 해시값, Value: 완성된 요리 데이터
    private Dictionary<int, FoodData> recipeBook = new Dictionary<int, FoodData>(64);
    private Dictionary<int, CustomRecipeData> customRecipeBook = new Dictionary<int, CustomRecipeData>(64);
    Dictionary<int, IngredientData> ingredients = new Dictionary<int, IngredientData>();

    public void InitializeRecipeBook()
    {
        for (int i = 0; i < allIngredient.Count; i++)
        {
            ingredients.Add(allIngredient[i].ingredientID, allIngredient[i]);
        }

        for (int i = 0; i < allFoodDatabase.Count; i++)
        {
            FoodData food = allFoodDatabase[i];
            int hash = CalculateHash(food.requiredIngredients);

            if (!recipeBook.ContainsKey(hash))
            {
                recipeBook.Add(hash, food);
            }
        }

        // 2. 세이브 데이터에서 커스텀 레시피 등록
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

    public IngredientData GetIngredientById(int id)
    {
        if (ingredients.TryGetValue(id, out IngredientData value)) return value;
        return null;
    }

    public bool TryDevelopNewRecipe(IReadOnlyList<IngredientData> currentBowl, string newMenuName, out CustomRecipeData developedRecipe)
    {
        int currentHash = CalculateHash(currentBowl);
        developedRecipe = null;

        // 이미 존재하는 레시피인지 확인 (기본 및 커스텀 모두 체크)
        if (recipeBook.ContainsKey(currentHash) || customRecipeBook.ContainsKey(currentHash))
        {
            Debug.Log("이미 존재하는 레시피 조합입니다!");
            return false;
        }

        // 존재하지 않는다면 새로운 레시피 생성
        developedRecipe = new CustomRecipeData();
        developedRecipe.customFoodName = newMenuName;

        // 재료 ID 리스트 생성 및 총합 가격 계산
        int calculatedPrice = 0;
        for (int i = 0; i < currentBowl.Count; i++)
        {
            developedRecipe.ingredientIDs.Add(currentBowl[i].ingredientID);
            // 필요에 따라 재료별 가격을 합산 (여기서는 임의로 100원씩 더함)
            calculatedPrice += 100;
        }

        // 개발 밸런스에 맞춰 프리미엄이나 가치 상승분 적용 가능
        developedRecipe.basePrice = calculatedPrice + 500;

        // 데이터매니저 세이브 리스트 및 현재 딕셔너리에 추가
        DataManager.Instance.CurrentData.customRecipes.Add(developedRecipe);
        customRecipeBook.Add(currentHash, developedRecipe);

        // 즉시 파일에 저장
        DataManager.Instance.SaveGameData();

        Debug.Log($"<color=magenta>[연금술] 새로운 레시피 개발 성공: {newMenuName}</color>");
        return true;
    }

    public FoodData CheckRecipe(IReadOnlyList<IngredientData> currentBowl)
    {
        int currentHash = CalculateHash(currentBowl);

        if (recipeBook.TryGetValue(currentHash, out FoodData resultFood))
        {
            return resultFood;
        }
        if (customRecipeBook.TryGetValue(currentHash, out CustomRecipeData resultCustom))
        {
            FoodData rst = new FoodData();
            rst.foodName = resultCustom.customFoodName;
            rst.basePrice = resultCustom.basePrice;
            rst.requiredIngredients = new IngredientData[resultCustom.ingredientIDs.Count];
            for(int i = 0; i < resultCustom.ingredientIDs.Count; i++)
            {
                rst.requiredIngredients[i] = GetIngredientById(resultCustom.ingredientIDs[i]);
            }
            
            return rst;
        }

        return null;
    }

    // 💡 추가: 요리가 완성되었을 때 도감 데이터를 업데이트하는 함수
    public void RecordCookedDish(string targetName, bool isPremium)
    {
        List<RecipeSaveData> unlockedList = DataManager.Instance.CurrentData.unlockedRecipes;

        // 1. 이미 도감에 있는지 확인
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
            if (isPremium && !existingRecord.hasPremium)
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
            newRecord.hasPremium = isPremium;

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