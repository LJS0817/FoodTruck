using UnityEngine;

[System.Serializable]
public class UpgradeLevelData
{
    public int cost;
    public float value;
}

[CreateAssetMenu(fileName = "New Player Upgrade Data", menuName = "Tycoon/PlayerUpgrade")]
public class PlayerUpgradeData : ScriptableObject
{
    public string upgradeID; // 예: "MaxStamina", "DrainRate", "PremiumChance"
    public string upgradeName;
    public string description;

    // 레벨 0부터 최대 레벨까지의 비용과 수치
    public UpgradeLevelData[] levels;
}
