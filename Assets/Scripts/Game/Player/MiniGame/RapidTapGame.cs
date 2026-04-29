using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class RapidTapGame : MonoBehaviour
{
    [Header("Settings")]
    public int goalTapCount = 20;   // 목표 터치 횟수
    public float timeLimit = 3f;    // 제한 시간

    private int currentTapCount = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += CountTap;

        // 초기화 및 시작
        currentTapCount = 0;
        timer = timeLimit;
        isPlaying = true;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= CountTap;
        EnhancedTouchSupport.Disable();
    }

    private void CountTap(Finger finger)
    {
        if (!isPlaying) return;
        currentTapCount++;

        // 시각적 피드백 (예: 재료가 점점 으깨지는 연출) 호출 가능

        if (currentTapCount >= goalTapCount) Finish(true);
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0) Finish(false);
    }

    private void Finish(bool success)
    {
        isPlaying = false;
        MiniGameResult result = new MiniGameResult
        {
            isSuccess = success,
            qualityScore = success ? 1.0f : (float)currentTapCount / goalTapCount
        };
        MiniGameManager.Instance.ReportResult(result);
    }
}