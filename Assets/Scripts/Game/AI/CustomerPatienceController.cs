using System.Collections.Generic;
using UnityEngine;

public class CustomerPatienceController : MonoBehaviour
{
    [Header("UI Settings")]
    public CustomerPatienceUI uiPrefab;
    public Transform uiContainer;

    /// <summary>
    /// 주문 접수 시 호출: 실시간으로 UI를 Instantiate하고 활성화
    /// </summary>
    public bool OnOrderReceived(CustomerController customer, FoodData orderedFood)
    {
        // 💡 실시간으로 UI 생성 (풀링 제거)
        CustomerPatienceUI ui = Instantiate(uiPrefab, uiContainer);

        // UI 설정 및 활성화
        ui.Setup(orderedFood, customer);
        ui.ShowUI();

        return customer.TrySetPatience(ui);
    }
}
