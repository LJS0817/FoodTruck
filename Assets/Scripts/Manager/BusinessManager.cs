using UnityEngine;
using TMPro;

public class BusinessManager : MonoBehaviour
{
    public static BusinessManager Instance { get; private set; }

    [Header("Business State")]
    public bool IsBusinessOpen { get; private set; } = false;

    [Header("UI Settings")]
    [SerializeField] GameObject _toggleButtonOutside;
    [SerializeField] GameObject _toggleButtonInside;
    TMP_Text _toggleButtonOutsideText;
    TMP_Text _toggleButtonInsideText;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _toggleButtonOutsideText = _toggleButtonOutside.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
        _toggleButtonInsideText = _toggleButtonInside.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        // 게임 시작 시 장사는 종료된 상태로 시작
        IsBusinessOpen = false;
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.timeScaleMultiplier = 0.5f;
        UpdateButtonUI();

        if (ViewManager.Instance != null)
        {
            ChangeBusinessButton(ViewManager.Instance.isInsideTruck);
        }

        // 💡 피로도 소진 시 자동 장사 종료 이벤트 구독
        if (PlayerStaminaManager.Instance != null)
            PlayerStaminaManager.Instance.OnStaminaDepleted += OnStaminaDepleted;
    }

    private void OnDestroy()
    {
        if (PlayerStaminaManager.Instance != null)
            PlayerStaminaManager.Instance.OnStaminaDepleted -= OnStaminaDepleted;
    }

    /// <summary>피로도 0 도달 시 자동 호출</summary>
    private void OnStaminaDepleted()
    {
        if (IsBusinessOpen)
        {
            Debug.Log("<color=red>[BusinessManager] 피로도 소진! 자동으로 장사를 종료합니다.</color>");
            ToggleBusiness();
        }
    }

    /// <summary>
    /// 장사 시작/종료 버튼 OnClick 이벤트에 연결될 메서드
    /// UI 버튼에서 호출할 때는 forceOpen=false 로 팝업을 거칩니다.
    /// </summary>
    public void ToggleBusiness(bool forceOpen = false)
    {
        if (!IsBusinessOpen && !forceOpen)
        {
            // 장사를 켜려는 시도인 경우, 바로 켜지 않고 팝업을 호출
            if (MenuSetupUI.Instance != null)
            {
                MenuSetupUI.Instance.OpenUI();
                return;
            }
            else
            {
                Debug.LogWarning("[BusinessManager] MenuSetupUI.Instance가 존재하지 않아 바로 장사를 시작합니다.");
            }
        }

        IsBusinessOpen = !IsBusinessOpen;
        
        UpdateButtonUI();

        if (IsBusinessOpen)
        {
            Debug.Log("<color=green>[BusinessManager] 셔터 개방! 장사를 시작합니다. 손님들이 오기 시작합니다.</color>");
            // 시간 정상화 (1배속)
            if (GameTimeManager.Instance != null)
                GameTimeManager.Instance.timeScaleMultiplier = 1f;

            // 💡 피로도 감소 시작
            PlayerStaminaManager.Instance?.StartDraining();
            
            // 페이즈 변경
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.ChangePhase(DayPhase.Business);
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

            // 5. 임시 바트에 남은 요리를 인벤토리로 환수 및 초기화
            if (IngredientManager.Instance != null)
                IngredientManager.Instance.ClearAllTempBoxes();

            // 5. 시간 0.5배속으로 느리게 흐르도록 설정
            if (GameTimeManager.Instance != null)
                GameTimeManager.Instance.timeScaleMultiplier = 0.5f;

            // 💡 피로도 감소 중지
            PlayerStaminaManager.Instance?.StopDraining();

            // 페이즈 변경
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.ChangePhase(DayPhase.Settlement);
        }
    }

    public void ChangeBusinessButton(bool isInside)
    {
        if (isInside)
        {
            _toggleButtonInside.SetActive(true);
            _toggleButtonOutside.SetActive(false);
        }
        else
        {
            _toggleButtonInside.SetActive(false);
            _toggleButtonOutside.SetActive(true);
        }
    }

    private void UpdateButtonUI()
    {
        if (_toggleButtonOutsideText != null)
        {
            // 장사 중이면 '장사 종료' 버튼으로, 종료 상태면 '장사 시작' 버튼으로 표시
            _toggleButtonOutsideText.text = IsBusinessOpen ? "Close" : "Open";
        }

        if (_toggleButtonInsideText != null)
        {
            // 장사 중이면 '장사 종료' 버튼으로, 종료 상태면 '장사 시작' 버튼으로 표시
            _toggleButtonInsideText.text = IsBusinessOpen ? "Close" : "Open";
        }
    }
}
