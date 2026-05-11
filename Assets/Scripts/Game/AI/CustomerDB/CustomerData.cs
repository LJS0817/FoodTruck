using UnityEngine;

[CreateAssetMenu(fileName = "New Customer Data", menuName = "Tycoon/Customer")]
public class CustomerData : ScriptableObject
{
    [Header("Stats")]
    public string customerName;
    public float maxPatience;
    public float walkSpeed;
    public bool isVIP;
    public FoodData[] favoriteFoods;

    [Header("Identity")]
    // 💡 이 손님의 성별을 지정합니다. 매니저가 이 값을 보고 옷을 입혀줍니다.
    public Gender gender;

    [Header("Taste Preference")]
    // 💡 이 손님이 선호하는 맛 태그. 메뉴 선택 시 가중치로 작용합니다.
    public FlavorTag[] preferredFlavors;

    [Header("VIP Settings")]
    [Tooltip("VIP 손님일 경우 팁 배율 (기본 2.0배)")]
    public float vipTipMultiplier = 2.0f;
}