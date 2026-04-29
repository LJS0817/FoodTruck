using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Truck : MonoBehaviour, IInteractable
{
    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        // 트럭 오브젝트를 터치하면 내부 조리 화면(Canvas)을 켭니다.
        ViewManager.Instance.GoInside();
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }
}