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

public class IngredientBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Box Settings")]
    public float capacity = 100f;
    public int currentAmount = 0;
    public float qualityScore = 1.0f; // 💡 가공 품질 (1.0 = 일반, 1.2 = 프리미엄 등)
    public List<int> storedItemDays = new List<int>(); // 유통기한 보존용

    [Header("UI Elements")]
    public Image iconImage;
    public TMPro.TMP_Text amountText;

    IngredientBoxSetter _setter;

    Action RefillEvent;
    Action SetupEvent;

    // 드래그 제어
    private bool _isDraggingItem = false;
    private IngredientObject _spawnedIngredient;
    private ScrollRect _parentScrollRect;
    private int _draggedItemDays = -1; // 드래그 실패 시 복구용

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

        SetupEvent?.Invoke();
        // if (phase == DayPhase.Preparation)
        // {
        //     SetupEvent?.Invoke();
        //     return;
        // }

        // // 재고가 없을 경우 리필 이벤트
        // if (currentAmount <= 0)
        // {
        //     Debug.Log($"<color=red>재료 부족: {_setter.boxData.ingredientName} 상자가 비었습니다!</color>");
        //     RefillEvent?.Invoke();
        // }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Y축 드래그가 X축보다 크고, 준비 단계가 아니며, 상자가 세팅되었고, 재고가 있을 때 아이템 스폰
        DayPhase phase = DayCycleManager.Instance.CurrentPhase;
        // if (phase != DayPhase.Preparation && _setter != null && currentAmount > 0 && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        if (_setter != null && currentAmount > 0 && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        {
            _isDraggingItem = true;
            currentAmount--;
            
            _draggedItemDays = -1;
            if (storedItemDays.Count > 0)
            {
                _draggedItemDays = storedItemDays[0];
                storedItemDays.RemoveAt(0); // 첫 번째 재료 추출
            }
            
            UpdateUI();
            
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
            
            // 허공에 떨어졌거나 유효하지 않은 곳에 놓였을 때
            if (!_spawnedIngredient.wasDroppedSuccessfully)
            {
                _spawnedIngredient.OnDespawn(); // 사라지게 만듦
                currentAmount++;
                if (_draggedItemDays != -1)
                {
                    storedItemDays.Add(_draggedItemDays); // 유통기한 복구
                }
                UpdateUI(); // 상자에 다시 들어간 것을 UI에 반영
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
        ReturnToInventory();

        _setter = data;
        this.qualityScore = quality;
        
        if (amount > 0) {
            List<int> filled = InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, amount);
            currentAmount += filled.Count;
            storedItemDays.AddRange(filled);
        } else {
            Refill();
        }
        
        UpdateUI();
        
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
        UpdateUI();
    }

    public void ReturnToInventory()
    {
        if (currentAmount > 0 && _setter != null && _setter.boxData != null)
        {
            foreach (int days in storedItemDays)
            {
                InventoryManager.Instance.AddIngredient(_setter.boxData, 1, days);
            }
            Debug.Log($"[IngredientBox] {_setter.boxData.ingredientName} {currentAmount}개를 인벤토리로 반환했습니다.");
            currentAmount = 0;
            storedItemDays.Clear();
            UpdateUI();
        }
    }

    public void Refill() 
    {
        int targetAmount = (int)Math.Floor(capacity / _setter.boxData.volume) - currentAmount;
        if (targetAmount > 0)
        {
            List<int> filled = InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, targetAmount);
            currentAmount += filled.Count;
            storedItemDays.AddRange(filled);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (iconImage != null)
        {
            if (_setter != null && _setter.boxData != null && currentAmount > 0)
            {
                iconImage.sprite = _setter.boxData.ingredientSprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        
        if (amountText != null)
        {
            if (_setter != null && _setter.boxData != null && currentAmount > 0)
            {
                amountText.text = currentAmount.ToString();
            }
            else
            {
                amountText.text = "";
            }
        }
    }
}