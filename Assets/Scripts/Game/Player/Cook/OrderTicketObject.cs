using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderTicketObject : MonoBehaviour, IInteractable
{
    public OrderData currentOrder; // 💡 (참고) OrderData 내부도 string orderedFoodName 을 갖도록 수정하셔야 합니다.

    public void SetupTicket(OrderData orderData)
    {
        this.currentOrder = orderData;
        // foodIconRenderer.sprite = orderData.orderedFood.foodSprite; // 시각적 세팅
    }

    public void OnTouchBegin()
    {
        Dish completedDish = CookingManager.Instance.GetCompletedDish();

        if (completedDish != null)
        {
            // 💡 수정 6: 이름(string) 기반으로 주문표와 완성된 요리가 일치하는지 검사
            if (completedDish.foodName == currentOrder.orderedFoodName)
            {
                Debug.Log($"<color=cyan>[서빙 성공] {currentOrder.orderedFoodName}을(를) 손님에게 전달합니다!</color>");

                currentOrder.owner.ReceiveDish(completedDish);

                CookingManager.Instance.ClearDish();
                OrderManager.Instance.RemoveOrder(currentOrder, this);
            }
            else
            {
                Debug.LogWarning($"<color=orange>이 주문표에 맞는 요리가 아닙니다! (주문: {currentOrder.orderedFoodName})</color>");
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