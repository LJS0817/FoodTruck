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


[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tycoon/Ingredient")]
public class IngredientData : ScriptableObject
{
    public int ingredientID;
    public string ingredientName;
    public Sprite ingredientSprite;
    public float volume;
    public string description;


    [Header("경제")]
    public int basePrice;               // 재료의 기본 정가
    public int maxShelfLifeDays = 7;    // 구매 시점부터 최대 유통기한(일 수)
    public int maxPurchaseAmount = 99;  // 1회 최대 구매 가능 수량

    [Header("장비 조건")]
    public EquipmentType requiredEquipment = EquipmentType.None; // 이 재료를 구매/보관하려면 필요한 장비

    // 💡 이 재료를 냄비에 넣을 때 어떤 미니게임이 뜰지 결정합니다.
    public MiniGameType requiredMiniGame;

    [Header("맛 태그")]
    public System.Collections.Generic.List<FlavorTag> flavorTags;
}

[CreateAssetMenu(fileName = "New Food", menuName = "Tycoon/Food")]
public class FoodData : ScriptableObject
{
    public string foodName;
    public int basePrice;
    public float autoCookTime = 5.0f;
    public FoodPackageType foodPackageType;

    public Sprite iconSprite;

    // 이 요리를 만들기 위한 '정확한 순서'의 재료 배열
    public IngredientData[] requiredIngredients;
    
    // 이 요리를 만들기 위해 필요한 장비들
    public EquipmentType[] requiredEquipments;

    [Header("맛 태그")]
    public System.Collections.Generic.List<FlavorTag> flavorTags;
}