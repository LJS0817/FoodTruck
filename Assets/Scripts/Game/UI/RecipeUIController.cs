using UnityEngine;

/// <summary>
/// Controls the Recipe UI panel. This controller is invoked from the bottom navigation bar
/// when the user selects the "Recipe" tab. It provides simple open/close methods that can be
/// expanded later to populate the UI with the player's discovered recipes.
/// </summary>
public class RecipeUIController : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;

    /// <summary>
    /// Open the Recipe UI panel.
    /// </summary>
    public void OpenUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Close the Recipe UI panel.
    /// </summary>
    public void CloseUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
