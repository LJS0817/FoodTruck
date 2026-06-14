using UnityEngine;

[CreateAssetMenu(fileName = "New Truck", menuName = "Tycoon/Truck Data")]
public class TruckData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("트럭의 이름")]
    public string truckName;
    
    [Tooltip("트럭에 대한 설명")]
    [TextArea] 
    public string truckDescription;
    
    [Tooltip("상점 구매 가격")]
    public int purchasePrice;

    [Header("스펙")]
    [Tooltip("연료 최대 용량 (발전기 가동 시간 등)")]
    public float fuelCapacity;
    
    [Tooltip("발전기 용량 (동시 가동 가능한 장비 수치 한도 등)")]
    public float generatorCapacity;

    [Header("내부 구조")]
    [Tooltip("트럭 내부의 장비 배치 순서. (배열 요소가 null이면 카운터, EquipmentData면 해당 장비)")]
    public EquipmentData[] stationLayout;
}
