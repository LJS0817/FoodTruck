using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecipeNamePopupUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public InputField nameInputField; // TextMeshPro를 쓰신다면 TMP_InputField로 변경하세요.

    private IReadOnlyList<IngredientData> pendingIngredients;

    // CookingManager에서 미등록 레시피 발견 시 이 함수를 호출합니다.
    public void ShowPopup(IReadOnlyList<IngredientData> ingredients)
    {
        pendingIngredients = ingredients;
        nameInputField.text = "";
        popupPanel.SetActive(true);
    }

    // UI의 '확인' 버튼에 연결할 함수
    public void OnConfirmNaming()
    {
        string newName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("메뉴 이름을 입력해야 합니다!");
            return;
        }

        // RecipeManager를 통해 신규 레시피 세이브 데이터에 등록
        if (CookingManager.Instance.recipeManager.TryDevelopNewRecipe(pendingIngredients, newName, out CustomRecipeData newRecipe))
        {
            // 도감에도 즉시 기록 (신규 메뉴이므로 기본 품질로 기록)
            CookingManager.Instance.recipeManager.RecordCookedDish(newName, false);

            // 냄비를 비워주고 창 닫기
            CookingManager.Instance.currentPot.ResetPot();
            popupPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("레시피 등록에 실패했습니다.");
        }
    }

    // UI의 '취소' 또는 '닫기' 버튼에 연결할 함수
    public void OnCancel()
    {
        // 개발을 포기했으므로 냄비의 재료를 버립니다.
        CookingManager.Instance.currentPot.ResetPot();
        popupPanel.SetActive(false);
    }
}