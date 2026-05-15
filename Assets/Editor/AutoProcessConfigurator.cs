#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoProcessConfigurator : EditorWindow
{
    [MenuItem("Tycoon/Auto Configure Food Processes")]
    public static void ShowWindow()
    {
        GetWindow<AutoProcessConfigurator>("Auto Configure Processes");
    }

    private void OnGUI()
    {
        GUILayout.Label("모든 요리(FoodData)의 재료를 분석하여\n자동으로 가공(Process) 설정을 적용합니다.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("자동 설정 및 에셋 생성 실행"))
        {
            RunAutoConfiguration();
        }
    }

    private static void RunAutoConfiguration()
    {
        string[] foodGuids = AssetDatabase.FindAssets("t:FoodData");
        int count = 0;

        foreach (string guid in foodGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FoodData food = AssetDatabase.LoadAssetAtPath<FoodData>(path);

            if (food != null && food.requiredIngredients != null && food.requiredIngredients.Length > 0)
            {
                // 이미 가공 설정이 기존 재료 수와 일치한다면 덮어쓰지 않음 (기획자 수정분 보존)
                // 만약 덮어쓰고 싶다면 아래 조건을 빼면 됩니다.
                if (food.ingredientConfigs == null || food.ingredientConfigs.Length != food.requiredIngredients.Length)
                {
                    food.ingredientConfigs = new FoodIngredientConfig[food.requiredIngredients.Length];
                    
                    for (int i = 0; i < food.requiredIngredients.Length; i++)
                    {
                        IngredientData rawIng = food.requiredIngredients[i];
                        if (rawIng == null) continue;

                        ProcessType decidedProcess = ProcessType.None;
                        string ingPath = AssetDatabase.GetAssetPath(rawIng);
                        string ingName = rawIng.ingredientName.ToLower();

                        // 1. 고기류 (Meats 폴더) -> 굽기(Bake)
                        if (ingPath.Contains("/Meats/"))
                        {
                            decidedProcess = ProcessType.Bake;
                        }
                        // 2. 야채류 (Vegetables 폴더) -> 자르기(Cut)
                        else if (ingPath.Contains("/Vegetables/"))
                        {
                            decidedProcess = ProcessType.Cut;
                        }
                        // 3. 빵류 -> 기본적으로 자르기(Cut) 할당 (필요 시 기획자가 Bake 등으로 변경 가능)
                        else if (ingName.Contains("빵") || ingName.Contains("bread") || ingName.Contains("번") || ingName.Contains("bun"))
                        {
                            decidedProcess = ProcessType.Cut;
                        }
                        // 4. 계란류 -> 굽기(Bake) (후라이)
                        else if (ingName.Contains("계란") || ingName.Contains("달걀") || ingName.Contains("egg"))
                        {
                            decidedProcess = ProcessType.Bake; 
                        }

                        food.ingredientConfigs[i] = new FoodIngredientConfig
                        {
                            rawIngredient = rawIng,
                            processType = decidedProcess
                        };
                    }
                }

                // 설정이 완료되었으므로, FoodData 내부의 Generate 함수를 호출하여 
                // 에셋(IngredientData, ProcessRecipeData) 자동 생성 및 requiredIngredients 갱신
                food.GenerateProcessedIngredients();
                EditorUtility.SetDirty(food);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>[AutoProcessConfigurator] 총 {count}개의 FoodData에 자동 가공 설정이 적용되었습니다!</color>");
    }
}
#endif
