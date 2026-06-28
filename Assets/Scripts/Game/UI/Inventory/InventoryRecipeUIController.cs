using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리와 레시피를 하나의 통합된 창 안에서 서브 탭으로 관리하는 컨트롤러입니다.
/// </summary>
public class InventoryRecipeUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup inventoryRecipeUIRoot;
    [Header("Sub UI Controllers")]
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private RecipeBookUI recipeBookUI;
    [SerializeField] ScrollRect scrollRect;

    [Header("Sub Menu Buttons")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button recipeTabButton;

    private void Start()
    {
        if (inventoryTabButton != null)
            inventoryTabButton.onClick.AddListener(ShowInventory);
        
        if (recipeTabButton != null)
            recipeTabButton.onClick.AddListener(ShowRecipe);

        // 스크립트 시작 시 숨김 처리
        CloseUI();
    }

    /// <summary>
    /// 메인 네비게이션에서 이 통합 창을 열 때 호출됩니다.
    /// 기본적으로 인벤토리를 보여줍니다.
    /// </summary>
    public void OpenUI()
    {
        inventoryRecipeUIRoot.alpha = 1f;
        inventoryRecipeUIRoot.interactable = true;
        inventoryRecipeUIRoot.blocksRaycasts = true;
        ShowInventory();
    }

    /// <summary>
    /// 메인 네비게이션에서 다른 탭으로 넘어갈 때 이 통합 창을 닫습니다.
    /// </summary>
    public void CloseUI()
    {
        if (inventoryUI != null) inventoryUI.CloseInventory();
        if (recipeBookUI != null) recipeBookUI.CloseRecipeBook();
        
        inventoryRecipeUIRoot.alpha = 0f;
        inventoryRecipeUIRoot.interactable = false;
        inventoryRecipeUIRoot.blocksRaycasts = false;

        scrollRect.content = inventoryUI.GetContent();
    }

    /// <summary>
    /// 인벤토리 서브 탭을 엽니다.
    /// </summary>
    public void ShowInventory()
    {
        if (inventoryUI != null) inventoryUI.OpenInventory();
        if (recipeBookUI != null) recipeBookUI.CloseRecipeBook();
        
        UpdateButtonStates(true);
    }

    /// <summary>
    /// 레시피 서브 탭을 엽니다.
    /// </summary>
    public void ShowRecipe()
    {
        if (inventoryUI != null) inventoryUI.CloseInventory();
        if (recipeBookUI != null) recipeBookUI.OpenRecipeBook();
        
        UpdateButtonStates(false);
    }

    /// <summary>
    /// 현재 선택된 서브 탭에 맞게 버튼 상태(Interactable)를 업데이트합니다.
    /// </summary>
    private void UpdateButtonStates(bool isInventorySelected)
    {
        if (inventoryTabButton != null) 
            inventoryTabButton.interactable = !isInventorySelected;
            
        if (recipeTabButton != null) 
            recipeTabButton.interactable = isInventorySelected;

        scrollRect.content = isInventorySelected ? inventoryUI.GetContent() : recipeBookUI.GetContent();
    }
}
