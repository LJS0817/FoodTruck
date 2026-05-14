using System;
using UnityEngine;

public enum MiniGameType
{
    None,
    Mash,   // 으깨기 (폭풍 터치)
    Stir    // 젓기 (타이밍)
}

public enum FoodPackageType {
    Container,
    Wrapper,
}

public enum EquipmentType
{
    None,           // 장비 불필요
    Grill,          // 그릴
    Refrigerator,   // 냉장고
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
}