using System;
using UnityEngine;

public enum MiniGameType
{
    None,
    Mash,   // 으깨기 (폭풍 터치) - Blend
    Stir,   // 젓기 (타이밍) - 요리맥니스용
    Slice,  // 썰기 (스와이프) - Cut
    Grill,  // 굽기 (온도 유지) - Bake / Fry
}

public enum FoodPackageType {
    Container,
    Wrapper,
}

public enum EquipmentType
{
    None,           // 장비 불필요
    Grill,          // 그릴 (Bake)
    Blender,        // 믹서기 (Blend)
    CuttingBoard,   // 도마 (Cut)
    Fryer,          // 튀김기 (Fry)
    Refrigerator,   // 냉장고 (Cool)
    Freezer,        // 냉동고 (Frozen)
    Battery,        // 전기 배터리
    Gas,            // 가스통
    Generator,      // 발전기
    Hood,           // 그릴 후드
    Kiosk,          // 키오스크
}

public enum FlavorTag
{
    None,
    Spicy,
    Sweet,
    Salty,
    Sour,
    Bitter,
    Warm,
    Cold,
    Greasy,
    Healthy,
}



[Serializable]
public struct FoodIngredientConfig
{
    public IngredientData rawIngredient;
    public ProcessType processType;
}

[CreateAssetMenu(fileName = "New Food", menuName = "Tycoon/Food")]
public class FoodData : ScriptableObject
{
    public string foodName;
    public int basePrice;
    public float autoCookTime = 5.0f;
    public FoodPackageType foodPackageType;

    public Sprite iconSprite;

    public bool isCustomRecipe; // 커스텀 레시피 여부

    // 이 요리를 만들기 위한 '정확한 순서'의 재료 배열
    public IngredientData[] requiredIngredients;
    
    // 이 요리를 만들기 위해 필요한 장비들
    public EquipmentType[] requiredEquipments;

    [Header("맛 태그")]
    public System.Collections.Generic.List<FlavorTag> flavorTags;

    [Header("가공 설정 (자동 생성용)")]
    [Tooltip("여기에 원재료와 가공 방식을 설정한 뒤, 우클릭 > Generate Processed Ingredients를 실행하세요.")]
    public FoodIngredientConfig[] ingredientConfigs;

#if UNITY_EDITOR
    [ContextMenu("Generate Processed Ingredients & Recipes")]
    public void GenerateProcessedIngredients()
    {
        if (ingredientConfigs == null || ingredientConfigs.Length == 0)
        {
            Debug.LogWarning("ingredientConfigs가 비어 있습니다.");
            return;
        }

        requiredIngredients = new IngredientData[ingredientConfigs.Length];
        string ingredientFolder = "Assets/ScriptableObjects/Ingredient/Processed";
        string processRecipeFolder = "Assets/ScriptableObjects/ProcessRecipe/Generated";

        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Ingredient"))
            UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Ingredient");
        if (!UnityEditor.AssetDatabase.IsValidFolder(ingredientFolder))
            UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects/Ingredient", "Processed");

        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/ScriptableObjects/ProcessRecipe"))
            UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects", "ProcessRecipe");
        if (!UnityEditor.AssetDatabase.IsValidFolder(processRecipeFolder))
            UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects/ProcessRecipe", "Generated");

        for (int i = 0; i < ingredientConfigs.Length; i++)
        {
            var config = ingredientConfigs[i];
            if (config.rawIngredient == null) continue;

            if (config.processType == ProcessType.None)
            {
                requiredIngredients[i] = config.rawIngredient;
                continue;
            }

            string processedIngName = $"{config.rawIngredient.name}_{config.processType}";
            string processedIngPath = $"{ingredientFolder}/{processedIngName}.asset";

            IngredientData processedIng = UnityEditor.AssetDatabase.LoadAssetAtPath<IngredientData>(processedIngPath);
            if (processedIng == null)
            {
                processedIng = ScriptableObject.CreateInstance<IngredientData>();
                processedIng.ingredientID = config.rawIngredient.ingredientID + (int)config.processType * 1000; // Unique ID
                processedIng.ingredientName = $"{config.processType} {config.rawIngredient.ingredientName}";
                processedIng.ingredientSprite = config.rawIngredient.ingredientSprite; // TODO: 가공된 스프라이트로 변경 필요
                processedIng.basePrice = config.rawIngredient.basePrice + 100; // 가공 시 가격 상승
                processedIng.maxShelfLifeDays = config.rawIngredient.maxShelfLifeDays;
                
                UnityEditor.AssetDatabase.CreateAsset(processedIng, processedIngPath);
                Debug.Log($"[FoodData] 생성됨: {processedIngPath}");
            }

            string recipeName = $"{config.rawIngredient.name}_to_{config.processType}";
            string recipePath = $"{processRecipeFolder}/{recipeName}.asset";

            ProcessRecipeData recipeData = UnityEditor.AssetDatabase.LoadAssetAtPath<ProcessRecipeData>(recipePath);
            if (recipeData == null)
            {
                recipeData = ScriptableObject.CreateInstance<ProcessRecipeData>();
                recipeData.inputIngredient = config.rawIngredient;
                recipeData.processType = config.processType;
                recipeData.outputIngredient = processedIng;
                recipeData.processTime = 3.0f;
                recipeData.requiredStamina = 5;

                // ProcessType별 기본 미니게임 자동 할당
                recipeData.miniGameType = config.processType switch
                {
                    ProcessType.Bake   => MiniGameType.Grill,
                    ProcessType.Fry    => MiniGameType.Grill,
                    ProcessType.Blend  => MiniGameType.Mash,
                    ProcessType.Cut    => MiniGameType.Slice,
                    _                  => MiniGameType.None   // Frozen, Cool, None
                };

                UnityEditor.AssetDatabase.CreateAsset(recipeData, recipePath);
                Debug.Log($"[FoodData] 레시피 생성됨: {recipePath}");
            }

            requiredIngredients[i] = processedIng;
        }

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"<color=green>[FoodData] '{foodName}'의 가공 데이터 연동 완료!</color>");
    }
#endif
}