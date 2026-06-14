using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Truck : MonoBehaviour, IInteractable
{
    [Header("Inside Layout")]
    [Tooltip("트럭 내부의 장비 배치 순서. (null이면 카운터, EquipmentData면 해당 장비)")]
    public EquipmentData[] stationLayout;

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        ViewManager.Instance.GoInside();
        
        // 트럭 내부 내비게이션 매니저에 현재 트럭 정보를 연동
        if (TruckInsideNavigation.Instance != null)
        {
            TruckInsideNavigation.Instance.truckData = this;
        }
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }
}