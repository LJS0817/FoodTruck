using UnityEngine;

/// <summary>
/// Manages the Truck Management UI, which contains three sub‑panels:
/// Upgrade, Interior (tool/furniture placement), and Hiring (worker management).
/// The Upgrade panel is shown by default when the Truck Management tab is opened.
/// Sub‑menu buttons should call the public methods ShowUpgrade, ShowInterior, and ShowHiring.
/// </summary>
public class TruckManagementUIController : MonoBehaviour
{
    [Header("Sub Panels")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject interiorPanel;
    [SerializeField] private GameObject hiringPanel;

    /// <summary>
    /// Open the Truck Management UI with the default (Upgrade) panel active.
    /// Called by MainUINavigationController when the Truck Management tab is selected.
    /// </summary>
    public void OpenDefault()
    {
        ShowPanel(upgradePanel);
    }

    /// <summary>
    /// Show the Upgrade sub‑panel.
    /// </summary>
    public void ShowUpgrade()
    {
        ShowPanel(upgradePanel);
    }

    /// <summary>
    /// Show the Interior sub‑panel (tool/furniture placement).
    /// </summary>
    public void ShowInterior()
    {
        ShowPanel(interiorPanel);
    }

    /// <summary>
    /// Show the Hiring sub‑panel (worker hiring/firing).
    /// </summary>
    public void ShowHiring()
    {
        ShowPanel(hiringPanel);
    }

    /// <summary>
    /// Helper to activate the requested panel and deactivate the others.
    /// </summary>
    private void ShowPanel(GameObject panel)
    {
        // Deactivate all panels first.
        if (upgradePanel) upgradePanel.SetActive(false);
        if (interiorPanel) interiorPanel.SetActive(false);
        if (hiringPanel) hiringPanel.SetActive(false);

        // Activate the requested panel if it exists.
        if (panel) panel.SetActive(true);
    }

    /// <summary>
    /// Close the entire Truck Management UI (used when switching away from the tab).
    /// </summary>
    public void CloseUI()
    {
        if (upgradePanel) upgradePanel.SetActive(false);
        if (interiorPanel) interiorPanel.SetActive(false);
        if (hiringPanel) hiringPanel.SetActive(false);
    }
}
