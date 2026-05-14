using UnityEngine;

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
