using UnityEngine;

[CreateAssetMenu(fileName = "New District Data", menuName = "Tycoon/District")]
public class DistrictData : ScriptableObject
{
    public int districtID;
    public string districtName;
    public string description;
    public Sprite backgroundSprite; // 이 구역의 배경 이미지

    [Header("Requirements & Costs")]
    public int requiredReputation;  // 해금에 필요한 최소 평판
    public int unlockCost;          // 해금(자릿세) 비용
    public int dailyRent;           // 매일 내야 하는 자릿세 유지비

    [Header("Customer Pool")]
    // 💡 이 구역에서 등장할 수 있는 손님 타입들 (주택가, 오피스, 번화가 등 분리)
    public CustomerData[] districtCustomerTypes;
}
