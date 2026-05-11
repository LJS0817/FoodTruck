using System.Collections;
using UnityEngine;

public class AutoCookManager : MonoBehaviour
{
    public static AutoCookManager Instance { get; private set; }

    private bool isCooking = false; // 현재 요리 중인지 체크하는 플래그

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 주문이 들어오거나, 요리가 하나 끝났을 때 호출하여 
    // 밀린 주문이 있는지 확인하고 조리를 시작하는 트리거 함수입니다.
    public void ProcessAutoOrder()
    {
        // 트럭 안에 있거나, 이미 조리 중이라면 무시합니다.
        if (ViewManager.Instance.isInsideTruck || isCooking) return;

        // OrderManager에서 가장 먼저 들어온 대기 주문을 가져옵니다.
        OrderData nextOrder = OrderManager.Instance.GetFirstActiveOrder();

        if (nextOrder != null)
        {
            // 💡 인벤토리에 요리에 필요한 재료가 충분한지 먼저 확인합니다.
            if (InventoryManager.Instance.HasIngredients(nextOrder.orderedFood.requiredIngredients))
            {
                StartCoroutine(AutoCookRoutine(nextOrder.owner, nextOrder.orderedFood));
            }
            else
            {
                // 재료가 없으면 조리를 시작하지 못하므로, 다음 주문을 시도하거나 대기 상태를 유지합니다.
                // (일단 무시하고 아무것도 안 하거나, 실패 피드백을 줄 수 있습니다.)
                Debug.Log($"<color=orange>[자동 요리 대기] {nextOrder.orderedFood.foodName} 요리에 필요한 재료가 부족합니다.</color>");
            }
        }
    }

    private IEnumerator AutoCookRoutine(CustomerController customer, FoodData orderedFood)
    {
        isCooking = true; // 요리가 시작되었으므로 상태를 잠금

        // 💡 본격적인 요리가 시작될 때 WorkOnOrder 호출 (조건 충족)
        customer.WorkOnOrder();

        float timer = 0f;
        float targetTime = orderedFood.autoCookTime;

        // 💡 알바생 보조 셰프 등 자동 요리 속도 상승 버프 적용
        if (WorkerManager.Instance != null)
        {
            float speedBoost = WorkerManager.Instance.GetAbilityTotalValue(WorkerAbility.AutoCookSpeedUp);
            targetTime *= Mathf.Max(0.1f, 1f - speedBoost); // ex: 20% 단축이면 0.8 곱함
        }

        while (timer < targetTime)
        {
            // 유저가 도중에 트럭 안으로 들어가면 자동 요리 중지
            if (ViewManager.Instance.isInsideTruck)
            {
                Debug.Log("<color=orange>유저가 트럭 안으로 들어와 자동 요리가 취소되었습니다.</color>");
                isCooking = false;
                yield break;
            }

            // 손님이 중간에 화가 나서 나가버렸다면 중지
            if (customer.currentPatience <= 0)
            {
                isCooking = false;

                // 손님이 떠나면 OrderManager 쪽에서 해당 주문표가 제거되었을 것이므로, 
                // 즉시 다음 주문이 있는지 확인하고 넘어갑니다.
                ProcessAutoOrder();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 💡 시간이 다 차면 실제로 인벤토리에서 재료를 일괄 차감하고 요리 생성
        InventoryManager.Instance.ConsumeIngredients(orderedFood.requiredIngredients);

        Dish autoDish = new Dish();
        autoDish.Initialize(orderedFood, false, 1.0f);

        Debug.Log($"<color=green>[자동 요리 완성] {orderedFood.foodName} 자동 서빙 완료!</color>");
        customer.ReceiveDish(autoDish);

        // 주문 완료 처리 (이 시점에 OrderManager의 activeOrders에서 해당 주문이 빠져나감)
        OrderManager.Instance.CompleteOrderOf(customer);

        isCooking = false; // 요리 끝!

        // 💡 요리가 끝났으므로 대기 중인 다음 요리가 있는지 확인하고 즉시 시작 (조건 충족)
        ProcessAutoOrder();
    }
}