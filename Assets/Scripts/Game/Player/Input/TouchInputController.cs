using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchInputController : MonoBehaviour
{
    private IInteractable currentTarget;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // EnhancedTouch 시스템 활성화 (필수)
        EnhancedTouchSupport.Enable();

        // 터치 이벤트 구독
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 및 시스템 비활성화 (메모리 누수 방지)
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;

        EnhancedTouchSupport.Disable();
    }

    // 1. 화면에 손가락이 닿았을 때
    private void OnFingerDown(Finger finger)
    {
        // 이미 조작 중인 객체가 있다면 다른 손가락의 터치는 무시 (단일 조작 기준)
        if (currentTarget != null) return;

        // 💡 UI 위의 터치라면 월드 오브젝트 조작을 차단
        if (IsPointerOverUI(finger)) return;

        // 터치된 스크린 좌표를 월드 좌표로 변환
        Vector2 touchPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);

        // 가비지 할당이 없는 Physics2D.Raycast 사용
        RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);

        if (hit.collider != null)
        {
            currentTarget = hit.collider.GetComponent<IInteractable>()?.OnTouchBegin(touchPos);
        }
    }

    // 💡 GC 방지를 위한 재사용 리스트
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    /// <summary>
    /// 현재 터치가 UI 요소 위에 있는지 확인합니다.
    /// EventSystem.RaycastAll을 사용하여 IsPointerOverGameObject보다 정확하게 판별합니다.
    /// </summary>
    private bool IsPointerOverUI(Finger finger)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = finger.screenPosition;
        
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _raycastResults);
        
        return _raycastResults.Count > 0;
    }

    // 2. 손가락을 화면에 대고 움직일 때
    private void OnFingerMove(Finger finger)
    {
        if (currentTarget != null)
        {
            Vector2 touchPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
            currentTarget.OnTouchDrag(touchPos);
        }
    }

    // 3. 화면에서 손가락을 떼었을 때
    private void OnFingerUp(Finger finger)
    {
        if (currentTarget != null)
        {
            currentTarget.OnTouchEnd();
            currentTarget = null; // 타겟 초기화
        }
    }
}