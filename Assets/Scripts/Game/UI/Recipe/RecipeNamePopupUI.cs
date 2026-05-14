using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RecipeNamePopupUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField; // TextMeshPro를 쓰신다면 TMP_InputField로 변경하세요.
    [SerializeField] TMP_Text msgText;

    CanvasGroup popupPanel;
    private IReadOnlyList<IngredientData> pendingIngredients;

    private void Start() {
        popupPanel = GetComponent<CanvasGroup>();
        popupPanel.alpha = 0f;
        popupPanel.interactable = false;
        popupPanel.blocksRaycasts = false;
    }

    // CookingManager에서 미등록 레시피 발견 시 이 함수를 호출합니다.
    public void ShowPopup(IReadOnlyList<IngredientData> ingredients)
    {
        pendingIngredients = ingredients;
        nameInputField.text = "";
        popupPanel.alpha = 1f;
        popupPanel.interactable = true;
        popupPanel.blocksRaycasts = true;
    }

    // UI의 '확인' 버튼에 연결할 함수
    public void OnConfirmNaming()
    {
        string newName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            msgText.text = "메뉴 이름을 입력해야 합니다!";
            return;
        }

        // RecipeManager를 통해 신규 레시피 세이브 데이터에 등록
        if (CookingManager.Instance.recipeManager.TryDevelopNewRecipe(pendingIngredients, newName, out CustomRecipeData newRecipe))
        {
            // 도감에도 즉시 기록 (신규 메뉴이므로 기본 품질로 기록)
            CookingManager.Instance.recipeManager.RecordCookedDish(newName, false);

            // 냄비를 비워주고 창 닫기
            CookingManager.Instance.currentPot.ResetPot();
            popupPanel.alpha = 0f;
            popupPanel.interactable = false;
            popupPanel.blocksRaycasts = false;
        }
        else
        {
            msgText.text = "이미 존재하는 레시피입니다. 다른 이름을 입력해주세요.";
        }
    }

    // UI의 '취소' 또는 '닫기' 버튼에 연결할 함수
    public void OnCancel()
    {
        // 개발을 포기했으므로 냄비의 재료를 버립니다.
        CookingManager.Instance.currentPot.ResetPot();
        popupPanel.alpha = 0f;
        popupPanel.interactable = false;
        popupPanel.blocksRaycasts = false;
    }
}