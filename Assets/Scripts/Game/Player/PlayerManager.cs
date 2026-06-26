using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public event Action<int> OnMoneyChanged;

    private int currentMoney;
    public int CurrentMoney => currentMoney;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 1. DataManager의 JSON 데이터에서 초기 재화 불러오기
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            currentMoney = GameManager.Instance.dataManager.CurrentData.currentMoney;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) AddMoney(1000);       
    }

    // 수익 획득
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        Debug.Log($"<color=yellow>[수익] {amount}원 획득! 현재 잔액: {currentMoney}원</color>");
        UpdateDataAndUI();
    }

    // 재화 지출 (상점 구매, 알바생 일당 등)
    public bool SpendMoney(int amount)
    {
        if (amount < 0 || currentMoney < amount) return false;

        if (amount == 0) return true; // 0원 지출은 차감 없이 바로 성공 처리

        currentMoney -= amount;
        Debug.Log($"<color=orange>[지출] {amount}원 사용. 현재 잔액: {currentMoney}원</color>");
        UpdateDataAndUI();
        return true; // 지출 성공
    }

    public bool CheckMoney(int amount)
    {
        return currentMoney >= amount;
    }

    private void UpdateDataAndUI()
    {
        // 1. DataManager에 현재 값 전달 
        // (디스크 I/O 과부하를 막기 위해 Save는 하루 영업 종료 시에만 한 번 호출하는 것을 권장합니다)
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.CurrentData.currentMoney = currentMoney;
        }

        // 2. 이벤트 구독 중인 UI들에게 즉시 알림
        OnMoneyChanged?.Invoke(currentMoney);
    }
}