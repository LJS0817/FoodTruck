using System.Collections.Generic;
using UnityEngine;
using System;

public class OrderTicketController : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] OrderTicket _insideUIPrefab;
    [SerializeField] Transform _insideUIContainer;
    
    [SerializeField] OrderTicket _outsideUIPrefab;
    [SerializeField] Transform _outsideUIContainer;

    private List<OrderTicket> _insideTicketPool;
    private bool[] _insideTicketUsed;
    private Queue<OrderTicket> _outsideTicketPool;

    private void Start()
    {
        int max = OrderManager.Instance.maxOrders;
        _insideTicketPool = new List<OrderTicket>(max);
        _insideTicketUsed = new bool[max];
        _outsideTicketPool = new Queue<OrderTicket>(max);

        // 1. Inside Ticket은 순서와 무관하게 미리 생성 (고정 슬롯 개념)
        for (int i = 0; i < max; i++)
        {
            OrderTicket insideTicket = Instantiate(_insideUIPrefab, _insideUIContainer);
            insideTicket.ticketType = OrderTicket.TicketType.Inside; // 타입 지정
            insideTicket.gameObject.SetActive(false);
            _insideTicketPool.Add(insideTicket);
            _insideTicketUsed[i] = false;
        }

        // 2. Outside Ticket은 순서가 중요하므로 미리 만들어두고 Queue로 관리
        for (int i = 0; i < max; i++)
        {
            OrderTicket outsideTicket = Instantiate(_outsideUIPrefab, _outsideUIContainer);
            outsideTicket.ticketType = OrderTicket.TicketType.Outside; // 타입 지정
            outsideTicket.gameObject.SetActive(false);
            _outsideTicketPool.Enqueue(outsideTicket);
        }
    }

    public (OrderTicket, OrderTicket) CreateOrderTicket(OrderData orderData)
    {
        // --- 1. Inside Ticket 재사용 ---
        OrderTicket ticketInside = null;
        for (int i = 0; i < _insideTicketPool.Count; i++)
        {
            if (!_insideTicketUsed[i])
            {
                _insideTicketUsed[i] = true;
                ticketInside = _insideTicketPool[i];
                break;
            }
        }
        
        // 예외 처리 (혹시 풀이 부족할 경우)
        if (ticketInside == null)
        {
            ticketInside = Instantiate(_insideUIPrefab, _insideUIContainer);
            ticketInside.ticketType = OrderTicket.TicketType.Inside; // 타입 지정 추가
            _insideTicketPool.Add(ticketInside);
            // 배열 크기를 늘리기보다는 예외 상황이므로 단순히 풀에만 추가함
        }

        ticketInside.gameObject.SetActive(true);
        ticketInside.SetupTicket(orderData);
        ticketInside.ShowUI();


        // --- 2. Outside Ticket 재사용 (Queue 기반 풀링, 자식 순서 정렬) ---
        OrderTicket ticketOutside = null;
        if (_outsideTicketPool.Count > 0)
        {
            ticketOutside = _outsideTicketPool.Dequeue();
        }
        else
        {
            ticketOutside = Instantiate(_outsideUIPrefab, _outsideUIContainer);
            ticketOutside.ticketType = OrderTicket.TicketType.Outside; // 타입 지정 추가
        }

        ticketOutside.gameObject.SetActive(true);
        // Vertical Layout Group 자동 정렬을 위해 맨 아래(마지막 자식)로 보냄
        ticketOutside.transform.SetAsLastSibling(); 
        ticketOutside.SetupTicket(orderData);
        ticketOutside.ShowUI();

        return (ticketInside, ticketOutside);
    }

    // OrderManager에서 주문 완료/취소 시 호출
    public void ReturnTickets((OrderTicket, OrderTicket) ticketObject)
    {
        if (ticketObject.Item1 != null)
        {
            ticketObject.Item1.HideUI();
            int index = _insideTicketPool.IndexOf(ticketObject.Item1);
            if (index >= 0 && index < _insideTicketUsed.Length)
            {
                _insideTicketUsed[index] = false;
            }
        }

        if (ticketObject.Item2 != null)
        {
            ticketObject.Item2.HideUI();
            _outsideTicketPool.Enqueue(ticketObject.Item2);
        }
    }
}
