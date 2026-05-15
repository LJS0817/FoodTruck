using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
public class AutoAssetInjector : Editor
{
    [MenuItem("Tycoon/Auto Inject All Assets to Managers")]
    public static void InjectAssets()
    {
        // 1. WorkerManager
        var workerManager = FindObjectOfType<WorkerManager>();
        if (workerManager != null)
        {
            workerManager.allWorkers = LoadAllAssets<WorkerData>("Assets/ScriptableObjects/Worker");
            EditorUtility.SetDirty(workerManager);
            Debug.Log($"[Auto Inject] Injected {workerManager.allWorkers.Count} Workers.");
        }

        // 2. PlayerUpgradeManager
        var upgradeManager = FindObjectOfType<PlayerUpgradeManager>();
        if (upgradeManager != null)
        {
            upgradeManager.allUpgrades = LoadAllAssets<PlayerUpgradeData>("Assets/ScriptableObjects/PlayerUpgrade");
            EditorUtility.SetDirty(upgradeManager);
            Debug.Log($"[Auto Inject] Injected {upgradeManager.allUpgrades.Count} Player Upgrades.");
        }

        // 3. AchievementManager
        var achievementManager = FindObjectOfType<AchievementManager>();
        if (achievementManager != null)
        {
            achievementManager.allAchievements = LoadAllAssets<AchievementData>("Assets/ScriptableObjects/Achievement");
            EditorUtility.SetDirty(achievementManager);
            Debug.Log($"[Auto Inject] Injected {achievementManager.allAchievements.Count} Achievements.");
        }

        // 4. RandomEventManager
        var randomEventManager = FindObjectOfType<RandomEventManager>();
        if (randomEventManager != null)
        {
            randomEventManager.allEvents = LoadAllAssets<RandomEventData>("Assets/ScriptableObjects/RandomEvent");
            EditorUtility.SetDirty(randomEventManager);
            Debug.Log($"[Auto Inject] Injected {randomEventManager.allEvents.Count} Random Events.");
        }

        // 5. MarketingManager
        var marketingManager = FindObjectOfType<MarketingManager>();
        if (marketingManager != null)
        {
            marketingManager.allMarketingCampaigns = LoadAllAssets<MarketingData>("Assets/ScriptableObjects/Marketing");
            EditorUtility.SetDirty(marketingManager);
            Debug.Log($"[Auto Inject] Injected {marketingManager.allMarketingCampaigns.Count} Marketing Campaigns.");
        }

        // 6. RecipeManager (Ingredients)
        var recipeManager = FindObjectOfType<RecipeManager>();
        if (recipeManager != null)        {
            recipeManager.allIngredient = LoadAllAssets<IngredientData>("Assets/ScriptableObjects/Ingredient");
            recipeManager.allFoodDatabase = LoadAllAssets<FoodData>("Assets/ScriptableObjects/Food");
            EditorUtility.SetDirty(recipeManager);
            Debug.Log($"[Auto Inject] Injected {recipeManager.allIngredient.Count} Ingredients.");
        }

        // 7. DistrictManager
        var districtManager = FindObjectOfType<DistrictManager>();
        if (districtManager != null)
        {
            districtManager.allDistricts = LoadAllAssets<DistrictData>("Assets/ScriptableObjects/District");
            districtManager.allDistricts = districtManager.allDistricts.OrderBy(d => d.districtID).ToList();
            EditorUtility.SetDirty(districtManager);
            Debug.Log($"[Auto Inject] Injected {districtManager.allDistricts.Count} Districts.");
        }

        // 8. RecipeStoreManager
        var recipeStoreManager = FindObjectOfType<RecipeStoreManager>();
        if (recipeStoreManager != null)
        {
            var allFoods = LoadAllAssets<FoodData>("Assets/ScriptableObjects/Food");
            SerializedObject so = new SerializedObject(recipeStoreManager);
            SerializedProperty catalogProp = so.FindProperty("recipeCatalog");
            
            catalogProp.ClearArray();
            for(int i = 0; i < allFoods.Count; i++)
            {
                catalogProp.InsertArrayElementAtIndex(i);
                SerializedProperty elementProp = catalogProp.GetArrayElementAtIndex(i);
                
                elementProp.FindPropertyRelative("recipeData").objectReferenceValue = allFoods[i];
                // 음식 기본 판매가의 10배를 레시피 해금 가격으로 설정
                elementProp.FindPropertyRelative("price").intValue = allFoods[i].basePrice * 10; 
                elementProp.FindPropertyRelative("maxPurchaseAmount").intValue = 1;
            }
            
            so.ApplyModifiedProperties();
            Debug.Log($"[Auto Inject] Injected {allFoods.Count} Recipes into RecipeStoreManager.");
        }

        // 9. EquipmentStoreManager
        var equipmentManager = FindObjectOfType<EquipmentStoreManager>();
        if (equipmentManager != null)
        {
            var allEquipments = LoadAllAssets<EquipmentData>("Assets/ScriptableObjects/Equipment");
            SerializedObject so = new SerializedObject(equipmentManager);
            SerializedProperty equipmentsProp = so.FindProperty("allEquipments");
            
            equipmentsProp.ClearArray();
            for(int i = 0; i < allEquipments.Count; i++)
            {
                equipmentsProp.InsertArrayElementAtIndex(i);
                equipmentsProp.GetArrayElementAtIndex(i).objectReferenceValue = allEquipments[i];
            }
            so.ApplyModifiedProperties();
            Debug.Log($"[Auto Inject] Injected {allEquipments.Count} Equipments.");
        }

        // 10. WaitingZoneManager
        var waitingZoneManager = FindObjectOfType<WaitingZoneManager>();
        if (waitingZoneManager != null)
        {
            waitingZoneManager.allWaitingZoneItems = LoadAllAssets<WaitingZoneItemData>("Assets/ScriptableObjects/WaitingZoneItem");
            EditorUtility.SetDirty(waitingZoneManager);
            Debug.Log($"[Auto Inject] Injected {waitingZoneManager.allWaitingZoneItems.Count} Waiting Zone Items.");
        }

        // 11. ProcessManager
        var processManager = FindObjectOfType<ProcessManager>();
        if (processManager != null)
        {
            processManager.allProcessRecipes = LoadAllAssets<ProcessRecipeData>("Assets/ScriptableObjects/ProcessRecipe");
            EditorUtility.SetDirty(processManager);
            Debug.Log($"[Auto Inject] Injected {processManager.allProcessRecipes.Count} Process Recipes.");
        }

        // 저장 (씬에 변경사항이 생겼음을 Unity에 알림)
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log("<color=green>[Auto Inject] 모든 데이터가 성공적으로 주입되었습니다!</color>");
    }

    private static List<T> LoadAllAssets<T>(string folderPath) where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folderPath });
        List<T> assets = new List<T>();
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
}
#endif
