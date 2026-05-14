using System.Collections.Generic;
using UnityEngine;

public class UIWindowController : MonoBehaviour
{
    // List 대신 HashSet을 사용하여 중복 검사(Contains) 및 삭제(Remove) 속도를 O(1)로 최적화합니다.
    private HashSet<CanvasGroup> _openWindows = new HashSet<CanvasGroup>();

    public void OpenWindow(CanvasGroup cWindow)
    {
        if (cWindow == null) return;

        // 다른 창이 열려있다면 모두 닫기 (CloseAll)
        CloseAll();

        OpenWindowTrigger(cWindow);
    }

    public void OpenWindowTrigger(CanvasGroup cWindow) 
    {
        if (cWindow == null) return;

        Open(cWindow);
        // HashSet의 Add는 내장으로 중복을 방지하므로 Contains 검사가 필요 없습니다.
        _openWindows.Add(cWindow);
    }

    public void CloseWindow(CanvasGroup cWindow)
    {
        if (cWindow == null) return;

        Close(cWindow);
        _openWindows.Remove(cWindow);
    }

    private void Close(CanvasGroup cWindow)
    {
        if (cWindow == null) return;
        
        // 이미 닫혀있는 경우 불필요한 프로퍼티 접근(오버헤드) 방지
        if (!cWindow.interactable) return;

        cWindow.alpha = 0f;
        cWindow.interactable = false;
        cWindow.blocksRaycasts = false;
    }

    private void Open(CanvasGroup cWindow)
    {
        if (cWindow == null) return;

        // 이미 열려있는 경우 불필요한 프로퍼티 접근 방지
        if (cWindow.interactable) return;

        cWindow.alpha = 1f;
        cWindow.interactable = true;
        cWindow.blocksRaycasts = true;
    }

    public void CloseAll()
    {
        if (_openWindows.Count == 0) return;

        // foreach 할당 최소화 및 순회
        foreach (var window in _openWindows)
        {
            if (window != null)
            {
                Close(window);
            }
        }
        _openWindows.Clear();
    }
}