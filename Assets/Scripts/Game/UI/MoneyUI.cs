using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Animation Settings")]
    [SerializeField] private float countDuration = 0.5f; // 숫자 카운팅 연출 시간

    private int currentDisplayMoney = 0;
    private Coroutine countingCoroutine;

    private void OnEnable()
    {
        // 오브젝트 활성화 시 이벤트 구독
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnMoneyChanged += HandleMoneyChanged;

            // 이벤트 구독 시점의 초기값을 즉시 UI에 반영
            currentDisplayMoney = PlayerManager.Instance.CurrentMoney;
        }
        UpdateTextFast(currentDisplayMoney);
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 반드시 구독 해제 (메모리 누수 방지)
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
        }
    }

    // PlayerManager에서 OnMoneyChanged 이벤트가 발생할 때 호출됨
    private void HandleMoneyChanged(int targetMoney)
    {
        // 이미 숫자가 올라가고 있는 중이라면 이전 연출을 멈추고 새 목표값으로 재시작
        if (countingCoroutine != null)
        {
            StopCoroutine(countingCoroutine);
        }
        countingCoroutine = StartCoroutine(CountMoneyAnimation(targetMoney));
    }

    // 숫자가 부드럽게 변경되는 코루틴 애니메이션
    private IEnumerator CountMoneyAnimation(int targetMoney)
    {
        float elapsedTime = 0f;
        int startMoney = currentDisplayMoney;

        while (elapsedTime < countDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / countDuration;
            // Mathf.Lerp를 통해 시작 금액과 목표 금액 사이의 값을 보간
            currentDisplayMoney = (int)Mathf.Lerp(startMoney, targetMoney, t);

            UpdateTextFast(currentDisplayMoney);
            yield return null;
        }

        // 연출 종료 후 최종 목표값으로 정확히 보정
        currentDisplayMoney = targetMoney;
        UpdateTextFast(currentDisplayMoney);

        countingCoroutine = null;
    }

    // TextMeshPro 갱신 최적화
    private void UpdateTextFast(int moneyValue)
    {
        // ToString("N0")는 1,000 단위 콤마를 찍어줍니다.
        moneyText.text = moneyValue.ToString("N0");
        //moneyText.text = moneyValue.ToString("N0") + " 원";
    }
}