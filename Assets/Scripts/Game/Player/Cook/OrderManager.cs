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
    public OrderTicketObject ticketPrefab;
    public Transform ticketRack; // 주문표가 걸릴 부모 Transform
    public float ticketSpacing = 1.2f;

    // 현재 활성화된 주문 리스트
    private List<OrderData> activeOrders = new List<OrderData>(4);
    private List<OrderTicketObject> visualTickets = new List<OrderTicketObject>(4);

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

        SpawnTicketVisual(newOrder);
        AutoCookManager.Instance.ProcessAutoOrder(customer, food);

        Debug.Log($"[주문 접수] {food.foodName} 주문표가 추가되었습니다! (현재 {activeOrders.Count}/{maxOrders})");
        return true;
    }

    private void SpawnTicketVisual(OrderData orderData)
    {
        // (최적화를 위해 추후 Object Pool 연동 권장)
        OrderTicketObject ticket = Instantiate(ticketPrefab, ticketRack);
        ticket.SetupTicket(orderData);
        visualTickets.Add(ticket);

        UpdateTicketPositions();
    }

    public void RemoveOrder(OrderData orderData, OrderTicketObject ticketObject)
    {
        activeOrders.Remove(orderData);
        visualTickets.Remove(ticketObject);
        Destroy(ticketObject.gameObject); // (풀링 사용 시 OnDespawn 처리)

        UpdateTicketPositions();
    }

    // 주문표가 빠지면 남은 주문표들을 옆으로 밀어주는 시각적 정렬 로직
    private void UpdateTicketPositions()
    {
        for (int i = 0; i < visualTickets.Count; i++)
        {
            Vector3 newPos = new Vector3(i * ticketSpacing, 0, 0);
            visualTickets[i].transform.localPosition = newPos;
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
                OrderTicketObject targetTicket = visualTickets[i];

                // 기존에 만들어둔 RemoveOrder 함수 재사용
                RemoveOrder(targetOrder, targetTicket);

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
                OrderTicketObject targetTicket = visualTickets[i];

                // 주문 제거 로직 실행
                RemoveOrder(targetOrder, targetTicket);

                Debug.Log($"<color=cyan>[주문 완료] {customer.currentData.customerName}의 요리가 전달되어 주문표를 제거합니다.</color>");
                return;
            }
        }
    }
}