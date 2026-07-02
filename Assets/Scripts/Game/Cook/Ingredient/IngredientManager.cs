using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    public static IngredientManager Instance { get; private set; }

    [SerializeField] InventoryManager _inventoryMng;
    [SerializeField] List<IngredientBoxSetter> _boxSetters;
    [SerializeField] Transform _boxParent;
    
    [Header("Dynamic Box Settings")]
    [Tooltip("UI 버전 IngredientBox 프리팹을 연결하세요.")]
    [SerializeField] IngredientBox _boxPrefab;
    [Tooltip("초기 상자 개수. 추후 업그레이드 시스템과 연동 시 이 값을 변경하거나 UpdateBoxCount를 호출하세요.")]
    public int maxBoxCount = 4;

    [Header("Temp Box Settings")]
    [Tooltip("임시 상자가 스폰될 부모 오브젝트입니다. (Inspector에서 할당 필요)")]
    [SerializeField] Transform _tempBoxParent;
    public int tempBoxCount = 3;
    private List<IngredientBox> _tempBoxes;

    List<IngredientBox> _boxes;
    int _currentBoxIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        _currentBoxIndex = -1;
        _boxes = new List<IngredientBox>();
        _tempBoxes = new List<IngredientBox>();

        // 에디터 등에 남아있을 수 있는 더미 데이터 삭제
        for (int i = _boxParent.childCount - 1; i > 0; i--)
        {
            Destroy(_boxParent.GetChild(i).gameObject);
        }
        
        if (_tempBoxParent != null)
        {
            for (int i = _tempBoxParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_tempBoxParent.GetChild(i).gameObject);
            }
        }

        UpdateBoxCount(maxBoxCount);
        InitTempBoxes();
    }

    private void Start()
    {
        if (_boxSetters != null && _boxSetters.Count > 0 && _boxSetters[0].boxData != null)
        {
            // 초기 더미 재료 추가 (필요시 제거 가능)
            _inventoryMng.AddIngredient(_boxSetters[0].boxData, 100, _boxSetters[0].boxData.maxShelfLifeDays);
        }
    }

    /// <summary>
    /// 상자 개수를 동적으로 변경합니다. (추후 업그레이드 시 호출)
    /// </summary>
    public void UpdateBoxCount(int newCount)
    {
        maxBoxCount = newCount;
        
        ScrollRect parentScrollRect = _boxParent.GetComponentInParent<ScrollRect>();

        while (_boxes.Count < maxBoxCount)
        {
            int index = _boxes.Count;
            IngredientBox newBox = Instantiate(_boxPrefab, _boxParent);
            _boxes.Add(newBox);
            
            newBox.Init(
                index,
                onRefill: () => {
                    _currentBoxIndex = index; 
                    OpenInventoryForRefill();  
                },
                onSetup: () => { 
                    _currentBoxIndex = index; 
                    OpenInventoryForSetup(); 
                },
                scrollRect: parentScrollRect
            );
        }
    }

    private void InitTempBoxes()
    {
        if (_tempBoxParent == null) return;

        ScrollRect parentScrollRect = _tempBoxParent.GetComponentInParent<ScrollRect>();

        for (int i = 0; i < tempBoxCount; i++)
        {
            IngredientBox newBox = Instantiate(_boxPrefab, _tempBoxParent);
            newBox.isTemporary = true;
            _tempBoxes.Add(newBox);
            
            newBox.Init(i, null, null, parentScrollRect);
        }
    }

    private void OpenInventoryForSetup()
    {
        IngredientData targetData = _boxes[_currentBoxIndex].GetCurrentData();
        Debug.Log($"[IngredientBoxManager] {_currentBoxIndex}번 상자 세팅을 위해 인벤토리를 엽니다. (대상: {targetData?.ingredientName})");
        _inventoryMng.OpenUIWithApplyBtn(targetData);
    }

    private void OpenInventoryForRefill()
    {
        IngredientData targetData = _boxes[_currentBoxIndex].GetCurrentData();
        Debug.Log($"[IngredientBoxManager] {_currentBoxIndex}번 상자 세팅/리필을 위해 인벤토리를 엽니다. (대상: {targetData?.ingredientName})");
        _inventoryMng.OpenUIWithApplyBtn(targetData);
    }

    public void SetupBox(int idx, float quality = 1.0f)
    {
        _boxes[_currentBoxIndex].SetupIngredient(_boxSetters[idx], IngredientState.Raw, ProcessType.None, quality);
    }

    public void EmptyCurrentBox()
    {
        if (_currentBoxIndex >= 0 && _currentBoxIndex < _boxes.Count)
        {
            _boxes[_currentBoxIndex].ResetBox();
        }
    }

    public IngredientBox GetCurrentBox()
    {
        if (_currentBoxIndex >= 0 && _currentBoxIndex < _boxes.Count)
        {
            return _boxes[_currentBoxIndex];
        }
        return null;
    }

    public void SetupBox(InventoryItem item, int amount = -1)
    {
        IngredientData data = item.data;
        // 💡 미니게임 체크 로직 추가
        if (data.requiredMiniGame != MiniGameType.None && MiniGameManager.Instance != null)
        {
            Debug.Log($"[IngredientManager] {data.ingredientName} 가공을 위해 {data.requiredMiniGame} 미니게임을 시작합니다.");
            
            // 일회성 이벤트 등록
            Action<MiniGameResult> onFinished = null;
            onFinished = (result) => {
                MiniGameManager.Instance.OnMiniGameFinished -= onFinished;
                
                // 가공 성공 시에만 세팅 (또는 점수에 따른 차등 처리)
                if (result.isSuccess)
                {
                    // 점수 계산 (예: 1.0~1.2 프리미엄 보너스)
                    float finalQuality = 1.0f + (result.qualityScore * 0.2f);
                    CompleteSetup(item, finalQuality, amount);
                }
                else
                {
                    Debug.Log("[IngredientManager] 가공 실패! 일반 품질로 세팅됩니다.");
                    CompleteSetup(item, 1.0f, amount);
                }
            };

            MiniGameManager.Instance.OnMiniGameFinished += onFinished;
            MiniGameManager.Instance.StartMiniGame(data.requiredMiniGame, 0f);
        }
        else
        {
            float baseQuality = 1.0f;
            if (item.grade == ItemGrade.Premium) baseQuality = 1.2f;
            else if (item.grade == ItemGrade.Perfect) baseQuality = 1.4f;
            CompleteSetup(item, baseQuality, amount);
        }
    }

    private void CompleteSetup(InventoryItem item, float quality, int amount)
    {
        for (int i = 0; i < _boxSetters.Count; i++)
        {
            if (_boxSetters[i].boxData.ingredientID == item.data.ingredientID)
            {
                _boxes[_currentBoxIndex].SetupIngredient(_boxSetters[i], item.state, item.processType, quality, amount);
                return;
            }
        }
    }

    public List<IngredientBox> GetAllBoxes()
    {
        return _boxes;
    }

    public IngredientBox FindOrAssignBoxFor(IngredientData data, IngredientState state, ProcessType pt, ItemGrade grade)
    {
        // 1. Exact match (not empty)
        foreach (var box in _boxes)
        {
            if (box.currentAmount > 0 && box.GetCurrentData() != null &&
                box.GetCurrentData().ingredientID == data.ingredientID &&
                box.targetState == state && box.targetProcess == pt)
            {
                return box;
            }
        }
        if (_tempBoxes != null)
        {
            foreach (var box in _tempBoxes)
            {
                if (box.currentAmount > 0 && box.GetCurrentData() != null &&
                    box.GetCurrentData().ingredientID == data.ingredientID &&
                    box.targetState == state && box.targetProcess == pt)
                {
                    return box;
                }
            }
        }

        IngredientBoxSetter setter = GetSetterFor(data);
        if (setter == null) return null;

        // 2. Empty Fixed Box
        foreach (var box in _boxes)
        {
            if (box.currentAmount == 0)
            {
                box.SetupFromCollectedItem(setter, state, pt, 1.0f, 0);
                return box;
            }
        }

        // 3. Empty Temp Box
        if (_tempBoxes != null)
        {
            foreach (var box in _tempBoxes)
            {
                if (box.currentAmount == 0)
                {
                    box.SetupFromCollectedItem(setter, state, pt, 1.0f, 0);
                    return box;
                }
            }
        }

        // 4. No space
        return null;
    }

    private IngredientBoxSetter GetSetterFor(IngredientData data)
    {
        if (_boxSetters == null) return null;
        for (int i = 0; i < _boxSetters.Count; i++)
        {
            if (_boxSetters[i].boxData != null && _boxSetters[i].boxData.ingredientID == data.ingredientID)
            {
                return _boxSetters[i];
            }
        }
        return null;
    }

    public void ClearAllTempBoxes()
    {
        if (_tempBoxes == null) return;
        
        foreach (var box in _tempBoxes)
        {
            if (box.currentAmount > 0)
            {
                box.ResetBox(); // 내부적으로 환수 없이 껍데기 초기화
            }
        }
        Debug.Log("<color=cyan>[IngredientManager] 임시 바트 초기화 완료.</color>");
    }

    /// <summary>
    /// 지정된 인벤토리 아이템이 현재 열려 있는 상자를 제외한 '다른' 상자에 배치되어 있는지 확인합니다.
    /// </summary>
    public bool IsPlacedInAnotherBox(InventoryItem item)
    {
        if (item == null || item.data == null) return false;

        ItemGrade targetGrade = item.grade;

        for (int i = 0; i < _boxes.Count; i++)
        {
            if (i == _currentBoxIndex) continue; // 현재 설정 중인 박스 통과

            var box = _boxes[i];
            if (box.GetCurrentData() != null && box.GetCurrentData().ingredientID == item.data.ingredientID)
            {
                ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                if (boxGrade == targetGrade && box.targetState == item.state && box.targetProcess == item.processType)
                {
                    return true;
                }
            }
        }

        if (_tempBoxes != null)
        {
            foreach (var box in _tempBoxes)
            {
                // tempBox는 인덱스로 비교하기 모호하지만 일반적으로 세팅 중인 박스는 고정 바트이므로 여기는 모두 체크
                if (box.GetCurrentData() != null && box.GetCurrentData().ingredientID == item.data.ingredientID)
                {
                    ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                    if (boxGrade == targetGrade && box.targetState == item.state && box.targetProcess == item.processType)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    
    /// <summary>
    /// 특정 아이템이 '어느 상자에든' 배치되어 있는지 확인 (UI 표시용)
    /// </summary>
    public bool IsPlacedAnywhere(InventoryItem item)
    {
        if (item == null || item.data == null) return false;

        ItemGrade targetGrade = item.grade;

        foreach (var box in _boxes)
        {
            if (box.GetCurrentData() != null && box.GetCurrentData().ingredientID == item.data.ingredientID)
            {
                ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                if (boxGrade == targetGrade && box.targetState == item.state && box.targetProcess == item.processType)
                {
                    return true;
                }
            }
        }

        if (_tempBoxes != null)
        {
            foreach (var box in _tempBoxes)
            {
                if (box.GetCurrentData() != null && box.GetCurrentData().ingredientID == item.data.ingredientID)
                {
                    ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                    if (boxGrade == targetGrade && box.targetState == item.state && box.targetProcess == item.processType)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리에 남은 재고가 0인 상태의 박스들을 찾아 모두 비웁니다.
    /// </summary>
    public void CheckAndEmptyBoxesWithoutStock()
    {
        if (InventoryManager.Instance == null) return;

        Action<IngredientBox> checkEmptyBox = (box) =>
        {
            if (box.GetCurrentData() != null)
            {
                ItemGrade boxGrade = box.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                int totalAmount = InventoryManager.Instance.GetTotalSpecificAmount(box.GetCurrentData().ingredientID, box.targetState, box.targetProcess, boxGrade);
                
                if (totalAmount <= 0)
                {
                    Debug.Log($"<color=orange>[IngredientManager] 재고가 바닥나서 {box.GetCurrentData().ingredientName} 상자를 리셋합니다.</color>");
                    box.ResetBox();
                }
            }
        };

        foreach (var box in _boxes)
        {
            checkEmptyBox(box);
        }

        if (_tempBoxes != null)
        {
            foreach (var box in _tempBoxes)
            {
                checkEmptyBox(box);
            }
        }
    }

    /// <summary>
    /// 메뉴 팝업에서 선택한 레시피들의 요구 재료들을 빈 상자에 자동으로 세팅합니다.
    /// </summary>
    public void AutoFillBoxes(List<IngredientData> uniqueIngredients)
    {
        // 1. 기존 상자 초기화 (원한다면 기존에 들어있던 재료를 유지할 수도 있으나, 여기서는 덮어쓰거나 리셋하는 방식을 취합니다)
        // 안전하게 인벤토리로 모두 환수
        for (int i = 0; i < _boxes.Count; i++)
        {
            if (_boxes[i].currentAmount > 0)
            {
                _boxes[i].ResetBox();
            }
        }

        // 2. 전달받은 고유 재료들을 상자에 할당
        for (int i = 0; i < uniqueIngredients.Count; i++)
        {
            if (i >= _boxes.Count) break; // 상자 개수를 초과하면 무시

            IngredientData data = uniqueIngredients[i];
            IngredientBoxSetter setter = GetSetterFor(data);
            
            if (setter != null)
            {
                // 💡 보유한 재고 중 첫 번째 아이템의 정보(등급, 상태)를 기준으로 상자를 자동 세팅합니다.
                InventoryItem availableItem = InventoryManager.Instance.GetFirstAvailableItem(data.ingredientID);
                if (availableItem != null)
                {
                    // 최대 수량만큼 세팅 (예: 10개) -> Inventory에서 드래그 시점에 차감됨
                    int amountToPut = Mathf.Min(InventoryManager.Instance.GetTotalAmount(data.ingredientID), 10); 
                    float quality = availableItem.grade == ItemGrade.Premium ? 1.2f : (availableItem.grade == ItemGrade.Perfect ? 1.4f : 1.0f);
                    _boxes[i].SetupIngredient(setter, availableItem.state, availableItem.processType, quality, amountToPut);
                }
            }
        }
        
        Debug.Log($"<color=cyan>[IngredientManager] {uniqueIngredients.Count}개의 재료를 조리대에 자동 세팅 완료!</color>");
    }
    
    // 💡 Mid-Day Save: 현재 상자 상태 저장
    public void SaveBoxStates(List<IngredientBoxSaveData> targetList)
    {
        for (int i = 0; i < _boxes.Count; i++)
        {
            if (_boxes[i].currentAmount > 0 && _boxes[i].GetCurrentData() != null)
            {
                targetList.Add(new IngredientBoxSaveData
                {
                    index = i,
                    isTempBox = false,
                    ingredientID = _boxes[i].GetCurrentData().ingredientID,
                    amount = _boxes[i].currentAmount,
                    storedItemDays = new List<int>(_boxes[i].storedItemDays),
                    state = _boxes[i].targetState,
                    processType = _boxes[i].targetProcess,
                    qualityScore = _boxes[i].qualityScore
                });
            }
        }
        
        if (_tempBoxes != null)
        {
            for (int i = 0; i < _tempBoxes.Count; i++)
            {
                if (_tempBoxes[i].currentAmount > 0 && _tempBoxes[i].GetCurrentData() != null)
                {
                    targetList.Add(new IngredientBoxSaveData
                    {
                        index = i,
                        isTempBox = true,
                        ingredientID = _tempBoxes[i].GetCurrentData().ingredientID,
                        amount = _tempBoxes[i].currentAmount,
                        storedItemDays = new List<int>(_tempBoxes[i].storedItemDays),
                        state = _tempBoxes[i].targetState,
                        processType = _tempBoxes[i].targetProcess,
                        qualityScore = _tempBoxes[i].qualityScore
                    });
                }
            }
        }
    }

    // 💡 Mid-Day Load: 상자 상태 복원
    public void RestoreBoxStates(List<IngredientBoxSaveData> savedData)
    {
        if (GameManager.Instance == null || GameManager.Instance.recipeManager == null) return;
        RecipeManager recipeManager = GameManager.Instance.recipeManager;

        // 기존 상자들 모두 초기화 (인벤토리 반환 안 함 - DataManager에서 통으로 로드되므로)
        foreach (var box in _boxes) box.ForceClearBox();
        if (_tempBoxes != null) foreach (var box in _tempBoxes) box.ForceClearBox();

        foreach (var data in savedData)
        {
            IngredientData ingData = recipeManager.GetIngredientById(data.ingredientID);
            if (ingData == null) continue;

            IngredientBoxSetter setter = GetSetterFor(ingData);
            if (setter == null) continue;

            IngredientBox targetBox = null;
            if (!data.isTempBox && data.index >= 0 && data.index < _boxes.Count)
            {
                targetBox = _boxes[data.index];
            }
            else if (data.isTempBox && _tempBoxes != null && data.index >= 0 && data.index < _tempBoxes.Count)
            {
                targetBox = _tempBoxes[data.index];
            }

            if (targetBox != null)
            {
                targetBox.RestoreBoxState(setter, data.state, data.processType, data.qualityScore, data.amount, data.storedItemDays);
            }
        }
        Debug.Log($"<color=cyan>[IngredientManager] 상자 상태({savedData.Count}개) 복원 완료.</color>");
    }
}