using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class IngredientBoxSetter
{
    public IngredientObject prefabToSpawn;

    public IngredientData boxData
    { 
        get
        {
            if (prefabToSpawn != null)
            {
                return prefabToSpawn.currentData;
            }
            else
            {
                Debug.LogWarning("<color=yellow>[IngredientBoxSetter] prefabToSpawn이 할당되지 않았습니다.</color>");
                return null;
            }
        }
        set {
            if (prefabToSpawn != null)
            {
                prefabToSpawn.SetupIngredient(value);
            }
            else
            {
                Debug.LogWarning("<color=yellow>[IngredientBoxSetter] prefabToSpawn이 할당되지 않았습니다. 데이터를 설정할 수 없습니다.</color>");
            }
        }
    } // 상자에 세팅할 재료 데이터 (SO)
}

public class IngredientBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
{
    [Header("Box Settings")]
    public float capacity = 100f;
    public int currentAmount = 0;
    public float qualityScore = 1.0f; // 💡 가공 품질 (1.0 = 일반, 1.2 = 프리미엄 등)
    public List<int> storedItemDays = new List<int>(); // 유통기한 보존용

    public IngredientState targetState = IngredientState.Raw;
    public ProcessType targetProcess = ProcessType.None;
    public bool isTemporary = false;

    [Header("UI Elements")]
    public Image iconImage;
    public TMPro.TMP_Text amountText;
    [Tooltip("상자가 비어있을 때 활성화할 게임오브젝트 (예: '+' 버튼이나 빈 박스 이미지)")]
    public GameObject emptyIndicator;

    IngredientBoxSetter _setter;

    Action RefillEvent;
    Action SetupEvent;

    // 드래그 제어
    private bool _isDraggingItem = false;
    private bool _wasDragged = false;
    private IngredientObject _spawnedIngredient;
    private ScrollRect _parentScrollRect;
    private int _draggedItemDays = -1; // 드래그 실패 시 복구용
    
    // 드래그 중 박스가 리셋될 것에 대비한 캐시
    private IngredientBoxSetter _draggedSetter; 
    private IngredientState _draggedState;
    private ProcessType _draggedProcess;
    private float _draggedQuality;

    public void Init(Action onRefill, Action onSetup = null, ScrollRect scrollRect = null)
    {
        RefillEvent = onRefill;
        SetupEvent = onSetup;
        _parentScrollRect = scrollRect;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _wasDragged = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDraggingItem || _wasDragged) return;

        DayPhase phase = DayCycleManager.Instance.CurrentPhase;

        SetupEvent?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _wasDragged = true;

        // Y축 드래그가 X축보다 크고, 준비 단계가 아니며, 상자가 세팅되었고, 재고가 있을 때 아이템 스폰
        DayPhase phase = DayCycleManager.Instance.CurrentPhase;
        if (_setter != null && currentAmount > 0 && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        {
            // 박스 리셋 시점을 대비해 정보를 캐싱
            _draggedSetter = _setter;
            _draggedState = targetState;
            _draggedProcess = targetProcess;
            _draggedQuality = qualityScore;
            
            IngredientObject prefab = _setter.prefabToSpawn;
            IngredientData data = _setter.boxData;
            
            // 인벤토리에서 실시간으로 재고 1개를 소비합니다. (가장 유통기한이 임박한 것 기준)
            ItemGrade targetGrade = qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
            _draggedItemDays = InventoryManager.Instance.UseSpecificIngredient(data.ingredientID, targetState, targetProcess, targetGrade);
            
            if (_draggedItemDays == -1) return; // 재고 부족

            _isDraggingItem = true;
            
            // UseSpecificIngredient 내부에서 마지막 재고 소진 시 
            // CheckAndEmptyBoxesWithoutStock()에 의해 _setter가 null이 될 수 있음
            if (_setter != null)
            {
                currentAmount--;
                UpdateUI();
            }
            
            // 화면 좌표를 월드 좌표로 변환
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;

            // 2D 재료 생성
            _spawnedIngredient = Instantiate(prefab, worldPos, Quaternion.identity);
            _spawnedIngredient.SetupIngredient(data, _draggedState, _draggedProcess);
            _spawnedIngredient.OnTouchBegin(worldPos);

            // 스크롤 이벤트 차단 (옵션)
            if (_parentScrollRect != null)
                _parentScrollRect.OnEndDrag(eventData);
        }
        else
        {
            _isDraggingItem = false;
            if (_parentScrollRect != null)
                _parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDraggingItem && _spawnedIngredient != null)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;
            _spawnedIngredient.OnTouchDrag(worldPos);
        }
        else if (!_isDraggingItem)
        {
            if (_parentScrollRect != null)
                _parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDraggingItem && _spawnedIngredient != null)
        {
            _spawnedIngredient.OnTouchEnd();
            
            // 허공에 떨어졌거나 유효하지 않은 곳에 놓였을 때
            if (!_spawnedIngredient.wasDroppedSuccessfully)
            {
                _spawnedIngredient.OnDespawn(); // 사라지게 만듦
                
                // 만약 드래그 도중 마지막 재료여서 상자가 빈 상자로 초기화되었다면 원상 복구합니다.
                if (_setter == null && _draggedSetter != null)
                {
                    _setter = _draggedSetter;
                    targetState = _draggedState;
                    targetProcess = _draggedProcess;
                    qualityScore = _draggedQuality;
                    currentAmount = 0; // 아래에서 1 더해짐
                }
                
                if (_setter != null)
                {
                    currentAmount++;
                }
                
                if (_draggedItemDays != -1 && _draggedSetter != null)
                {
                    ItemGrade targetGrade = _draggedQuality >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                    InventoryManager.Instance.AddIngredient(_draggedSetter.boxData, 1, _draggedItemDays, _draggedState, _draggedProcess, targetGrade); // 유통기한 및 상세 정보 복구
                }
                
                if (_setter != null)
                {
                    UpdateUI(); // 상자에 다시 들어간 것을 UI에 반영
                }
            }
            else
            {
                // 사용 완료 시 수량이 0이고 임시 바트라면 즉시 초기화
                if (isTemporary && currentAmount <= 0)
                {
                    ResetBox();
                }
            }
            
            _spawnedIngredient = null;
            _draggedItemDays = -1;
        }
        else if (!_isDraggingItem)
        {
            if (_parentScrollRect != null)
                _parentScrollRect.OnEndDrag(eventData);
        }
        
        _isDraggingItem = false;
    }

    public void SetupIngredient(IngredientBoxSetter data, float quality = 1.0f, int amount = -1) 
    {
        // 💡 물리적 아이템 이동 없음 (인벤토리에 그대로 둠)
        // 기존에 할당되어 있었다면 초기화 (환수할 게 없음)
        _setter = data;
        this.qualityScore = quality;
        this.storedItemDays.Clear(); // 사용하지 않음
        
        if (amount > 0)
        {
            this.currentAmount = amount;
            UpdateUI();
        }
        else
        {
            Refill(); // 남은 재고를 확인하여 가득 채움
        }
    }

    public IngredientData GetCurrentData()
    {
        return _setter != null ? _setter.boxData : null;
    }

    public void ResetBox()
    {
        // 물리적으로 아이템을 뺀 적이 없으므로 환수(ReturnToInventory) 생략
        _setter = null;
        currentAmount = 0;
        targetState = IngredientState.Raw;
        targetProcess = ProcessType.None;
        UpdateUI();
    }

    public void SetupFromCollectedItem(IngredientBoxSetter data, IngredientState state, ProcessType processType, float quality, int amount)
    {
        _setter = data;
        this.targetState = state;
        this.targetProcess = processType;
        this.qualityScore = quality;
        this.currentAmount += amount;
        
        UpdateUI();
    }

    public void AddCollectedItem(int amount, float quality, int shelfLifeDays)
    {
        this.currentAmount += amount;
        this.qualityScore = quality;
        // 이제 인벤토리에 직접 아이템을 넣습니다 (박스 내 임시 보관 없음)
        if (_setter != null && _setter.boxData != null)
        {
            ItemGrade grade = quality >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
            InventoryManager.Instance.AddIngredient(_setter.boxData, amount, shelfLifeDays, targetState, targetProcess, grade);
        }
        UpdateUI();
    }

    public void ReturnToInventory()
    {
        // 더 이상 물리적 환수를 할 필요가 없습니다 (이미 Inventory에 있음)
        currentAmount = 0;
        storedItemDays.Clear();
        UpdateUI();
    }

    public void Refill() 
    {
        // 이젠 물리적 이동이 없으므로, 현재 인벤토리에 남은 수량을 기반으로 표시 수량만 갱신
        if (_setter != null && _setter.boxData != null)
        {
            ItemGrade grade = qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
            int availableAmount = InventoryManager.Instance.GetTotalSpecificAmount(_setter.boxData.ingredientID, targetState, targetProcess, grade);
            
            // capacity 제한만큼 할당
            int targetAmount = (int)Math.Floor(capacity / _setter.boxData.volume);
            currentAmount = Math.Min(targetAmount, availableAmount);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        bool isEmpty = (_setter == null || _setter.boxData == null || currentAmount <= 0);

        if (emptyIndicator != null)
        {
            emptyIndicator.SetActive(isEmpty);
        }

        if (iconImage != null)
        {
            if (!isEmpty)
            {
                Sprite displaySprite = _setter.boxData.ingredientSprite;
                
                if (targetProcess != ProcessType.None)
                {
                    ProcessMethodData method = _setter.boxData.GetProcessMethod(targetProcess);
                    if (method != null && method.stateSteps != null)
                    {
                        foreach (var step in method.stateSteps)
                        {
                            if (step.state == targetState)
                            {
                                displaySprite = step.stateSprite;
                                break;
                            }
                        }
                    }
                }

                iconImage.sprite = displaySprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        
        if (amountText != null)
        {
            if (!isEmpty)
            {
                ItemGrade grade = qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
                int availableStock = InventoryManager.Instance != null ? InventoryManager.Instance.GetTotalSpecificAmount(_setter.boxData.ingredientID, targetState, targetProcess, grade) : 0;
                // 할당된 수량과 실제 남은 재고 중 작은 값을 표시
                int displayAmount = Math.Min(currentAmount, availableStock);
                
                // 만약 현재 할당량보다 재고가 적어지면 할당량을 실제 재고로 맞춤
                if (currentAmount > availableStock) {
                    currentAmount = availableStock;
                }
                
                amountText.text = displayAmount.ToString();
            }
            else
            {
                amountText.text = "";
            }
        }
    }
}