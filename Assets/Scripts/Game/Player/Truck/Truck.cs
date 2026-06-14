using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Truck : MonoBehaviour, IInteractable
{
    [Header("트럭 데이터")]
    [Tooltip("트럭의 정보와 스펙, 레이아웃을 담고 있는 ScriptableObject")]
    public TruckData truckData;

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        ViewManager.Instance.GoInside();
        
        // 트럭 내부 내비게이션 매니저에 현재 트럭 정보를 연동
        if (TruckInsideNavigation.Instance != null && truckData != null)
        {
            TruckInsideNavigation.Instance.truckData = this.truckData;
        }
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }
}