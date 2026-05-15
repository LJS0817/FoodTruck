using System;
using UnityEngine;

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

[RequireComponent(typeof(BoxCollider2D))]
public class IngredientBox : MonoBehaviour, IInteractable
{
    [Header("Box Settings")]
    public Transform spawnPoint;          // 재료가 등장할 위치
    public float capacity = 100f;
    public int currentAmount = 0;
    public float qualityScore = 1.0f; // 💡 가공 품질 (1.0 = 일반, 1.2 = 프리미엄 등)
    IngredientBoxSetter _setter;

    Action RefillEvent;
    Action SetupEvent; // 💡 세팅되지 않은 상자를 터치했을 때 호출할 이벤트

    public void Init(Action onRefill, Action onSetup = null)
    {
        RefillEvent = onRefill;
        SetupEvent = onSetup;
    }

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        // 💡 페이즈에 따른 조작 분기
        DayPhase phase = DayCycleManager.Instance.CurrentPhase;

        // 1. 준비 단계(Preparation)에서는 재료 세팅/변경 UI를 띄웁니다.
        if (phase == DayPhase.Preparation)
        {
            SetupEvent?.Invoke();
            return this;
        }

        // 2. 그 외 단계(Business 등)에서는 기존 로직 수행
        
        // 세팅되지 않은 빈 상자일 경우
        if (_setter == null)
        {
            Debug.Log("<color=yellow>빈 상자입니다. 준비 단계(09시~12시)에서 재료를 세팅하세요.</color>");
        }
        // 인벤토리에 재고가 있는지 확인
        else if (currentAmount > 0)
        {
            return SpawnIngredient(touchPosition);
        }
        // 재고가 없을 경우 리필 요구
        else
        {
            Debug.Log($"<color=red>재료 부족: {_setter.boxData.ingredientName} 상자가 비었습니다!</color>");
            RefillEvent?.Invoke();
        }
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition)
    {
        // 상자 자체는 드래그되지 않으므로 비워둡니다.
    }

    public void OnTouchEnd()
    {
        // 터치 종료 로직
    }

    public void SetupIngredient(IngredientBoxSetter data, float quality = 1.0f, int amount = -1) {
        // 💡 기존에 있던 재료가 있다면 인벤토리로 반환
        ReturnToInventory();

        _setter = data;
        this.qualityScore = quality;
        
        if (amount > 0) {
            currentAmount += InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, amount);
        } else {
            Refill();
        }
        
        // 재료가 바뀌었으므로 판매 가능 레시피 갱신
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.UpdateAvailableRecipes();
        }
    }

    public IngredientData GetCurrentData()
    {
        return _setter != null ? _setter.boxData : null;
    }

    private IInteractable SpawnIngredient(Vector2 touchPosition)
    {
        currentAmount--;
        // 💡 인벤토리에서 차감하는 로직은 이미 상자를 채울 때(FillIngredient) 수행되므로 여기서 호출하지 않습니다.
        
        // 2. 재료 생성 
        // (현재는 Instantiate지만, 최적화를 위해 이 부분도 반드시 Object Pool에서 꺼내오도록 수정해야 합니다)
        IngredientObject newIngredient = Instantiate(_setter.prefabToSpawn, touchPosition, Quaternion.identity);

        // 3. 생성된 재료에 데이터 덮어씌우기
        newIngredient.SetupIngredient(_setter.boxData);
        return newIngredient.OnTouchBegin(touchPosition);

        // 등장 시 약간의 애니메이션 효과 (Scale 튕김 등) 추가 가능
    }

    public void ResetBox()
    {
        ReturnToInventory();
        _setter = null;
    }

    // 💡 상자에 남은 재료를 다시 인벤토리로 반환합니다.
    public void ReturnToInventory()
    {
        if (currentAmount > 0 && _setter != null && _setter.boxData != null)
        {
            // 상자에서 다시 꺼낸 재료는 어뷰징 방지를 위해 유통기한을 1일로 설정하여 반환합니다.
            InventoryManager.Instance.AddIngredient(_setter.boxData, currentAmount, 1);
            Debug.Log($"[IngredientBox] {_setter.boxData.ingredientName} {currentAmount}개를 인벤토리로 반환했습니다.");
            currentAmount = 0;
        }
    }

    public void Refill() {
        currentAmount += InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, (int)Math.Floor(capacity / _setter.boxData.volume) - currentAmount);
    }
}