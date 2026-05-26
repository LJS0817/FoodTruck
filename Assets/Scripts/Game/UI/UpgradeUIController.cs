using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the Upgrade panel.
/// Allows the player to increase:
///  • The number of usable IngredientBox slots.
///  • The speed multiplier of tools (e.g., chopping, stirring).
///  • The quality multiplier of tools (affects premium ingredient generation).
///  • Skill / passive level count.
/// Each upgrade costs in‑game currency and raises the corresponding stat by one step.
/// </summary>
public class UpgradeUIController : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // Upgrade data (could be moved to a separate model later)
    // ---------------------------------------------------------------------
    private const int MaxIngredientBoxLevel = 10;
    private const int MaxToolSpeedLevel   = 5;
    private const int MaxToolQualityLevel = 5;
    private const int MaxSkillLevel       = 5;

    // Current levels – persisted via PlayerPrefs for this prototype.
    private int _ingredientBoxLevel = 0;
    private int _toolSpeedLevel     = 0;
    private int _toolQualityLevel   = 0;
    private int _skillLevel         = 0;

    // ---------------------------------------------------------------------
    // UI References (assign in the Inspector)
    // ---------------------------------------------------------------------
    [Header("Ingredient Box Upgrade UI")]
    [SerializeField] private Button _btnUpgradeIngredientBox;
    [SerializeField] private TMP_Text _txtIngredientBoxLevel;
    [SerializeField] private TMP_Text _txtIngredientBoxCost;

    [Header("Tool Speed Upgrade UI")]
    [SerializeField] private Button _btnUpgradeToolSpeed;
    [SerializeField] private TMP_Text _txtToolSpeedLevel;
    [SerializeField] private TMP_Text _txtToolSpeedCost;

    [Header("Tool Quality Upgrade UI")]
    [SerializeField] private Button _btnUpgradeToolQuality;
    [SerializeField] private TMP_Text _txtToolQualityLevel;
    [SerializeField] private TMP_Text _txtToolQualityCost;

    [Header("Skill / Passive Upgrade UI")]
    [SerializeField] private Button _btnUpgradeSkill;
    [SerializeField] private TMP_Text _txtSkillLevel;
    [SerializeField] private TMP_Text _txtSkillCost;

    // ---------------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------------
    private void Awake()
    {
        LoadSavedData();
        BindButtonEvents();
        RefreshAllUI();
    }

    // ---------------------------------------------------------------------
    // Public API – called from MainUINavigationController or other scripts
    // ---------------------------------------------------------------------
    public void OpenUI()
    {
        gameObject.SetActive(true);
        RefreshAllUI();
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------------
    // Upgrade implementations
    // ---------------------------------------------------------------------
    private void UpgradeIngredientBox()
    {
        if (_ingredientBoxLevel >= MaxIngredientBoxLevel) return;
        int cost = GetIngredientBoxUpgradeCost();
        if (CurrencyManager.TrySpend(cost))
        {
            _ingredientBoxLevel++;
            // Here you would notify the system that the max box count increased.
            // e.g., IngredientBoxManager.SetMaxBoxes(_ingredientBoxLevel + 1);
            SaveData();
            RefreshIngredientBoxUI();
        }
    }

    private void UpgradeToolSpeed()
    {
        if (_toolSpeedLevel >= MaxToolSpeedLevel) return;
        int cost = GetToolSpeedUpgradeCost();
        if (CurrencyManager.TrySpend(cost))
        {
            _toolSpeedLevel++;
            // Apply multiplier – e.g., ToolManager.SetSpeedMultiplier(1f + 0.1f * _toolSpeedLevel);
            SaveData();
            RefreshToolSpeedUI();
        }
    }

    private void UpgradeToolQuality()
    {
        if (_toolQualityLevel >= MaxToolQualityLevel) return;
        int cost = GetToolQualityUpgradeCost();
        if (CurrencyManager.TrySpend(cost))
        {
            _toolQualityLevel++;
            // Apply multiplier – e.g., ToolManager.SetQualityMultiplier(1f + 0.1f * _toolQualityLevel);
            SaveData();
            RefreshToolQualityUI();
        }
    }

    private void UpgradeSkill()
    {
        if (_skillLevel >= MaxSkillLevel) return;
        int cost = GetSkillUpgradeCost();
        if (CurrencyManager.TrySpend(cost))
        {
            _skillLevel++;
            // Activate/passivate new skill – e.g., SkillManager.EnableSkill(_skillLevel);
            SaveData();
            RefreshSkillUI();
        }
    }

    // ---------------------------------------------------------------------
    // Cost formulas (simple exponential scaling)
    // ---------------------------------------------------------------------
    private int GetIngredientBoxUpgradeCost() => 100 * (int)Mathf.Pow(2, _ingredientBoxLevel);
    private int GetToolSpeedUpgradeCost()   => 200 * (int)Mathf.Pow(2, _toolSpeedLevel);
    private int GetToolQualityUpgradeCost() => 200 * (int)Mathf.Pow(2, _toolQualityLevel);
    private int GetSkillUpgradeCost()      => 500 * (int)Mathf.Pow(2, _skillLevel);

    // ---------------------------------------------------------------------
    // UI Refresh helpers
    // ---------------------------------------------------------------------
    private void RefreshAllUI()
    {
        RefreshIngredientBoxUI();
        RefreshToolSpeedUI();
        RefreshToolQualityUI();
        RefreshSkillUI();
    }

    private void RefreshIngredientBoxUI()
    {
        _txtIngredientBoxLevel?.SetText($"Level {_ingredientBoxLevel}/{MaxIngredientBoxLevel}");
        _txtIngredientBoxCost?.SetText(_ingredientBoxLevel < MaxIngredientBoxLevel ? $"Cost: {GetIngredientBoxUpgradeCost()}" : "MAX");
        if (_btnUpgradeIngredientBox != null)
            _btnUpgradeIngredientBox.interactable = _ingredientBoxLevel < MaxIngredientBoxLevel && CurrencyManager.CanSpend(GetIngredientBoxUpgradeCost());
    }

    private void RefreshToolSpeedUI()
    {
        _txtToolSpeedLevel?.SetText($"Level {_toolSpeedLevel}/{MaxToolSpeedLevel}");
        _txtToolSpeedCost?.SetText(_toolSpeedLevel < MaxToolSpeedLevel ? $"Cost: {GetToolSpeedUpgradeCost()}" : "MAX");
        if (_btnUpgradeToolSpeed != null)
            _btnUpgradeToolSpeed.interactable = _toolSpeedLevel < MaxToolSpeedLevel && CurrencyManager.CanSpend(GetToolSpeedUpgradeCost());
    }

    private void RefreshToolQualityUI()
    {
        _txtToolQualityLevel?.SetText($"Level {_toolQualityLevel}/{MaxToolQualityLevel}");
        _txtToolQualityCost?.SetText(_toolQualityLevel < MaxToolQualityLevel ? $"Cost: {GetToolQualityUpgradeCost()}" : "MAX");
        if (_btnUpgradeToolQuality != null)
            _btnUpgradeToolQuality.interactable = _toolQualityLevel < MaxToolQualityLevel && CurrencyManager.CanSpend(GetToolQualityUpgradeCost());
    }

    private void RefreshSkillUI()
    {
        _txtSkillLevel?.SetText($"Level {_skillLevel}/{MaxSkillLevel}") ;
        _txtSkillCost?.SetText(_skillLevel < MaxSkillLevel ? $"Cost: {GetSkillUpgradeCost()}" : "MAX");
        if (_btnUpgradeSkill != null)
            _btnUpgradeSkill.interactable = _skillLevel < MaxSkillLevel && CurrencyManager.CanSpend(GetSkillUpgradeCost());
    }

    // ---------------------------------------------------------------------
    // Persistence (simple PlayerPrefs implementation for demonstration)
    // ---------------------------------------------------------------------
    private void SaveData()
    {
        PlayerPrefs.SetInt("Upgrade_IngredientBoxLevel", _ingredientBoxLevel);
        PlayerPrefs.SetInt("Upgrade_ToolSpeedLevel",   _toolSpeedLevel);
        PlayerPrefs.SetInt("Upgrade_ToolQualityLevel", _toolQualityLevel);
        PlayerPrefs.SetInt("Upgrade_SkillLevel",       _skillLevel);
        PlayerPrefs.Save();
    }

    private void LoadSavedData()
    {
        _ingredientBoxLevel = PlayerPrefs.GetInt("Upgrade_IngredientBoxLevel", 0);
        _toolSpeedLevel     = PlayerPrefs.GetInt("Upgrade_ToolSpeedLevel", 0);
        _toolQualityLevel   = PlayerPrefs.GetInt("Upgrade_ToolQualityLevel", 0);
        _skillLevel         = PlayerPrefs.GetInt("Upgrade_SkillLevel", 0);
    }

    // ---------------------------------------------------------------------
    // Hook up UI button clicks
    // ---------------------------------------------------------------------
    private void BindButtonEvents()
    {
        if (_btnUpgradeIngredientBox != null) _btnUpgradeIngredientBox.onClick.AddListener(UpgradeIngredientBox);
        if (_btnUpgradeToolSpeed != null)    _btnUpgradeToolSpeed.onClick.AddListener(UpgradeToolSpeed);
        if (_btnUpgradeToolQuality != null)  _btnUpgradeToolQuality.onClick.AddListener(UpgradeToolQuality);
        if (_btnUpgradeSkill != null)        _btnUpgradeSkill.onClick.AddListener(UpgradeSkill);
    }
}

/// <summary>
/// Simple static currency manager used by the UpgradeUIController.
/// Replace with the actual game‑wide currency system when it becomes available.
/// </summary>
public static class CurrencyManager
{
    // For this prototype we store the current amount in PlayerPrefs.
    private const string CurrencyKey = "PlayerCurrency";

    public static int Current => PlayerPrefs.GetInt(CurrencyKey, 0);

    public static bool CanSpend(int amount) => Current >= amount;

    public static bool TrySpend(int amount)
    {
        if (!CanSpend(amount)) return false;
        PlayerPrefs.SetInt(CurrencyKey, Current - amount);
        PlayerPrefs.Save();
        return true;
    }

    // Utility to add currency – e.g., when the player earns money.
    public static void Add(int amount)
    {
        PlayerPrefs.SetInt(CurrencyKey, Current + amount);
        PlayerPrefs.Save();
    }
}
