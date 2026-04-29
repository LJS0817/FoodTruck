using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TimingGame : MonoBehaviour
{
    [Header("UI Objects")]
    public RectTransform needle;      // 움직이는 바늘
    public RectTransform targetZone;  // 목표 영역

    [Header("Settings")]
    public float moveSpeed = 500f;    // 바늘 속도
    private int direction = 1;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += CheckTiming;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= CheckTiming;
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        // 바늘 왕복 운동 (가로 방향 예시)
        needle.anchoredPosition += Vector2.right * direction * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(needle.anchoredPosition.x) > 200f) direction *= -1;
    }

    private void CheckTiming(Finger finger)
    {
        // 바늘과 목표 영역 사이의 거리 계산
        float dist = Mathf.Abs(needle.anchoredPosition.x - targetZone.anchoredPosition.x);
        bool success = dist < 30f; // 30픽셀 이내면 성공

        MiniGameResult result = new MiniGameResult
        {
            isSuccess = success,
            qualityScore = success ? 1.0f : 0.5f
        };
        MiniGameManager.Instance.ReportResult(result);
    }
}