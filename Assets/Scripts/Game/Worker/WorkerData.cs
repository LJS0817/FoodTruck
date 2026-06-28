using System;
using System.Collections.Generic;
using UnityEngine;

public enum WorkerAbility
{
    None,
    PatienceBoost,       // 손님 인내심 감소 속도 저하 (홀 매니저)
    StaminaSaver,        // 사장님 피로도 감소 속도 저하 (주방 보조)
    AutoCookSpeedUp,     // 자동 요리 속도 증가 (조리사)
    SpawnRateBoost,      // 손님 방문 확률 증가 (호객꾼)
    TipBonus,            // 팁 발생 확률 및 금액 증가 (미소 천사)
    IngredientDiscount,  // 상점 재료 구매 시 할인 적용 (흥정의 달인)
    HygieneSaver,        // 청결도 하락 속도 저하 (청소 반장)
    PremiumRateBoost,    // 프리미엄/비싼 요리 주문 확률 증가 (영업 사원)
    WeatherResist,       // 악천후 페널티 감소 (날씨 요정)
    WaitingCapacity,     // 웨이팅 줄 최대 길이 확장 (줄세우기 장인)
    SellPriceBonus,      // 전체 판매 수익률 보너스 (회계사)
    CookingMinigameEasy, // 미니게임(썰기/젓기) 판정 완화 (수석 셰프)
    VIPSpawnBoost,       // VIP 등장 확률 증가 (인맥왕)
    MarketRefreshDiscount, // 시장 새로고침/갱신 비용 할인 (정보원)
    OvertimeBonus        // 밤(심야) 영업 시 보너스 적용 (야행성 올빼미)
}

public enum WorkerGrade
{
    C, B, A, S
}

public enum WorkerSpecialty
{
    Cook,           // 주방 특화
    Service,        // 홀/접객 특화
    Maintenance,    // 잡일/청소 특화
    Balanced        // 올라운더 (평균)
}

[Serializable]
public class WorkerAbilityNode
{
    public WorkerAbility abilityType;
    public float baseValue;
}

[Serializable]
public class WorkerData
{
    public string workerID;
    public string workerName;
    public WorkerGrade grade;
    
    [Header("Economy")]
    public int hiringCost;   // 최초 고용 시 비용
    public int baseDailySalary;  // 기본 일급
    public float incentiveMultiplier = 1.0f; // 현재 적용 중인 인센티브 배율 (기본 1.0)
    
    [Header("Growth")]
    public int currentLevel = 1;
    public int maxLevel;
    
    [Header("Base Stats (능력치)")]
    public WorkerSpecialty specialty;
    public int cookSkill;    // 요리 숙련도
    public int humanSkill;   // 손님 응대 숙련도
    public int stamina;      // 피곤함 정도 (체력)
    public int cleanSkill;   // 청소 숙련도

    [Header("Abilities (스킬)")]
    public List<WorkerAbilityNode> abilities = new List<WorkerAbilityNode>();

    [Header("Runtime State (런타임 상태)")]
    public float currentStamina;
    public bool isResting;
    public float restTimer;
    public bool isPendingRest; // 하던 일을 마치고 퇴근하기 위한 대기 상태

    public int GetAppearanceSeed()
    {
        if (string.IsNullOrEmpty(workerID))
            return 0;
        return workerID.GetHashCode();
    }
}
