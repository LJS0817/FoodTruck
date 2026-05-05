using UnityEngine;
using TMPro;

public class BusinessManager : MonoBehaviour
{
    public static BusinessManager Instance { get; private set; }

    [Header("Business State")]
    public bool IsBusinessOpen { get; private set; } = false;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text toggleButtonText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 장사는 종료된 상태로 시작
        IsBusinessOpen = false;
        GameTimeManager.Instance.timeScaleMultiplier = 0.5f;
        UpdateButtonUI();
    }

    /// <summary>
    /// 장사 시작/종료 버튼 OnClick 이벤트에 연결될 메서드
    /// </summary>
    public void ToggleBusiness()
    {
        IsBusinessOpen = !IsBusinessOpen;
        
        UpdateButtonUI();

        if (IsBusinessOpen)
        {
            Debug.Log("<color=green>[BusinessManager] 셔터 개방! 장사를 시작합니다. 손님들이 오기 시작합니다.</color>");
            // 시간 정상화 (1배속)
            if (GameTimeManager.Instance != null)
                GameTimeManager.Instance.timeScaleMultiplier = 1f;
        }
        else
        {
            Debug.Log("<color=orange>[BusinessManager] 셔터 닫힘! 장사를 종료합니다. 손님들이 돌아갑니다.</color>");
            
            // 1. 남은 손님 모두 강제로 돌려보내기
            if (CustomerManager.Instance != null)
                CustomerManager.Instance.ForceLeaveAllCustomers();
            
            // 2. 모든 주문 폐기
            if (OrderManager.Instance != null)
                OrderManager.Instance.ClearAllOrders();
            
            // 3. 조리 중이던 냄비 비우기 (재료 반환 없음)
            if (CookingManager.Instance != null && CookingManager.Instance.currentPot != null)
                CookingManager.Instance.currentPot.ResetPot();

            // 4. 조리대에 이미 완성된 요리가 있다면 폐기
            if (CookingManager.Instance != null)
                CookingManager.Instance.ClearDish();

            // 5. 시간 0.5배속으로 느리게 흐르도록 설정
            if (GameTimeManager.Instance != null)
                GameTimeManager.Instance.timeScaleMultiplier = 0.5f;
        }
    }

    private void UpdateButtonUI()
    {
        if (toggleButtonText != null)
        {
            // 장사 중이면 '장사 종료' 버튼으로, 종료 상태면 '장사 시작' 버튼으로 표시
            toggleButtonText.text = IsBusinessOpen ? "Close" : "Open";
        }
    }
}
