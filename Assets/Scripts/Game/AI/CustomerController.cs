using UnityEngine;

public class CustomerController : PoolableObject, IStateMachine
{
    [Header("Data & UI")]
    public CustomerData currentData;

    [Header("Visual Renderers")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer faceRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer legRenderer;

    private BaseState currentState;

    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public float currentPatience;

    // 💡 수정 1: FoodData 객체 대신 메뉴의 이름을 저장합니다.
    [HideInInspector] public string orderedFoodName;
    [HideInInspector] public Dish receivedDish;

    public override void OnSpawn()
    {
        base.OnSpawn();
        receivedDish = null;
        ChangeState(new CustomerEnterState(this));
    }

    private void Update()
    {
        currentState?.Tick();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void ReceiveDish(Dish dish)
    {
        // 💡 수정 2: 요리 이름(string)을 비교하여 일치하는지 확인합니다.
        if (dish.foodName == orderedFoodName)
        {
            this.receivedDish = dish;
            Debug.Log($"<color=cyan>[서빙 성공] {currentData.customerName}이(가) {dish.foodName} 요리를 받았습니다!</color>");
            ChangeState(new CustomerLeaveState(this, true));
        }
        else
        {
            Debug.Log($"<color=red>[서빙 실패] 주문한 요리가 아닙니다! (주문: {orderedFoodName} / 서빙: {dish.foodName})</color>");
        }
    }

    public void SetupCustomer(CustomerData data, ref GenderParts visualParts)
    {
        this.currentData = data;
        this.currentPatience = currentData.maxPatience;

        EquipRandomPart(headRenderer, visualParts.headParts);
        EquipRandomPart(faceRenderer, visualParts.faceParts);
        EquipRandomPart(bodyRenderer, visualParts.bodyParts);
        EquipRandomPart(legRenderer, visualParts.legParts);
    }

    private void EquipRandomPart(SpriteRenderer renderer, Sprite[] parts)
    {
        if (renderer != null && parts != null && parts.Length > 0)
        {
            int randomIndex = Random.Range(0, parts.Length);
            renderer.sprite = parts[randomIndex];
        }
    }

    public void UpdateTargetPosition(Vector3 newPos)
    {
        targetPosition = newPos;
    }
}