using UnityEngine;

// 모든 터치 가능한 2D 오브젝트는 이 인터페이스를 상속받습니다.
public interface IInteractable
{
    // 터치가 시작되었을 때 (Pointer Down)
    IInteractable OnTouchBegin(Vector2 touchPosition);

    // 터치한 상태로 드래그할 때 (Pointer Drag)
    void OnTouchDrag(Vector2 touchPosition);

    // 터치가 끝났을 때 (Pointer Up)
    void OnTouchEnd();
}