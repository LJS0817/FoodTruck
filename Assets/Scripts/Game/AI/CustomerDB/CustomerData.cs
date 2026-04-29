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
}