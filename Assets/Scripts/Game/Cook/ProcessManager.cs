using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 재료 가공 시스템의 핵심 매니저.
/// 가공 실행 흐름: 재료 검증 → 장비 검색 및 보너스 계산 → 체력 소모 → 미니게임 실행 → 완료 처리
/// </summary>
public class ProcessManager : MonoBehaviour
{
    public static ProcessManager Instance { get; private set; }

    [Header("모든 가공 레시피 데이터")]
    public List<ProcessRecipeData> allProcessRecipes;

    // Key: inputIngredient.ingredientID, Value: 해당 재료로 가능한 레시피 목록
    private Dictionary<int, List<ProcessRecipeData>> processDict = new Dictionary<int, List<ProcessRecipeData>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeDict();
    }

    private void InitializeDict()
    {
        processDict.Clear();
        for (int i = 0; i < allProcessRecipes.Count; i++)
        {
            ProcessRecipeData recipe = allProcessRecipes[i];
            if (recipe.inputIngredient == null) continue;
            int inputId = recipe.inputIngredient.ingredientID;
            if (!processDict.ContainsKey(inputId))
                processDict[inputId] = new List<ProcessRecipeData>();
            processDict[inputId].Add(recipe);
        }
    }

    // ─── 조회 ──────────────────────────────────────────────

    /// <summary>
    /// 특정 재료로 가능한 모든 가공 방식 반환
    /// </summary>
    public List<ProcessRecipeData> GetPossibleProcesses(IngredientData input)
    {
        if (input != null && processDict.TryGetValue(input.ingredientID, out List<ProcessRecipeData> list))
            return list;
        return new List<ProcessRecipeData>();
    }

    /// <summary>
    /// 재료 + 가공 타입으로 레시피 검색
    /// </summary>
    public ProcessRecipeData GetProcessRecipe(IngredientData input, ProcessType processType)
    {
        List<ProcessRecipeData> list = GetPossibleProcesses(input);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].processType == processType)
                return list[i];
        }
        return null;
    }

    // ─── 가공 실행 ─────────────────────────────────────────

    /// <summary>
    /// 가공 실행 진입점.
    /// 장비가 있으면 보너스를 적용하고, 미니게임이 설정되어 있으면 미니게임 실행 후 완료합니다.
    /// </summary>
    public void ExecuteProcess(IngredientData input, ProcessType processType, Action<bool, IngredientData> onComplete, bool consumeInventory = true, bool addToInventory = true)
    {
        ProcessRecipeData recipe = GetProcessRecipe(input, processType);

        if (recipe == null)
        {
            Debug.LogWarning($"[가공 실패] {input.ingredientName} 를 {processType} 방식으로 가공할 수 없습니다.");
            onComplete?.Invoke(false, null);
            return;
        }

        // 1. 장비 조회 및 보너스 계산
        ProcessTypeEntry equipmentEntry = GetEquipmentEntry(processType);
        float finalStamina = recipe.requiredStamina * equipmentEntry.staminaMultiplier;
        float finalTime    = recipe.processTime      * equipmentEntry.timeMultiplier;

        // 2. 인벤토리 차감 (UI 등에서 직접 호출했을 때만)
        if (consumeInventory)
        {
            if (!InventoryManager.Instance.UseIngredient(input.ingredientID))
            {
                Debug.LogWarning("[가공 실패] 재고가 부족합니다.");
                onComplete?.Invoke(false, null);
                return;
            }
        }

        // 3. 체력 검사 및 차감
        if (PlayerStaminaManager.Instance != null)
        {
            if (PlayerStaminaManager.Instance.CurrentStamina < finalStamina)
            {
                Debug.LogWarning("[가공 실패] 체력이 부족합니다.");
                InventoryManager.Instance.AddIngredient(input, 1, input.maxShelfLifeDays);
                onComplete?.Invoke(false, null);
                return;
            }
            PlayerStaminaManager.Instance.DrainStamina(finalStamina);
        }

        Debug.Log($"[가공 시작] {input.ingredientName} → {processType} | 시간:{finalTime:F1}초 | 장비 보너스 적용");

        // 4. 미니게임 실행 (설정된 경우)
        if (recipe.miniGameType != MiniGameType.None && MiniGameManager.Instance != null)
        {
            // 콜백 클로저 – 미니게임 결과를 받아 완료 처리
            Action<MiniGameResult> onMiniGameFinished = null;
            onMiniGameFinished = (result) =>
            {
                MiniGameManager.Instance.OnMiniGameFinished -= onMiniGameFinished;
                CompleteProcess(recipe, result.qualityScore, equipmentEntry, onComplete, addToInventory);
            };
            MiniGameManager.Instance.OnMiniGameFinished += onMiniGameFinished;
            MiniGameManager.Instance.StartMiniGame(recipe.miniGameType, equipmentEntry.miniGameEaseBonus);
        }
        else
        {
            // 미니게임 없음 → 시간만 대기하고 기본 품질로 완료
            // 실제 타이머가 필요하면 Coroutine으로 finalTime 만큼 대기 후 호출
            CompleteProcess(recipe, 0.5f, equipmentEntry, onComplete, addToInventory);
        }
    }

    // ─── 내부 처리 ─────────────────────────────────────────

    private void CompleteProcess(ProcessRecipeData recipe, float rawQualityScore,
                                  ProcessTypeEntry equipmentEntry, Action<bool, IngredientData> onComplete, bool addToInventory)
    {
        // 장비의 품질 보너스를 합산하되 1.0을 초과하지 않게 제한
        float finalQuality = Mathf.Min(1f, rawQualityScore + equipmentEntry.qualityBonus);
        bool isPremium = finalQuality >= 0.8f;

        string mark = isPremium ? "✨" : "";
        Debug.Log($"<color=cyan>[가공 완료] {mark}{recipe.outputIngredient.ingredientName} 획득! (품질: {finalQuality:P0})</color>");

        if (addToInventory)
        {
            InventoryManager.Instance.AddIngredient(recipe.outputIngredient, 1, recipe.outputIngredient.maxShelfLifeDays);
        }
        onComplete?.Invoke(true, recipe.outputIngredient);
    }

    /// <summary>
    /// 현재 보유 장비 중 해당 ProcessType을 지원하는 장비의 효과를 반환합니다.
    /// 장비가 없거나 지원하지 않으면 보너스가 없는 기본값(multiplier = 1.0)을 반환합니다.
    /// </summary>
    private ProcessTypeEntry GetEquipmentEntry(ProcessType processType)
    {
        if (EquipmentStoreManager.Instance == null)
            return DefaultEntry(processType);

        // EquipmentType 순서대로 장비를 검색하여 해당 ProcessType을 지원하는 장비를 찾음
        // (Tier가 높은 장비일수록 더 좋은 보너스를 가지므로, 같은 ProcessType이 중복되면 높은 Tier가 우선)
        EquipmentData bestEquipment = null;
        foreach (EquipmentType eqType in System.Enum.GetValues(typeof(EquipmentType)))
        {
            EquipmentData eq = EquipmentStoreManager.Instance.GetOwnedEquipment(eqType);
            if (eq != null && eq.Supports(processType))
            {
                if (bestEquipment == null || eq.tier > bestEquipment.tier)
                    bestEquipment = eq;
            }
        }

        if (bestEquipment != null)
        {
            Debug.Log($"[가공] 장비 '{bestEquipment.equipmentName}' (Tier {bestEquipment.tier}) 효과 적용");
            return bestEquipment.GetEntry(processType);
        }

        return DefaultEntry(processType);
    }

    private static ProcessTypeEntry DefaultEntry(ProcessType type)
    {
        return new ProcessTypeEntry
        {
            processType = type,
            timeMultiplier     = 1f,
            staminaMultiplier  = 1f,
            qualityBonus       = 0f,
            miniGameEaseBonus  = 0f
        };
    }
}
