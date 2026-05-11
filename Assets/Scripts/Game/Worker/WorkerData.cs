using UnityEngine;

public enum WorkerAbility
{
    None,
    PatienceBoost,   // 손님 인내심 감소 속도 저하
    StaminaSaver,    // 사장님 피로도 감소 속도 저하
    AutoCookSpeedUp, // 자동 요리 속도 증가
    SpawnRateBoost   // 손님 방문 확률 증가
}

[CreateAssetMenu(fileName = "New Worker Data", menuName = "Tycoon/Worker")]
public class WorkerData : ScriptableObject
{
    public int workerID;
    public string workerName;
    public Sprite workerIcon;
    public string description;

    [Header("Economy")]
    public int hiringCost;   // 최초 고용 시 비용
    public int dailySalary;  // 매일 차감되는 일급

    [Header("Ability")]
    public WorkerAbility ability;
    public float abilityValue; // 예: 0.15 (15% 효과)
}
