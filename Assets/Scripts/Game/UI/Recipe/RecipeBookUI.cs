using System.Collections.Generic;
using UnityEngine;

public class RecipeBookUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject recipeBookPanel;      // 도감 전체 창 (Canvas)
    public Transform contentParent;         // ScrollView의 Content 오브젝트
    public RecipeSlotUI slotPrefab;         // 각 레시피를 표시할 프리팹 (버튼 형태)

    private List<RecipeSlotUI> spawnedSlots = new List<RecipeSlotUI>();

    // 버튼 이벤트로 호출하여 도감 열기
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
        // 1. 기존에 생성된 슬롯 초기화 (오브젝트 풀링을 쓰면 더 좋습니다)
        foreach (var slot in spawnedSlots) { Destroy(slot.gameObject); }
        spawnedSlots.Clear();

        // 2. 전체 레시피 데이터 가져오기
        List<FoodData> allRecipes = GameManager.Instance.recipeManager.allRecipesInGame;
        List<RecipeSaveData> savedRecord = DataManager.Instance.CurrentData.unlockedRecipes;

        // 3. 슬롯 생성 및 데이터 주입
        foreach (FoodData recipe in allRecipes)
        {
            RecipeSlotUI newSlot = Instantiate(slotPrefab, contentParent);

            // 이 레시피가 세이브 데이터에 있는지 검색
            RecipeSaveData myRecord = savedRecord.Find(x => x.foodName == recipe.foodName);

            bool isUnlocked = myRecord != null && myRecord.isUnlocked;
            bool hasPremium = myRecord != null && myRecord.hasPremium;

            // 슬롯 UI 업데이트 (비밀/해금 상태 표시)
            newSlot.SetupSlot(recipe, isUnlocked, hasPremium);

            spawnedSlots.Add(newSlot);
        }
    }
}