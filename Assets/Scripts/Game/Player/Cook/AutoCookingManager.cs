using System.Collections;
using UnityEngine;

public class AutoCookManager : MonoBehaviour
{
    public static AutoCookManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 💡 수정 4: FoodData 객체 대신 이름(string)과 자동 조리 시간(float)을 받도록 수정
    public void ProcessAutoOrder(CustomerController customer, string orderedFoodName, float targetTime)
    {
        if (ViewManager.Instance.isInsideTruck) return;

        StartCoroutine(AutoCookRoutine(customer, orderedFoodName, targetTime));
    }

    private IEnumerator AutoCookRoutine(CustomerController customer, string orderedFoodName, float targetTime)
    {
        float timer = 0f;

        while (timer < targetTime)
        {
            if (ViewManager.Instance.isInsideTruck)
            {
                Debug.Log("<color=orange>유저가 트럭 안으로 들어와 자동 요리가 취소되었습니다.</color>");
                yield break;
            }

            if (customer.currentPatience <= 0) yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        Dish autoDish = new Dish();
        // 💡 수정 5: 변경된 Dish 초기화 방식 (이름으로 초기화)
        autoDish.Initialize(orderedFoodName, false, 1.0f);

        Debug.Log($"<color=green>[자동 요리 완성] {orderedFoodName} 자동 서빙 완료!</color>");
        customer.ReceiveDish(autoDish);
    }
}