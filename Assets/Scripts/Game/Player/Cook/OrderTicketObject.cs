using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderTicketObject : MonoBehaviour, IInteractable
{
    public OrderData currentOrder;
    // public SpriteRenderer foodIconRenderer; // 주문표에 그려질 음식 아이콘

    public void SetupTicket(OrderData orderData)
    {
        this.currentOrder = orderData;
        // foodIconRenderer.sprite = orderData.orderedFood.foodSprite; // 시각적 세팅
    }

    // 주문표를 터치했을 때 실행 (서빙 시도)
    public void OnTouchBegin()
    {
        // 1. 조리대(CookingManager)에 완성된 요리가 있는지 확인
        Dish completedDish = CookingManager.Instance.GetCompletedDish();

        if (completedDish != null)
        {
            // 2. 완성된 요리가 이 주문표의 요리와 일치하는지 확인
            if (completedDish.foodData == currentOrder.orderedFood)
            {
                Debug.Log($"<color=cyan>[서빙 성공] {currentOrder.orderedFood.foodName}을(를) 손님에게 전달합니다!</color>");

                // 손님에게 요리 전달
                currentOrder.owner.ReceiveDish(completedDish);

                // 냄비 비우기 및 주문표 제거
                CookingManager.Instance.ClearDish();
                OrderManager.Instance.RemoveOrder(currentOrder, this);
            }
            else
            {
                Debug.LogWarning("<color=orange>이 주문표에 맞는 요리가 아닙니다!</color>");
            }
        }
        else
        {
            Debug.Log("<color=yellow>아직 완성된 요리가 없습니다.</color>");
        }
    }

    public void OnTouchDrag(Vector2 touchPosition) { }
    public void OnTouchEnd() { }
}