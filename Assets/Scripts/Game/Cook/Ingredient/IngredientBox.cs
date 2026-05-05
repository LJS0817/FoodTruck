using System;
using UnityEngine;

[Serializable]
public class IngredientBoxSetter
{
    public IngredientData boxData;
    public IngredientObject prefabToSpawn;
}

[RequireComponent(typeof(BoxCollider2D))]
public class IngredientBox : MonoBehaviour, IInteractable
{
    [Header("Box Settings")]
    public Transform spawnPoint;          // 재료가 등장할 위치
    public float capacity = 100f;
    public int currentAmount = 0;
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
        // 1. 세팅되지 않은 빈 상자일 경우
        if (_setter == null)
        {
            Debug.Log("<color=yellow>빈 상자입니다. 인벤토리를 열어 재료를 세팅하세요.</color>");
            SetupEvent?.Invoke();
        }
        // 2. 인벤토리에 재고가 있는지 확인
        else if (currentAmount > 0)
        {
            return SpawnIngredient(touchPosition);
        }
        // 3. 재고가 없을 경우 리필 요구
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

    public void SetupIngredient(IngredientBoxSetter data) {
        _setter = data;
        Refill();
    }

    public IngredientData GetCurrentData()
    {
        return _setter != null ? _setter.boxData : null;
    }

    private IInteractable SpawnIngredient(Vector2 touchPosition)
    {
        currentAmount--;
        InventoryManager.Instance.UseIngredient(_setter.boxData.ingredientID);
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
        currentAmount = 0;
    }

    public void Refill() {
        currentAmount = InventoryManager.Instance.FillIngredient(_setter.boxData.ingredientID, (int)Math.Floor(capacity / _setter.boxData.volume));
    }
}