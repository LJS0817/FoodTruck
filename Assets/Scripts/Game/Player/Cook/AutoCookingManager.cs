using System.Collections;
using UnityEngine;

public class AutoCookManager : MonoBehaviour
{
    public static AutoCookManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // OrderManager나 CustomerWaitState에서 주문이 들어왔을 때 이 함수를 호출합니다.
    public void ProcessAutoOrder(CustomerController customer, FoodData orderedFood)
    {
        // 트럭 안에 있을 때는 자동 요리가 작동하지 않음 (수동으로 해야 함)
        if (ViewManager.Instance.isInsideTruck) return;

        // 트럭 밖에 있다면 자동 요리 코루틴 시작
        StartCoroutine(AutoCookRoutine(customer, orderedFood));
    }

    private IEnumerator AutoCookRoutine(CustomerController customer, FoodData orderedFood)
    {
        float timer = 0f;
        float targetTime = orderedFood.autoCookTime;

        // 💡 주의: 대기 중에 유저가 트럭 안으로 들어가거나(View 변경), 손님이 화나서 나가버리면 중단해야 함
        while (timer < targetTime)
        {
            // 유저가 도중에 트럭 안으로 들어가면 자동 요리 중지
            if (ViewManager.Instance.isInsideTruck)
            {
                Debug.Log("<color=orange>유저가 트럭 안으로 들어와 자동 요리가 취소되었습니다.</color>");
                yield break;
            }

            // 손님이 중간에 화가 나서 나가버렸다면 중지 (CustomerController에 bool isWaiting 변수 필요)
            if (customer.currentPatience <= 0) yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        // 시간이 다 차면 자동으로 요리(Dish)를 생성하여 서빙
        Dish autoDish = new Dish();
        // 자동 요리는 미니게임을 거치지 않으므로 프리미엄(false), 일반 퀄리티(1.0f)로 고정
        autoDish.Initialize(orderedFood, false, 1.0f);

        Debug.Log($"<color=green>[자동 요리 완성] {orderedFood.foodName} 자동 서빙 완료!</color>");
        customer.ReceiveDish(autoDish);

        // 💡 OrderManager의 주문표(Rack)에서도 해당 주문을 지워주는 로직 필요
    }
}