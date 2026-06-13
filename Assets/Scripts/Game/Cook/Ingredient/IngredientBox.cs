using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

public class IngredientBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Box Settings")]
    public float capacity = 100f;
    public int currentAmount = 0;
    public float qualityScore = 1.0f; // 💡 가공 품질 (1.0 = 일반, 1.2 = 프리미엄 등)
    IngredientBoxSetter _setter;

    Action RefillEvent;
    Action SetupEvent;

    // 드래그 제어
    private bool _isDraggingItem = false;
    private IngredientObject _spawnedIngredient;
    private ScrollRect _parentScrollRect;

    public void Init(Action onRefill, Action onSetup = null, ScrollRect scrollRect = null)
    {
        RefillEvent = onRefill;
        SetupEvent = onSetup;
        _parentScrollRect = scrollRect;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDraggingItem) return;

        DayPhase phase = DayCycleManager.Instance.CurrentPhase;

        // 준비 단계(Preparation)에서는 재료 세팅/변경 UI를 띄웁니다.
        if (phase == DayPhase.Preparation)
        {
            SetupEvent?.Invoke();
            return;
        }

        // 세팅되지 않은 상자일 경우 처리
        if (_setter == null)
        {
            Debug.Log("<color=yellow>빈 상자입니다. 준비 단계(09시~12시)에서 재료를 세팅하세요.</color>");
            return;
        }

        // 재고가 없을 경우 리필 이벤트
        if (currentAmount <= 0)
        {
            Debug.Log($"<color=red>재료 부족: {_setter.boxData.ingredientName} 상자가 비었습니다!</color>");
            RefillEvent?.Invoke();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Y축 드래그가 X축보다 크고, 준비 단계가 아니며, 상자가 세팅되었고, 재고가 있을 때 아이템 스폰
        DayPhase phase = DayCycleManager.Instance.CurrentPhase;
        if (phase != DayPhase.Preparation && _setter != null && currentAmount > 0 && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        {
            _isDraggingItem = true;
            currentAmount--;
            
            // 화면 좌표를 월드 좌표로 변환
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;

            // 2D 재료 생성
            _spawnedIngredient = Instantiate(_setter.prefabToSpawn, worldPos, Quaternion.identity);
            _spawnedIngredient.SetupIngredient(_setter.boxData);
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
            _spawnedIngredient = null;
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
        ReturnToInventory();

        _setter = data;
        this.qualityScore = quality;
        
        if (amount > 0) {
            currentAmount += InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, amount);
        } else {
            Refill();
        }
        
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.UpdateAvailableRecipes();
        }
    }

    public IngredientData GetCurrentData()
    {
        return _setter != null ? _setter.boxData : null;
    }

    public void ResetBox()
    {
        ReturnToInventory();
        _setter = null;
    }

    public void ReturnToInventory()
    {
        if (currentAmount > 0 && _setter != null && _setter.boxData != null)
        {
            InventoryManager.Instance.AddIngredient(_setter.boxData, currentAmount, 1);
            Debug.Log($"[IngredientBox] {_setter.boxData.ingredientName} {currentAmount}개를 인벤토리로 반환했습니다.");
            currentAmount = 0;
        }
    }

    public void Refill() 
    {
        currentAmount += InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, (int)Math.Floor(capacity / _setter.boxData.volume) - currentAmount);
    }
}