using System.Collections.Generic;
using UnityEngine;

public class RecipeBookUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject recipeBookPanel;
    public Transform contentParent;
    public RecipeSlotUI slotPrefab;

    private List<RecipeSlotUI> spawnedSlots = new List<RecipeSlotUI>();

    public void OpenRecipeBook()
    {
        recipeBookPanel.SetActive(true);
        RefreshUI();
    }

    public void CloseRecipeBook()
    {
        recipeBookPanel.SetActive(false);
    }

    private void RefreshUI()
    {
        // 1. 슬롯 초기화 (추후 오브젝트 풀링 적용 권장)
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            Destroy(spawnedSlots[i].gameObject);
        }
        spawnedSlots.Clear();

        // 2. 기본 레시피 도감 출력
        List<FoodData> allRecipes = GameManager.Instance.recipeManager.allFoodDatabase;
        List<RecipeSaveData> savedRecord = DataManager.Instance.CurrentData.unlockedRecipes;

        for (int i = 0; i < allRecipes.Count; i++)
        {
            FoodData recipe = allRecipes[i];
            RecipeSlotUI newSlot = Instantiate(slotPrefab, contentParent);

            RecipeSaveData myRecord = null;

            // 세이브 기록 순회 
            for (int j = 0; j < savedRecord.Count; j++)
            {
                if (savedRecord[j].foodName == recipe.foodName)
                {
                    myRecord = savedRecord[j];
                    break;
                }
            }

            bool isUnlocked = myRecord != null && myRecord.isUnlocked;
            bool hasPremium = myRecord != null && myRecord.hasPremium;

            newSlot.SetupSlot(recipe, isUnlocked, hasPremium);
            spawnedSlots.Add(newSlot);
        }

        // 3. 유저가 개발한 커스텀 레시피 도감 출력
        List<CustomRecipeData> customRecords = DataManager.Instance.CurrentData.customRecipes;
        for (int i = 0; i < customRecords.Count; i++)
        {
            CustomRecipeData customRecipe = customRecords[i];
            RecipeSlotUI customSlot = Instantiate(slotPrefab, contentParent);

            // 커스텀 레시피용 슬롯 설정 (RecipeSlotUI에 해당 함수 추가 필요)
            customSlot.SetupCustomSlot(customRecipe.customFoodName, customRecipe.basePrice);
            spawnedSlots.Add(customSlot);
        }
    }
}