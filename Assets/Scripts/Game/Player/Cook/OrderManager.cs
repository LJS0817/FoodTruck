using System.Collections.Generic;
using UnityEngine;

// 💡 수정 1: FoodData 대신 string을 저장하여 커스텀 요리 주문도 처리 가능하게 변경
[System.Serializable]
public class OrderData
{
    public CustomerController owner;
    public string orderedFoodName;
}

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Order Settings")]
    public int maxOrders = 4;
    public OrderTicketObject ticketPrefab;
    public Transform ticketRack;
    public float ticketSpacing = 1.2f;

    private List<OrderData> activeOrders = new List<OrderData>(4);
    private List<OrderTicketObject> visualTickets = new List<OrderTicketObject>(4);

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 💡 수정 2: FoodData 객체 대신 요리 이름(string)을 매개변수로 받음
    public bool TryAddOrder(CustomerController customer, string foodName)
    {
        if (activeOrders.Count >= maxOrders)
        {
            return false;
        }

        // 데이터 생성 및 리스트 추가
        OrderData newOrder = new OrderData { owner = customer, orderedFoodName = foodName };
        activeOrders.Add(newOrder);

        SpawnTicketVisual(newOrder);
        Debug.Log($"[주문 접수] {foodName} 주문표가 추가되었습니다! (현재 {activeOrders.Count}/{maxOrders})");
        return true;
    }

    private void SpawnTicketVisual(OrderData orderData)
    {
        OrderTicketObject ticket = Instantiate(ticketPrefab, ticketRack);
        ticket.SetupTicket(orderData);
        visualTickets.Add(ticket);

        UpdateTicketPositions();
    }

    public void RemoveOrder(OrderData orderData, OrderTicketObject ticketObject)
    {
        activeOrders.Remove(orderData);
        visualTickets.Remove(ticketObject);

        if (ticketObject != null)
        {
            Destroy(ticketObject.gameObject);
        }

        UpdateTicketPositions();
    }

    private void UpdateTicketPositions()
    {
        for (int i = 0; i < visualTickets.Count; i++)
        {
            // 런타임 할당을 최소화하기 위해 Vector3 캐싱 고려 가능
            visualTickets[i].transform.localPosition = new Vector3(i * ticketSpacing, 0, 0);
        }
    }

    public void CancelOrderOf(CustomerController customer)
    {
        // 성능 최적화를 위해 LINQ 대신 for문 유지
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].owner == customer)
            {
                OrderData targetOrder = activeOrders[i];
                OrderTicketObject targetTicket = visualTickets[i];

                RemoveOrder(targetOrder, targetTicket);

                Debug.Log($"<color=orange>[주문 취소] {customer.currentData.customerName}이(가) 떠나서 주문표가 폐기되었습니다.</color>");
                return;
            }
        }
    }
}