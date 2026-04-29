using System;
using UnityEngine;

public struct MiniGameResult
{
    public bool isSuccess;      // 성공 여부
    public float qualityScore;  // 0.0 ~ 1.0 점수 (1.0이면 프리미엄 확정)
}

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    // 게임 완료 시 호출될 이벤트 (결과값 전달)
    public event Action<MiniGameResult> OnMiniGameFinished;

    [Header("Mini Game UI/Objects")]
    public GameObject rapidTapUI; // 으깨기 게임 UI/오브젝트
    public GameObject timingUI;   // 젓기 게임 UI/오브젝트

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. 미니게임 시작 호출 (외부에서 사용)
    public void StartMiniGame(string gameType)
    {
        Debug.Log($"[MiniGame] {gameType} 시작!");

        // 현재는 간단히 활성화만 하지만, 나중에 각 게임 전용 스크립트의 Init 호출
        if (gameType == "Mash") rapidTapUI.SetActive(true);
        else if (gameType == "Stir") timingUI.SetActive(true);
    }

    // 2. 게임 종료 및 결과 보고 (각 미니게임 스크립트에서 호출)
    public void ReportResult(MiniGameResult result)
    {
        rapidTapUI.SetActive(false);
        timingUI.SetActive(false);

        Debug.Log($"[MiniGame] 종료! 성공: {result.isSuccess}, 점수: {result.qualityScore}");
        OnMiniGameFinished?.Invoke(result);
    }
}