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

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tycoon/Ingredient")]
public class IngredientData : ScriptableObject
{
    public int ingredientID;
    public string ingredientName;
    public Sprite ingredientSprite;

    // 💡 이 재료를 냄비에 넣을 때 어떤 미니게임이 뜰지 결정합니다.
    public MiniGameType requiredMiniGame;
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
}