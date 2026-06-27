using System.Collections.Generic;
using UnityEngine;
using System;

public enum ProcessState
{
    None,
    Processing,
    Completed,
    Spoiled
}

public class ProcessTask
{
    public EquipmentType equipmentType;
    public IngredientData inputIngredient;
    public ProcessMethodData method;
    public ProcessTypeEntry equipmentEntry;
    
    public ProcessState state;
    
    public float elapsedTime;
    public float qualityScore;
}

/// <summary>
/// 재료 가공 시스템의 핵심 매니저.
/// 백그라운드 태스크 방식으로 작동하며, 화면 전환 시에도 조리 시간이 유지됩니다.
/// </summary>
public class ProcessManager : MonoBehaviour
{
    public static ProcessManager Instance { get; private set; }

    [Header("옵션")]
    [Tooltip("체력 소모 배율 등 각종 매니저 관련 설정")]

    // 백그라운드 진행 중인 작업 목록 (장비 타입별 1개)
    private Dictionary<EquipmentType, ProcessTask> activeTasks = new Dictionary<EquipmentType, ProcessTask>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 백그라운드 타이머 업데이트
        foreach (var kvp in activeTasks)
        {
            ProcessTask task = kvp.Value;
            
            // timeMultiplier가 낮을수록 시간이 빨리 가도록 계산 (예: 0.5면 2배속)
            float speedMultiplier = 1f / Mathf.Max(0.1f, task.equipmentEntry.timeMultiplier);
            task.elapsedTime += Time.deltaTime * speedMultiplier;
            
            float optimalTime = task.method.GetOptimalTime();
            float ruinedTime = task.method.GetRuinedTime();

            if (task.state == ProcessState.Processing)
            {
                if (task.elapsedTime >= optimalTime)
                {
                    // 조리 자연 완료 (기본 품질 0.5)
                    task.state = ProcessState.Completed;
                    task.qualityScore = 0.5f; 
                    Debug.Log($"<color=green>[ProcessManager] {task.equipmentType} 조리 자연 완료! 수거 대기 중.</color>");
                }
            }
            else if (task.state == ProcessState.Completed)
            {
                if (task.elapsedTime >= ruinedTime)
                {
                    // 방치되어 타버림
                    task.state = ProcessState.Spoiled;
                    Debug.LogWarning($"<color=red>[ProcessManager] {task.equipmentType} 결과물이 타버렸습니다!</color>");
                }
            }
        }
    }

    // 특정 장비에 진행 중인 작업이 있는지 확인
    public ProcessTask GetActiveTask(EquipmentType type)
    {
        if (activeTasks.TryGetValue(type, out ProcessTask task))
        {
            return task;
        }
        return null;
    }

    // ─── 가공 실행 (백그라운드 등록) ──────────────────────────

    public bool StartProcess(EquipmentType equipType, IngredientData input, ProcessType processType)
    {
        if (activeTasks.ContainsKey(equipType))
        {
            Debug.LogWarning($"[가공 실패] {equipType} 은(는) 이미 작업 중입니다.");
            return false;
        }

        ProcessMethodData method = input.GetProcessMethod(processType);
        if (method == null)
        {
            Debug.LogWarning($"[가공 실패] {input.ingredientName} 를 {processType} 방식으로 가공할 수 없습니다.");
            return false;
        }

        ProcessTypeEntry equipmentEntry = GetEquipmentEntry(processType);
        float finalStamina = method.requiredStamina * equipmentEntry.staminaMultiplier;

        if (PlayerStaminaManager.Instance != null)
        {
            if (PlayerStaminaManager.Instance.CurrentStamina < finalStamina)
            {
                Debug.LogWarning("[가공 실패] 체력이 부족합니다.");
                return false;
            }
            // 미니게임 참여 여부와 무관하게 시작 시 체력 소모
            PlayerStaminaManager.Instance.DrainStamina(finalStamina);
        }

        ProcessTask newTask = new ProcessTask
        {
            equipmentType = equipType,
            inputIngredient = input,
            method = method,
            equipmentEntry = equipmentEntry,
            state = ProcessState.Processing,
            elapsedTime = 0f
        };

        activeTasks[equipType] = newTask;
        Debug.Log($"<color=cyan>[ProcessManager] {equipType} 조리 시작: {input.ingredientName} → {processType}</color>");
        return true;
    }

    // ─── 유저 상호작용 (미니게임 / 수거) ──────────────────────────

    /// <summary>
    /// 수동으로 조리 중인 장비를 터치했을 때 호출됩니다.
    /// Processing 중이라면 미니게임 시작.
    /// Completed 라면 수거 (성공 아이템 획득)
    /// Spoiled 라면 타버린 아이템 획득 (또는 폐기)
    /// </summary>
    public void InteractWithTask(EquipmentType equipType, Action<bool, IngredientData> onCollected)
    {
        if (!activeTasks.TryGetValue(equipType, out ProcessTask task))
        {
            return;
        }

        if (task.state == ProcessState.Processing)
        {
            // 아직 조리 중이면 미니게임 실행
            if (task.method.requiredMiniGame != MiniGameType.None && MiniGameManager.Instance != null)
            {
                Action<MiniGameResult> onMiniGameFinished = null;
                onMiniGameFinished = (result) =>
                {
                    MiniGameManager.Instance.OnMiniGameFinished -= onMiniGameFinished;
                    // 미니게임 완료 시 즉시 조리 완료(Optimal) 상태로 건너뜀
                    task.elapsedTime = task.method.GetOptimalTime();
                    task.state = ProcessState.Completed;
                    task.qualityScore = result.qualityScore;
                    CollectTask(equipType, onCollected);
                };
                MiniGameManager.Instance.OnMiniGameFinished += onMiniGameFinished;
                MiniGameManager.Instance.StartMiniGame(task.method.requiredMiniGame, task.equipmentEntry.miniGameEaseBonus);
            }
        }
        else if (task.state == ProcessState.Completed)
        {
            // 자연 완료된 요리 수거
            CollectTask(equipType, onCollected);
        }
        else if (task.state == ProcessState.Spoiled)
        {
            // 타버린 요리 수거
            CollectSpoiledTask(equipType, onCollected);
        }
    }

    private ItemGrade GetGrade(float quality)
    {
        if (quality >= 0.95f) return ItemGrade.Perfect;
        if (quality >= 0.8f) return ItemGrade.Premium;
        return ItemGrade.Normal;
    }

    private void CollectTask(EquipmentType equipType, Action<bool, IngredientData> onCollected)
    {
        if (activeTasks.TryGetValue(equipType, out ProcessTask task))
        {
            float finalQuality = Mathf.Min(1f, task.qualityScore + task.equipmentEntry.qualityBonus);
            ItemGrade finalGrade = GetGrade(finalQuality);
            string mark = finalGrade == ItemGrade.Perfect ? "🌟" : (finalGrade == ItemGrade.Premium ? "✨" : "");

            IngredientData resultItem = task.inputIngredient;
            Debug.Log($"<color=green>[ProcessManager] {equipType} 수거 완료! {mark}{resultItem.ingredientName} ({task.method.processType}, Optimal) 획득! (품질: {finalQuality:P0})</color>");
            
            IngredientBox targetBox = IngredientManager.Instance.FindOrAssignBoxFor(resultItem, IngredientState.Optimal, task.method.processType, finalGrade);
            if (targetBox != null)
            {
                targetBox.AddCollectedItem(1, finalQuality, resultItem.maxShelfLifeDays);
            }
            else
            {
                InventoryManager.Instance.AddIngredient(resultItem, 1, resultItem.maxShelfLifeDays, IngredientState.Optimal, task.method.processType, finalGrade);
            }

            // 태스크 삭제
            activeTasks.Remove(equipType);
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
        }
    }

    /// <summary>
    /// 조리 중간에 강제로 재료를 회수할 때 호출됩니다.
    /// 누른 시점의 상태(Raw, Optimal 등)를 반환합니다.
    /// </summary>
    public void ExtractTask(EquipmentType equipType, Action<bool, IngredientData> onCollected)
    {
        if (activeTasks.TryGetValue(equipType, out ProcessTask task))
        {
            var stateEntry = task.method.GetStateAtTime(task.elapsedTime);
            IngredientState currentState = stateEntry != null ? stateEntry.state : IngredientState.Raw;

            // 품질은 진행도에 비례해서 계산 (취소 시 최대 절반 품질까지만 허용 등)
            float progress = Mathf.Clamp01(task.elapsedTime / task.method.GetOptimalTime());
            float quality = Mathf.Lerp(0f, 0.5f, progress); // 미완성이므로 기본 품질(0.5)을 넘지 못함
            ItemGrade grade = GetGrade(quality);

            IngredientData resultItem = task.inputIngredient;
            Debug.Log($"<color=cyan>[ProcessManager] {equipType} 강제 회수! {resultItem.ingredientName} ({task.method.processType}, {currentState}) 획득!</color>");
            
            IngredientBox targetBox = IngredientManager.Instance.FindOrAssignBoxFor(resultItem, currentState, task.method.processType, grade);
            if (targetBox != null)
            {
                targetBox.AddCollectedItem(1, quality, resultItem.maxShelfLifeDays);
            }
            else
            {
                InventoryManager.Instance.AddIngredient(resultItem, 1, resultItem.maxShelfLifeDays, currentState, task.method.processType, grade);
            }

            activeTasks.Remove(equipType);
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
            onCollected?.Invoke(true, resultItem);
        }
    }

    private void CollectSpoiledTask(EquipmentType equipType, Action<bool, IngredientData> onCollected)
    {
        if (activeTasks.TryGetValue(equipType, out ProcessTask task))
        {
            IngredientData spoiledResult = task.inputIngredient;
            Debug.Log($"<color=red>[ProcessManager] {equipType}에서 타버린 요리({spoiledResult.ingredientName}, Ruined)를 수거했습니다.</color>");
            
            IngredientBox targetBox = IngredientManager.Instance.FindOrAssignBoxFor(spoiledResult, IngredientState.Ruined, task.method.processType, ItemGrade.Normal);
            if (targetBox != null)
            {
                targetBox.AddCollectedItem(1, 1.0f, spoiledResult.maxShelfLifeDays);
            }
            else
            {
                InventoryManager.Instance.AddIngredient(spoiledResult, 1, spoiledResult.maxShelfLifeDays, IngredientState.Ruined, task.method.processType, ItemGrade.Normal);
            }

            activeTasks.Remove(equipType);
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
            onCollected?.Invoke(false, spoiledResult);
        }
    }

    /// <summary>
    /// 인벤토리 UI 등에서 장비 없이 즉시 가공을 시도할 때 사용되는 메서드입니다.
    /// </summary>
    public void ExecuteProcess(InventoryItem inputItem, ProcessType processType, Action<bool, IngredientData> onCollected)
    {
        IngredientData input = inputItem.data;
        ProcessMethodData method = input.GetProcessMethod(processType);
        if (method == null)
        {
            Debug.LogWarning($"[가공 실패] {input.ingredientName} 를 {processType} 방식으로 가공할 수 없습니다.");
            onCollected?.Invoke(false, null);
            return;
        }

        ProcessTypeEntry equipmentEntry = GetEquipmentEntry(processType);
        float finalStamina = method.requiredStamina * equipmentEntry.staminaMultiplier;

        int consumedDays = InventoryManager.Instance.ConsumeExactItem(inputItem);
        if (consumedDays == -1)
        {
            Debug.LogWarning("[가공 실패] 선택된 재고를 찾을 수 없습니다.");
            onCollected?.Invoke(false, null);
            return;
        }

        if (PlayerStaminaManager.Instance != null)
        {
            if (PlayerStaminaManager.Instance.CurrentStamina < finalStamina)
            {
                Debug.LogWarning("[가공 실패] 체력이 부족합니다.");
                InventoryManager.Instance.AddIngredient(input, 1, consumedDays, inputItem.state, inputItem.processType, inputItem.grade);
                onCollected?.Invoke(false, null);
                return;
            }
            PlayerStaminaManager.Instance.DrainStamina(finalStamina);
        }

        Action<float> completeAction = (float quality) =>
        {
            IngredientData resultItem = input;
            float finalQuality = Mathf.Min(1f, quality + equipmentEntry.qualityBonus);
            ItemGrade finalGrade = GetGrade(finalQuality);
            
            InventoryManager.Instance.AddIngredient(resultItem, 1, resultItem.maxShelfLifeDays, IngredientState.Optimal, processType, finalGrade);
            Debug.Log($"<color=green>[ProcessManager] 직접 가공 완료! {resultItem.ingredientName} ({processType}) 획득!</color>");
            
            if (DataManager.Instance != null) DataManager.Instance.SaveGameData();
            onCollected?.Invoke(true, resultItem);
        };

        if (method.requiredMiniGame != MiniGameType.None && MiniGameManager.Instance != null)
        {
            Action<MiniGameResult> onMiniGameFinished = null;
            onMiniGameFinished = (result) =>
            {
                MiniGameManager.Instance.OnMiniGameFinished -= onMiniGameFinished;
                completeAction(result.qualityScore);
            };
            MiniGameManager.Instance.OnMiniGameFinished += onMiniGameFinished;
            MiniGameManager.Instance.StartMiniGame(method.requiredMiniGame, equipmentEntry.miniGameEaseBonus);
        }
        else
        {
            completeAction(0.5f); // 기본 품질 0.5
        }
    }

    // ─── 기존 내부 함수 ─────────────────────────────────────────

    private ProcessTypeEntry GetEquipmentEntry(ProcessType processType)
    {
        if (EquipmentStoreManager.Instance == null)
            return DefaultEntry(processType);

        EquipmentData bestEquipment = null;
        foreach (EquipmentType eqType in System.Enum.GetValues(typeof(EquipmentType)))
        {
            EquipmentData eq = EquipmentStoreManager.Instance.GetEquippedEquipment(eqType);
            if (eq != null && eq.Supports(processType))
            {
                if (bestEquipment == null || eq.tier > bestEquipment.tier)
                    bestEquipment = eq;
            }
        }

        if (bestEquipment != null)
        {
            int level = EquipmentStoreManager.Instance.GetEquipmentLevel(bestEquipment);
            return bestEquipment.GetEntryWithLevel(processType, level);
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

    // 💡 Mid-Day Save: 진행 중인 가공 태스크 저장
    public void SaveTaskStates(List<ProcessTaskSaveData> targetList)
    {
        foreach (var kvp in activeTasks)
        {
            ProcessTask task = kvp.Value;
            
            // 미니게임 중단(Processing 중이면서 미니게임이 필요한 경우)은 저장하지 않고 환불 처리할 수도 있지만,
            // 일단 상태를 모두 저장하고 로드 시 처리할 수도 있습니다. 여기서는 모두 저장.
            targetList.Add(new ProcessTaskSaveData
            {
                equipmentType = task.equipmentType,
                ingredientID = task.inputIngredient.ingredientID,
                targetProcessType = task.method.processType,
                elapsedTime = task.elapsedTime,
                qualityScore = task.qualityScore,
                processState = task.state
            });
        }
    }

    // 💡 Mid-Day Load: 가공 태스크 복원
    public void RestoreTaskStates(List<ProcessTaskSaveData> savedData)
    {
        if (GameManager.Instance == null || GameManager.Instance.recipeManager == null) return;
        RecipeManager recipeManager = GameManager.Instance.recipeManager;

        activeTasks.Clear();

        foreach (var data in savedData)
        {
            IngredientData ingData = recipeManager.GetIngredientById(data.ingredientID);
            if (ingData == null) continue;

            ProcessMethodData method = ingData.GetProcessMethod(data.targetProcessType);
            if (method == null) continue;

            ProcessTypeEntry equipmentEntry = GetEquipmentEntry(data.targetProcessType);

            // 미니게임 플레이 중 강제 종료(취소) 조건 체크: Processing 상태이고 미니게임이 필요한 장비
            if (data.processState == ProcessState.Processing && method.requiredMiniGame != MiniGameType.None)
            {
                Debug.Log($"<color=orange>[ProcessManager] 미니게임 중단 감지: {ingData.ingredientName} 가공이 취소되어 인벤토리로 반환됩니다.</color>");
                // 인벤토리로 원상 복구 (기본 Raw 상태라고 가정)
                InventoryManager.Instance.AddIngredient(ingData, 1, ingData.maxShelfLifeDays);
                continue;
            }

            ProcessTask newTask = new ProcessTask
            {
                equipmentType = data.equipmentType,
                inputIngredient = ingData,
                method = method,
                equipmentEntry = equipmentEntry,
                state = data.processState,
                elapsedTime = data.elapsedTime,
                qualityScore = data.qualityScore
            };

            activeTasks[data.equipmentType] = newTask;
        }
        
        Debug.Log($"<color=cyan>[ProcessManager] 가공 태스크({activeTasks.Count}개) 복원 완료.</color>");
    }
}

