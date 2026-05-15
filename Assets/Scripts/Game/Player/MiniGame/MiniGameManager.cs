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
    public GameObject rapidTapUI; // 으깨기(Mash) 게임 UI
    public GameObject timingUI;   // 젓기(Stir) 게임 UI
    public GameObject sliceUI;    // 자르기(Cut) 게임 UI
    public GameObject grillUI;    // 굽기(Bake/Fry) 게임 UI

    // 현재 진행 중인 미니게임의 난이도 완화 보너스 (장비에서 전달)
    public float CurrentEaseBonus { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── 기존 API (CookingPot 등에서 호출하던 문자열 기반) ─

    /// <summary>
    /// [Deprecated] 문자열로 미니게임을 시작합니다. 신규 코드에서는 MiniGameType 오버로드를 사용하세요.
    /// </summary>
    public void StartMiniGame(string gameType)
    {
        if (!Enum.TryParse<MiniGameType>(gameType, out var type))
        {
            Debug.LogWarning($"[MiniGame] Unknown game type '{gameType}'. Defaulting to None.");
            type = MiniGameType.None;
        }
        StartMiniGame(type, 0f);
    }

    // ─── 신규 API (ProcessManager에서 호출) ────────────────

    /// <summary>
    /// ProcessType과 장비의 easeBonus를 받아 적절한 미니게임을 시작합니다.
    /// easeBonus가 클수록 성공 판정이 쉬워집니다 (각 미니게임 스크립트가 이 값을 읽어 적용).
    /// </summary>
    public void StartMiniGame(MiniGameType gameType, float easeBonus = 0f)
    {
        CurrentEaseBonus = easeBonus;
        Debug.Log($"[MiniGame] {gameType} 시작! (장비 완화 보너스: {easeBonus:P0})");

        HideAll();
        switch (gameType)
        {
            case MiniGameType.Mash:
                if (rapidTapUI != null) rapidTapUI.SetActive(true);
                break;
            case MiniGameType.Stir:
                if (timingUI != null) timingUI.SetActive(true);
                break;
            case MiniGameType.Slice:
                if (sliceUI != null) sliceUI.SetActive(true);
                break;
            case MiniGameType.Grill:
                if (grillUI != null) grillUI.SetActive(true);
                break;
            case MiniGameType.None:
            default:
                // 미니게임 없음 – 바로 기본 성공으로 처리
                ReportResult(new MiniGameResult { isSuccess = true, qualityScore = 0.5f });
                break;
        }
    }

    /// <summary>
    /// 미니게임 스크립트에서 완료 시 호출합니다.
    /// </summary>
    public void ReportResult(MiniGameResult result)
    {
        HideAll();
        Debug.Log($"[MiniGame] 종료! 성공: {result.isSuccess}, 점수: {result.qualityScore:P0}");
        OnMiniGameFinished?.Invoke(result);
    }

    private void HideAll()
    {
        if (rapidTapUI != null) rapidTapUI.SetActive(false);
        if (timingUI   != null) timingUI.SetActive(false);
        if (sliceUI    != null) sliceUI.SetActive(false);
        if (grillUI    != null) grillUI.SetActive(false);
    }
}