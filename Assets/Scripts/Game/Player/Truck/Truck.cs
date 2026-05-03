using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Truck : MonoBehaviour, IInteractable
{
    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        ViewManager.Instance.GoInside();
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }
}