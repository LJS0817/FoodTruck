using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuPreviewUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject previewPanel;      // 전체 패널
    [SerializeField] private Transform contentParent;      // 아이템이 배치될 부모
    [SerializeField] private MenuPreviewItemUI itemPrefab; // 아이템 프리팹
    [SerializeField] private TMP_Text emptyNoticeText;    // 메뉴가 없을 때 표시할 텍스트

    private List<MenuPreviewItemUI> spawnedItems = new List<MenuPreviewItemUI>();

    private void Start()
    {
        // MenuManager 이벤트 구독
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnMenuUpdated += RefreshUI;
        }

        // DayPhase 변경 이벤트 구독 (준비 단계에서만 보여주기 위함)
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(DayCycleManager.Instance.CurrentPhase);
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnMenuUpdated -= RefreshUI;
        }
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(DayPhase phase)
    {
        // 준비(Preparation) 및 장사(Business) 단계에서만 리스트를 보여줍니다.
        bool shouldShow = (phase == DayPhase.Preparation || phase == DayPhase.Business);
        previewPanel.SetActive(shouldShow);
    }

    public void RefreshUI()
    {
        if (MenuManager.Instance == null) return;

        List<FoodData> recipes = MenuManager.Instance.GetAvailableRecipes();

        // 1. 기존 아이템 비활성화 (풀링)
        foreach (var item in spawnedItems)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 새로운 레시피 리스트 생성
        if (recipes.Count == 0)
        {
            emptyNoticeText.gameObject.SetActive(true);
        }
        else
        {
            emptyNoticeText.gameObject.SetActive(false);
            for (int i = 0; i < recipes.Count; i++)
            {
                if (i >= spawnedItems.Count)
                {
                    MenuPreviewItemUI newItem = Instantiate(itemPrefab, contentParent);
                    spawnedItems.Add(newItem);
                }

                spawnedItems[i].SetInfo(recipes[i]);
                spawnedItems[i].gameObject.SetActive(true);
            }
        }
    }
}
