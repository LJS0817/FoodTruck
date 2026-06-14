using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class IngredientDataPopulator : EditorWindow
{
    [MenuItem("Tycoon/Tools/Populate All Ingredients")]
    public static void PopulateIngredients()
    {
        string[] guids = AssetDatabase.FindAssets("t:IngredientData");
        
        // 빠른 조회를 위해 생성된(가공된) 재료들을 미리 로드해둡니다.
        Dictionary<string, IngredientData> allIngredients = new Dictionary<string, IngredientData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(path);
            if (data != null && !path.Contains("Processed")) // 기존 Processed 폴더에 남아있는 잔여물은 무시
            {
                allIngredients[data.name] = data;
            }
        }

        int count = 0;
        foreach (var kvp in allIngredients)
        {
            string assetName = kvp.Key;
            IngredientData data = kvp.Value;

            // 1. 기본 정보 설정 (스프라이트 제외)
            if (string.IsNullOrEmpty(data.ingredientName))
            {
                data.ingredientName = assetName;
            }
            if (data.ingredientID == 0)
            {
                // 간단한 해시로 ID 생성 (충돌 방지용 임시 ID)
                data.ingredientID = Mathf.Abs(assetName.GetHashCode()) % 10000;
            }
            
            data.volume = 1.0f;
            data.description = $"신선한 {data.ingredientName}입니다.";
            data.basePrice = 100;
            data.maxShelfLifeDays = 7;
            data.maxPurchaseAmount = 99;

            // 장비 조건 설정 (고기류는 냉장고 필요)
            string lowerName = assetName.ToLower();
            if (lowerName.Contains("beef") || lowerName.Contains("pork") || lowerName.Contains("chicken") || lowerName.Contains("bacon") || lowerName.Contains("sausage"))
            {
                data.requiredEquipment = EquipmentType.Refrigerator;
            }
            else
            {
                data.requiredEquipment = EquipmentType.None;
            }

            data.requiredMiniGame = MiniGameType.None;

            // 맛 태그 적당히 설정
            if (data.flavorTags == null) data.flavorTags = new List<FlavorTag>();
            data.flavorTags.Clear();
            if (lowerName.Contains("hot") || lowerName.Contains("spicy")) data.flavorTags.Add(FlavorTag.Spicy);
            else if (lowerName.Contains("cheese") || lowerName.Contains("bacon")) data.flavorTags.Add(FlavorTag.Salty);
            else if (lowerName.Contains("lettuce") || lowerName.Contains("tomato") || lowerName.Contains("cabbage") || lowerName.Contains("carrot")) data.flavorTags.Add(FlavorTag.Healthy);
            else data.flavorTags.Add(FlavorTag.None);

            // 2. 가공 방식 설정 (ProcessMethods)
            if (data.processMethods == null) data.processMethods = new List<ProcessMethodData>();

            // 가공 방식 자동 할당 (이름이나 경로 기반 휴리스틱)
            string ingPath = AssetDatabase.GetAssetPath(data).ToLower();
            
            bool isMeat = lowerName.Contains("beef") || lowerName.Contains("pork") || lowerName.Contains("chicken") || lowerName.Contains("bacon") || lowerName.Contains("sausage") || lowerName.Contains("patty") || ingPath.Contains("meat");
            bool isVeggie = lowerName.Contains("potato") || lowerName.Contains("carrot") || lowerName.Contains("lettuce") || lowerName.Contains("cabbage") || lowerName.Contains("onion") || lowerName.Contains("tomato") || ingPath.Contains("vegetable");
            bool isBread = lowerName.Contains("bread") || lowerName.Contains("bun") || lowerName.Contains("빵");
            bool isCheese = lowerName.Contains("cheese") || lowerName.Contains("치즈");
            bool isEgg = lowerName.Contains("egg") || lowerName.Contains("계란") || lowerName.Contains("달걀");
            bool canFry = lowerName.Contains("potato") || lowerName.Contains("onion") || lowerName.Contains("chicken") || lowerName.Contains("shrimp");

            if (isMeat)
            {
                TryAddProcessMethod(data, ProcessType.Bake, MiniGameType.Grill);
                TryAddProcessMethod(data, ProcessType.Cut, MiniGameType.Slice);
            }
            if (isVeggie)
            {
                TryAddProcessMethod(data, ProcessType.Cut, MiniGameType.Slice);
            }
            if (canFry)
            {
                TryAddProcessMethod(data, ProcessType.Fry, MiniGameType.Grill);
            }
            if (isBread)
            {
                TryAddProcessMethod(data, ProcessType.Cut, MiniGameType.Slice);
                TryAddProcessMethod(data, ProcessType.Bake, MiniGameType.Grill);
            }
            if (isCheese)
            {
                TryAddProcessMethod(data, ProcessType.Cut, MiniGameType.Slice);
            }
            if (isEgg)
            {
                TryAddProcessMethod(data, ProcessType.Bake, MiniGameType.Grill);
            }

            EditorUtility.SetDirty(data);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>[IngredientDataPopulator] 총 {count}개의 재료 데이터 업데이트 및 가공 방식 세팅 완료!</color>");
    }

    private static void TryAddProcessMethod(IngredientData rawData, ProcessType pType, MiniGameType mType)
    {
        // 이미 해당 가공 방식이 추가되어 있다면 건너뜀 (기획자 수동 수정본 보존)
        if (rawData.GetProcessMethod(pType) != null) return;

        ProcessMethodData newMethod = new ProcessMethodData
        {
            processType = pType,
            requiredMiniGame = mType,
            requiredStamina = 5f,
            stateSteps = new List<IngredientStateEntry>
            {
                new IngredientStateEntry { state = IngredientState.Raw, timeThreshold = 0f, stateSprite = rawData.ingredientSprite },
                new IngredientStateEntry { state = IngredientState.Optimal, timeThreshold = 3f, stateSprite = rawData.ingredientSprite }, //TODO: 추후 스프라이트 교체
                new IngredientStateEntry { state = IngredientState.Ruined, timeThreshold = 8f, stateSprite = rawData.ingredientSprite }  //TODO: 추후 스프라이트 교체
            }
        };
        rawData.processMethods.Add(newMethod);
        Debug.Log($" -> {rawData.ingredientName}에 {pType} 가공 방식 자동 세팅 완료.");
    }
}
