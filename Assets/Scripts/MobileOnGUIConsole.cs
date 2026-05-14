using System.Collections.Generic;
using UnityEngine;

public class MobileOnGUIConsole : MonoBehaviour
{
    private struct LogMessage
    {
        public string message;
        public LogType type;
    }

    [Header("Settings")]
    public int maxLogCount = 50;
    public int fontSizeRatio = 40;

    private Queue<LogMessage> logQueue = new Queue<LogMessage>();
    private Vector2 scrollPosition;
    private bool showConsole = true;
    private bool lastLogAdded = false; // 새로운 로그가 들어왔는지 체크

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logQueue.Enqueue(new LogMessage { message = logString, type = type });

        if (logQueue.Count > maxLogCount)
            logQueue.Dequeue();

        if (type == LogType.Error || type == LogType.Exception)
        {
            logQueue.Enqueue(new LogMessage { message = stackTrace, type = type });
            if (logQueue.Count > maxLogCount) logQueue.Dequeue();
        }

        // 새로운 로그가 추가되었음을 알림 (자동 스크롤용)
        lastLogAdded = true;
    }

    private void OnGUI()
    {
        if (!showConsole)
        {
            // 콘솔이 꺼져있을 때 다시 켜는 작은 버튼 (테스트용)
            if (GUI.Button(new Rect(10, 10, 150, Screen.height * 0.05f), "Show Console")) 
                showConsole = true;
            return;
        }

        int dynamicFontSize = Screen.width / fontSizeRatio;
        GUI.skin.label.fontSize = dynamicFontSize;
        GUI.skin.button.fontSize = dynamicFontSize;
        GUI.skin.textArea.fontSize = dynamicFontSize; // 스크롤 박스 내부 폰트 대응

        float buttonHeight = Screen.height * 0.08f;
        float margin = 20f;

        GUILayout.BeginArea(new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2)));

        // 상단 버튼 레이아웃
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Height(buttonHeight)))
        {
            logQueue.Clear();
        }
        if (GUILayout.Button("Hide", GUILayout.Height(buttonHeight)))
        {
            showConsole = false;
        }
        GUILayout.EndHorizontal();

        // [핵심] 스크롤 뷰 설정
        // GUI.skin.box를 사용해 배경을 가시성 있게 만듭니다.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));

        foreach (var log in logQueue)
        {
            GUI.contentColor = GetLogColor(log.type);
            // Label 대신 TextArea를 쓰면 텍스트 복사 등이 용이하지만, 
            // 단순히 보는 용도라면 Label이 성능상 더 가볍습니다.
            GUILayout.Label($"[{log.type}] {log.message}");
        }

        // 새로운 로그가 들어왔다면 스크롤 위치를 가장 아래로 강제 이동
        if (lastLogAdded)
        {
            scrollPosition.y = float.MaxValue; 
            lastLogAdded = false;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.contentColor = Color.white;
    }

    private Color GetLogColor(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert: return Color.red;
            case LogType.Warning: return Color.yellow;
            default: return Color.white;
        }
    }
}