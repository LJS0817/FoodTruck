using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingPot : MonoBehaviour
{
    [Header("Settings")]
    public Transform ingredientStackParent;
    public float stackOffset = 0.2f;

    private List<IngredientData> contents = new List<IngredientData>(10);
    private List<GameObject> visualStack = new List<GameObject>(10);

    // 💡 미니게임 및 프리미엄 판별을 위한 변수 추가
    private int premiumCount = 0;
    private IngredientData pendingIngredient = null;

    private void OnEnable()
    {
        // 미니게임 매니저 이벤트 구독 (가비지 발생 없는 명시적 콜백)
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.OnMiniGameFinished += HandleMiniGameResult;
        }
    }

    private void OnDisable()
    {
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.OnMiniGameFinished -= HandleMiniGameResult;
        }
    }

    // 💡 1. 외부(유저 드래그)에서 재료를 넣을 때 최초로 호출되는 함수
    public void ReceiveIngredient(IngredientData data)
    {
        if (pendingIngredient != null) return; // 이미 다른 미니게임 진행 중이면 무시

        // IngredientData에 requiredMiniGame 열거형(Enum)이 있다고 가정합니다.
        if (data.requiredMiniGame != MiniGameType.None)
        {
            // 대기열에 올리고 미니게임 팝업 호출
            pendingIngredient = data;

            // MiniGameManager.Instance.StartMiniGame() 호출 시 Enum을 string으로 캐스팅하거나 Enum 자체를 받게 설정
            MiniGameManager.Instance.StartMiniGame(data.requiredMiniGame.ToString());
        }
        else
        {
            // 미니게임이 필요 없는 재료는 즉시 일반 품질로 투입
            AddIngredientToPot(data, false);
        }
    }

    // 💡 2. 미니게임이 끝나면 매니저가 이 함수를 호출하여 결과를 알려줍니다.
    private void HandleMiniGameResult(MiniGameResult result)
    {
        if (pendingIngredient == null) return;

        // 성공하고 점수가 높으면 프리미엄 판정!
        bool isPremium = result.isSuccess && result.qualityScore >= 0.8f;

        AddIngredientToPot(pendingIngredient, isPremium);
        pendingIngredient = null; // 대기 해제
    }

    // 💡 3. 실제 냄비에 재료를 쌓는 로직 (기존 AddIngredient 역할)
    private void AddIngredientToPot(IngredientData data, bool isPremium)
    {
        contents.Add(data);
        if (isPremium) premiumCount++; // 프리미엄 카운트 증가

        UpdateVisualStack(data);

        string qualityMark = isPremium ? "✨" : "";
        Debug.Log($"[CookingPot] {qualityMark}{data.ingredientName} 추가됨. 현재 재료 수: {contents.Count}");

        CheckCurrentRecipe();
    }

    public void ResetPot()
    {
        contents.Clear();
        premiumCount = 0; // 💡 카운트도 잊지 않고 초기화

        for (int i = 0; i < visualStack.Count; i++)
        {
            Destroy(visualStack[i]);
        }
        visualStack.Clear();
    }

    private void UpdateVisualStack(IngredientData data)
    {
        GameObject visual = new GameObject(data.ingredientName + "_Visual");
        visual.transform.SetParent(ingredientStackParent);

        float yPos = visualStack.Count * stackOffset;
        visual.transform.localPosition = new Vector3(0, yPos, 0);

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = data.ingredientSprite;
        renderer.sortingOrder = 10 + visualStack.Count;

        visualStack.Add(visual);
    }

    public void CheckCurrentRecipe()
    {
        if (contents.Count == 0) return;

        FoodData result = GameManager.Instance.recipeManager.CheckRecipe(contents);

        if (result != null)
        {
            Debug.Log($"<color=green>[판별 완료] 현재 요리: {result.foodName}</color>");
        }
        else
        {
            Debug.Log("<color=red>[판별 실패] 일치하는 레시피가 없습니다.</color>");
        }
    }

    public IReadOnlyList<IngredientData> GetContents()
    {
        return contents;
    }

    // 💡 최종 완성 시 요리가 프리미엄인지 판별하는 함수 추가
    public bool IsPremiumDish()
    {
        if (contents.Count == 0) return false;
        // 들어간 모든 재료가 프리미엄이어야 프리미엄 요리로 인정 (기획에 따라 조건 변경 가능)
        return premiumCount == contents.Count;
    }
}