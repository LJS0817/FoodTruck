using UnityEngine;

public enum ProcessType
{
    None,
    Bake,   // 굽기 (그릴)
    Fry,    // 튀기기 (튀김기)
    Blend,  // 갈기 (믹서기)
    Cut,    // 자르기 (도마)
    Frozen, // 냉동 (냉동고)
    Cool,   // 냉장 (냉장고)
}

[CreateAssetMenu(fileName = "New Process Recipe", menuName = "Tycoon/ProcessRecipe")]
public class ProcessRecipeData : ScriptableObject
{
    public IngredientData inputIngredient;
    public ProcessType processType;
    public IngredientData outputIngredient;
    
    [Header("가공 설정")]
    public float processTime = 3.0f;       // 기본 가공 시간 (장비 보너스로 단축 가능)
    public int requiredStamina = 5;        // 기본 체력 소모 (장비 보너스로 감소 가능)

    [Header("미니게임 연동")]
    [Tooltip("가공 시 실행할 미니게임. None이면 미니게임 없이 시간만 소요됩니다.")]
    public MiniGameType miniGameType = MiniGameType.None;
}
