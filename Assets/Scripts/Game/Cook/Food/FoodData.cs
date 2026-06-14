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

public enum ItemGrade
{
    Normal,
    Premium,
    Perfect
}

public enum ProcessType
{
    None,
    Bake,
    Fry,
    Blend,
    Cut,
    Cool,
    Frozen
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

    // 이 요리를 만들기 위해 필요한 장비들
    public EquipmentType[] requiredEquipments;

    [Header("맛 태그")]
    public System.Collections.Generic.List<FlavorTag> flavorTags;

    [Header("가공 설정")]
    [Tooltip("이 요리에 들어가는 원재료와 권장 가공 방식의 목록입니다.")]
    public FoodIngredientConfig[] ingredientConfigs;
}