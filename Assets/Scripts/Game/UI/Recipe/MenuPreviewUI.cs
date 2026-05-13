using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuPreviewUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup previewPanel;      // 전체 패널
    [SerializeField] private Transform normalRecipeContent; // 기본 레시피 부모
    [SerializeField] private Transform customRecipeContent; // 커스텀 레시피 부모
    [SerializeField] private MenuPreviewItemUI itemPrefab; // 아이템 프리팹
    [SerializeField] private TMP_Text emptyNoticeText;    // 메뉴가 없을 때 표시할 텍스트
    [SerializeField] private int _maxCount = 5;           // 최대 표시 개수
    [SerializeField] private TMP_Text normalMoreText;     // 기본 레시피 초과 시 텍스트
    [SerializeField] private TMP_Text customMoreText;     // 커스텀 레시피 초과 시 텍스트

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
        if (previewPanel != null)
        {
            previewPanel.alpha = shouldShow ? 1f : 0f;
            previewPanel.interactable = shouldShow;
            previewPanel.blocksRaycasts = shouldShow;
        }
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

        if (normalMoreText != null) normalMoreText.gameObject.SetActive(false);
        if (customMoreText != null) customMoreText.gameObject.SetActive(false);

        // 2. 새로운 레시피 리스트 생성
        if (recipes.Count == 0)
        {
            emptyNoticeText.gameObject.SetActive(true);
        }
        else
        {
            emptyNoticeText.gameObject.SetActive(false);
            
            int normalCount = 0;
            int customCount = 0;
            int normalTotal = 0;
            int customTotal = 0;
            
            // 전체 개수 파악
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i].isCustomRecipe) customTotal++;
                else normalTotal++;
            }
            
            int spawnedIndex = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                bool isCustom = recipes[i].isCustomRecipe;
                
                // 지정된 최대 개수 초과 시 생성 및 표시를 생략
                if (isCustom && customCount >= _maxCount) continue;
                if (!isCustom && normalCount >= _maxCount) continue;

                if (spawnedIndex >= spawnedItems.Count)
                {
                    MenuPreviewItemUI newItem = Instantiate(itemPrefab);
                    spawnedItems.Add(newItem);
                }

                MenuPreviewItemUI currentItem = spawnedItems[spawnedIndex];
                currentItem.SetInfo(recipes[i]);
                
                // 기본 레시피와 커스텀 레시피 구분하여 부모 설정
                Transform targetParent = isCustom ? customRecipeContent : normalRecipeContent;
                currentItem.transform.SetParent(targetParent, false);
                
                currentItem.gameObject.SetActive(true);
                
                if (isCustom) customCount++;
                else normalCount++;
                
                spawnedIndex++;
            }
            
            // "외 n개" 텍스트 처리
            if (normalTotal > _maxCount && normalMoreText != null)
            {
                normalMoreText.text = $"외 {normalTotal - _maxCount}개";
                normalMoreText.transform.SetAsLastSibling();
                normalMoreText.gameObject.SetActive(true);
            }
            if (customTotal > _maxCount && customMoreText != null)
            {
                customMoreText.text = $"외 {customTotal - _maxCount}개";
                customMoreText.transform.SetAsLastSibling();
                customMoreText.gameObject.SetActive(true);
            }
        }
    }
}
