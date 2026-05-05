using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipeStoreItem
{
    public FoodData recipeData;
    public int price;
}

public class RecipeStoreManager : MonoBehaviour
{
    [SerializeField] private List<RecipeStoreItem> recipeCatalog;

    public event Action OnRecipeUnlocked;

    public bool BuyRecipe(FoodData recipe, int price)
    {
        // 이미 해금되었는지 확인
        if (IsRecipeUnlocked(recipe.foodName))
        {
            Debug.LogWarning($"[RecipeStoreManager] 이미 {recipe.foodName} 레시피를 보유 중입니다.");
            return false;
        }

        if (PlayerManager.Instance.SpendMoney(price))
        {
            UnlockRecipe(recipe.foodName);
            Debug.Log($"<color=cyan>[레시피 구매] {recipe.foodName} 구매 완료! ({price}원)</color>");
            OnRecipeUnlocked?.Invoke();
            return true;
        }
        
        Debug.LogWarning($"<color=red>[레시피 구매 실패] 잔액이 부족합니다! ({price}원)</color>");
        return false;
    }

    public bool IsRecipeUnlocked(string foodName)
    {
        var unlocked = DataManager.Instance.CurrentData.unlockedRecipes;
        for (int i = 0; i < unlocked.Count; i++)
        {
            if (unlocked[i].foodName == foodName && unlocked[i].isUnlocked) return true;
        }
        return false;
    }

    private void UnlockRecipe(string foodName)
    {
        var unlocked = DataManager.Instance.CurrentData.unlockedRecipes;
        bool found = false;
        for (int i = 0; i < unlocked.Count; i++)
        {
            if (unlocked[i].foodName == foodName)
            {
                unlocked[i].isUnlocked = true;
                found = true;
                break;
            }
        }

        if (!found)
        {
            unlocked.Add(new RecipeSaveData { foodName = foodName, isUnlocked = true });
        }

        DataManager.Instance.SaveGameData();
    }

    public List<RecipeStoreItem> GetCatalog() => recipeCatalog;
}
