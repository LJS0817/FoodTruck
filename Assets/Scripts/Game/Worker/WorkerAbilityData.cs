using UnityEngine;

[CreateAssetMenu(fileName = "New Worker Ability Data", menuName = "Tycoon/Worker Ability Data")]
public class WorkerAbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public WorkerAbility abilityType;
    public string abilityName; // 예: "홀 매니저", "메인 셰프"
    public Sprite abilityIcon;
    [TextArea]
    public string description;

    [Header("Value Ranges")]
    [Tooltip("F등급은 능력이 없으므로 E~S 등급별 기본 부여 수치입니다.")]
    public float minBaseValue = 0.05f; // 예: 5%
    public float maxBaseValue = 0.20f; // 예: 20%
}
