using UnityEngine;

public class BankManager : MonoBehaviour
{
    public static BankManager Instance { get; private set; }

    private const float DAILY_INTEREST_RATE = 0.05f; // 일일 이자 5%
    private const int MAX_BANKRUPT_DAYS = 7;         // 7일 연속 적자 시 파산 패널티

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += ProcessDailyBank;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= ProcessDailyBank;
        }
    }

    // ===== 대출 및 상환 =====

    public int CurrentLoan => DataManager.Instance != null && DataManager.Instance.CurrentData != null 
                              ? DataManager.Instance.CurrentData.bankLoan : 0;

    public void TakeLoan(int amount)
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;

        DataManager.Instance.CurrentData.bankLoan += amount;
        PlayerManager.Instance.AddMoney(amount);
        Debug.Log($"<color=orange>[은행] {amount}원을 대출받았습니다. (현재 총 대출: {DataManager.Instance.CurrentData.bankLoan}원)</color>");
    }

    public bool RepayLoan(int amount)
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return false;

        int actualRepay = Mathf.Min(amount, DataManager.Instance.CurrentData.bankLoan);
        if (actualRepay <= 0) return false;

        if (PlayerManager.Instance.SpendMoney(actualRepay))
        {
            DataManager.Instance.CurrentData.bankLoan -= actualRepay;
            // 상환은 비용(Expenses)에 포함할지 말지는 자유지만, 보통 재무제표상 부채 상환은 비용이 아님.
            Debug.Log($"<color=green>[은행] {actualRepay}원의 대출을 상환했습니다. (남은 대출: {DataManager.Instance.CurrentData.bankLoan}원)</color>");
            return true;
        }
        return false;
    }

    // ===== 일일 이자 및 파산 체크 =====

    private void ProcessDailyBank()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;

        // 1. 대출 이자 차감
        int currentLoan = DataManager.Instance.CurrentData.bankLoan;
        if (currentLoan > 0)
        {
            int interest = Mathf.RoundToInt(currentLoan * DAILY_INTEREST_RATE);
            PlayerManager.Instance.SpendMoney(interest);
            SettlementManager.Instance?.AddExpense(interest);
            Debug.Log($"<color=red>[은행] 대출 이자 {interest}원이 차감되었습니다.</color>");
        }

        // 2. 파산 상태 체크 (돈이 마이너스인가?)
        if (PlayerManager.Instance.CurrentMoney < 0)
        {
            DataManager.Instance.CurrentData.bankruptDays++;
            Debug.LogWarning($"[은행] 적자 상태입니다! (누적 파산 일수: {DataManager.Instance.CurrentData.bankruptDays}/{MAX_BANKRUPT_DAYS})");

            if (DataManager.Instance.CurrentData.bankruptDays >= MAX_BANKRUPT_DAYS)
            {
                TriggerBankruptcyPenalty();
            }
        }
        else
        {
            // 흑자 전환 시 파산 일수 초기화
            if (DataManager.Instance.CurrentData.bankruptDays > 0)
            {
                DataManager.Instance.CurrentData.bankruptDays = 0;
                Debug.Log("<color=green>[은행] 흑자 전환! 파산 위기에서 벗어났습니다.</color>");
            }
        }
    }

    private void TriggerBankruptcyPenalty()
    {
        Debug.Log("<color=red><b>[압류] 7일 연속 적자로 인해 강제 압류 및 평판 하락 패널티가 발생합니다!</b></color>");
        
        // 평판 대폭 하락
        if (ReputationManager.Instance != null)
        {
            // 임시로 하락 로직 적용 (ReputationManager에 AddReputation(-50) 같은 함수가 있다면 사용)
            // 여기선 간단히 0으로 떨어뜨리거나 구현체에 맞게 조정
            ReputationManager.Instance.OnCustomerLeft(); // 여러 번 호출하거나 별도 패널티 함수 추가 필요
        }

        // 빚의 일부를 강제 청산하기 위해 인벤토리 몰수 등 극한의 패널티 가능
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAllIngredients(); // 예: 재료 전량 압류
        }

        // 파산 일수는 다시 0으로 (또는 게임 오버 처리)
        DataManager.Instance.CurrentData.bankruptDays = 0;
    }
}
