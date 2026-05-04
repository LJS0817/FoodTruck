using System.Collections.Generic;
using UnityEngine;

// 주문 정보를 담는 데이터 클래스
public class OrderData
{
    public CustomerController owner;
    public FoodData orderedFood;
}

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Order Settings")]
    public int maxOrders = 4; // 최대 받을 수 있는 주문표 개수
    [SerializeField]
    OrderTicketController _ticketController;

    // 현재 활성화된 주문 리스트
    private List<OrderData> activeOrders = new List<OrderData>(4);
    private List<OrderTicket> visualInsideTickets = new List<OrderTicket>(4);
    private List<OrderTicket> visualOutsideTickets = new List<OrderTicket>(4);


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 손님이 주문을 시도할 때 호출됨
    public bool TryAddOrder(CustomerController customer, FoodData food)
    {
        if (activeOrders.Count >= maxOrders)
        {
            return false; // 주문표 걸이가 꽉 차서 주문을 받을 수 없음
        }

        OrderData newOrder = new OrderData { owner = customer, orderedFood = food };
        activeOrders.Add(newOrder);

        bool result = customer.TrySetPatience(SpawnTicketVisual(newOrder));
        AutoCookManager.Instance.ProcessAutoOrder();
        Debug.Log($"[주문 접수] {food.foodName} 주문표가 추가되었습니다! (현재 {activeOrders.Count}/{maxOrders})");
        return result;
    }

    public OrderData GetFirstActiveOrder()
    {
        if (activeOrders.Count > 0)
        {
            return activeOrders[0];
        }
        return null;
    }

    private (OrderTicket, OrderTicket) SpawnTicketVisual(OrderData orderData)
    {
        // (최적화를 위해 추후 Object Pool 연동 권장)
        (OrderTicket, OrderTicket) result = _ticketController.CreateOrderTicket(orderData);
        visualInsideTickets.Add(result.Item1);
        visualOutsideTickets.Add(result.Item2);
        return result;
        // UpdateTicketPositions();
    }

    public void RemoveOrder(OrderData orderData, (OrderTicket, OrderTicket) ticketObject)
    {
        activeOrders.Remove(orderData);
        
        visualInsideTickets.Remove(ticketObject.Item1);
        visualOutsideTickets.Remove(ticketObject.Item2);
        
        // 💡 파괴 대신 풀로 반환
        _ticketController.ReturnTickets(ticketObject);

        // 💡 주문 삭제 시 인내심 UI 풀로 반환
        if (orderData.owner != null)
        {
            orderData.owner.RemoveOrder();
        }

    }

    public void CancelOrderOf(CustomerController customer)
    {
        // LINQ 대신 for문으로 해당 손님의 주문 데이터 탐색
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].owner == customer)
            {
                OrderData targetOrder = activeOrders[i];

                customer.RemoveOrder();

                // 기존에 만들어둔 RemoveOrder 함수 재사용
                RemoveOrder(targetOrder, (visualInsideTickets[i], visualOutsideTickets[i]));

                Debug.Log($"<color=orange>[주문 취소] {customer.currentData.customerName}이(가) 떠나서 주문표가 폐기되었습니다.</color>");
                return;
            }
        }
    }

    // 자동 요리가 완료되었을 때 호출 (주문표 제거)
    public void CompleteOrderOf(CustomerController customer)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].owner == customer)
            {
                OrderData targetOrder = activeOrders[i];

                customer.RemoveOrder();

                // 주문 제거 로직 실행
                RemoveOrder(targetOrder, (visualInsideTickets[i], visualOutsideTickets[i]));

                Debug.Log($"<color=cyan>[주문 완료] {customer.currentData.customerName}의 요리가 전달되어 주문표를 제거합니다.</color>");
                return;
            }
        }
    }

    // 💡 모든 주문 강제 취소 및 티켓 반환 (장사 종료 시)
    public void ClearAllOrders()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            OrderData order = activeOrders[i];
            if (order.owner != null)
            {
                order.owner.RemoveOrder();
            }
            _ticketController.ReturnTickets((visualInsideTickets[i], visualOutsideTickets[i]));
        }
        
        activeOrders.Clear();
        visualInsideTickets.Clear();
        visualOutsideTickets.Clear();
        Debug.Log("<color=orange>[주문 전체 폐기] 모든 주문이 폐기되었습니다.</color>");
    }
}