using UnityEngine;

/// <summary>
/// Controls the Recipe UI panel. This controller is invoked from the bottom navigation bar
/// when the user selects the "Recipe" tab. It provides simple open/close methods that can be
/// expanded later to populate the UI with the player's discovered recipes.
/// </summary>
public class RecipeUIController : MonoBehaviour
{
    /// <summary>
    /// Open the Recipe UI panel.
    /// </summary>
    public void OpenUI()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Close the Recipe UI panel.
    /// </summary>
    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
