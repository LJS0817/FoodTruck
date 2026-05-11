using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettlementUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settlementPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text grossSalesText;      // 총 매출
    [SerializeField] private TMP_Text expensesText;        // 총 지출 (재료비)
    [SerializeField] private TMP_Text netProfitText;       // 순이익 (매출 - 지출)
    [SerializeField] private TMP_Text customerCountText;   // 만족한 손님 수
    [SerializeField] private TMP_Text premiumCountText;    // 프리미엄 요리 개수
    [SerializeField] private TMP_Text topMenuText;         // 오늘의 인기 메뉴
    [SerializeField] private TMP_Text reviewText;          // 💡 오늘의 가상 SNS 리뷰 텍스트 (신규 추가, Inspector 연결 필요)

    [Header("Buttons")]
    [SerializeField] private Button nextDayButton;

    private void Start()
    {
        if (settlementPanel != null) settlementPanel.SetActive(false);

        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(OnClickNextDay);
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(DayPhase phase)
    {
        if (phase == DayPhase.Settlement)
            OpenSettlement();
        else
            CloseSettlement();
    }

    private void OpenSettlement()
    {
        if (SettlementManager.Instance == null) return;

        int day         = GameTimeManager.Instance != null ? GameTimeManager.Instance.GetCurrentDay() : 0;
        int grossSales  = SettlementManager.Instance.GetDailySales();
        int expenses    = SettlementManager.Instance.GetDailyExpenses();
        int netProfit   = SettlementManager.Instance.GetNetProfit();
        int customers   = SettlementManager.Instance.GetSatisfiedCustomerCount();
        int premiums    = SettlementManager.Instance.GetPremiumDishCount();
        int lostCount   = SettlementManager.Instance.GetLostCustomerCount();
        string topMenu  = SettlementManager.Instance.GetTopMenu();

        if (dayText != null)          dayText.text          = $"Day {day} 결산";
        if (grossSalesText != null)   grossSalesText.text   = $"{grossSales:N0}원";
        if (expensesText != null)     expensesText.text     = $"- {expenses:N0}원";
        if (netProfitText != null)
        {
            netProfitText.text  = $"{netProfit:N0}원";
            // 순이익이 적자이면 빨간색, 흑자이면 초록색으로 표시
            netProfitText.color = netProfit >= 0 ? Color.green : Color.red;
        }
        if (customerCountText != null) customerCountText.text = $"{customers}명";
        if (premiumCountText != null)  premiumCountText.text  = $"{premiums}개";
        if (topMenuText != null)       topMenuText.text       = topMenu;

        // 💡 리뷰 시스템 연동 (ReviewManager)
        if (reviewText != null)
        {
            var reviews = ReviewManager.GenerateDailyReviews(customers, lostCount, premiums, topMenu);
            reviewText.text = "<b>< 오늘의 SNS 후기 ></b>\n\n";
            foreach (var r in reviews)
            {
                reviewText.text += $"💬 \"{r}\"\n";
            }
        }

        settlementPanel.SetActive(true);

        // 정산 중 게임 시간 정지
        Time.timeScale = 0f;
    }

    private void CloseSettlement()
    {
        settlementPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClickNextDay()
    {
        Debug.Log("[SettlementUI] 다음 날로 넘어갑니다.");
        DayCycleManager.Instance.StartNextDay();
        CloseSettlement();
    }
}
