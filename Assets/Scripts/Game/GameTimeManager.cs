using UnityEngine;
using TMPro;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [SerializeField] private TMP_Text timeDisplayText;
    [SerializeField] private TMP_Text dateDisplayText;

    private const float TIME_MULTIPLIER = 96f;
    private float totalSeconds = 0f;
    private int currentDay;

    // 💡 최적화: 이전 프레임의 '분(Minute)'을 기억하여 바뀔 때만 텍스트를 갱신합니다.
    private int lastCalculatedMinute = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 💡 GameManager 등에서 데이터 로드 직후 호출해 줄 초기화 함수
    public void Initialize()
    {
        // 1. DataManager에서 저장된 날짜를 가져옵니다.
        currentDay = DataManager.Instance.CurrentData.currentDay;

        // 2. 게임 시작 시간 세팅 (예: 아침 8시 시작)
        SetTime(8, 0);

        // 3. 시작하자마자 UI 한번 갱신
        ForceUpdateUI();
    }

    void Update()
    {
        totalSeconds += Time.deltaTime * TIME_MULTIPLIER;

        // 하루(86,400초)가 지날 때마다 날짜 증가
        if (totalSeconds >= 86400f)
        {
            totalSeconds -= 86400f;
            currentDay++;

            // 💡 DataManager 연동: 날짜가 바뀌면 즉시 저장!
            DataManager.Instance.CurrentData.currentDay = currentDay;
            DataManager.Instance.SaveGameData();

            // 날짜 텍스트 갱신 (하루에 한 번만 실행되므로 안전함)
            dateDisplayText.text = "Day " + currentDay;
        }

        // 💡 GC 방어 로직: '분(Minute)'이 바뀌었을 때만 문자열을 새로 찍어냅니다.
        int totalMinutes = Mathf.FloorToInt(totalSeconds / 60);
        if (totalMinutes != lastCalculatedMinute)
        {
            lastCalculatedMinute = totalMinutes;
            UpdateClockUI(totalMinutes);
        }
    }

    private void UpdateClockUI(int totalMinutes)
    {
        int hours = (totalMinutes / 60) % 24;
        int minutes = totalMinutes % 60;

        // 시계: HH:mm (1분에 한 번만 가비지 발생)
        timeDisplayText.text = string.Format("{0:D2} : {1:D2}", hours, minutes);
    }

    private void ForceUpdateUI()
    {
        dateDisplayText.text = "Day " + currentDay;
        lastCalculatedMinute = -1; // 다음 Update에서 강제로 갱신되도록 유도
    }

    public void ToggleDayNight()
    {
        int currentHour = Mathf.FloorToInt(totalSeconds / 3600f) % 24;

        if (currentHour >= 6 && currentHour < 22)
            SetTime(22, 0);
        else
            SetTime(6, 0);
    }

    public void SetTime(int targetHour, int targetMinute)
    {
        totalSeconds = (targetHour * 3600f) + (targetMinute * 60f);
        lastCalculatedMinute = -1; // 시간 강제 변경 시 UI 즉시 갱신 유도
    }
}