using UnityEngine;

public class CustomerController : PoolableObject, IStateMachine
{
    [Header("Data & UI")]
    public CustomerData currentData;
    // public ProgressBar patienceGauge; // UI 게이지용 (추후 연결)

    // 💡 단일 spriteRenderer를 삭제하고, 파츠별 렌더러 변수를 선언했습니다.
    [Header("Visual Renderers")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer faceRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer legRenderer;

    // FSM 관련
    private BaseState currentState;

    // 런타임 변수
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public float currentPatience;
    [HideInInspector] public FoodData orderedFood;
    [HideInInspector] public Dish receivedDish;

    public override void OnSpawn()
    {
        base.OnSpawn();
        receivedDish = null; // 💡 풀링 사용 시 이전 데이터가 남지 않도록 초기화
        ChangeState(new CustomerEnterState(this));
    }

    private void Update()
    {
        currentState?.Tick();
    }

    // --- IStateMachine 인터페이스 구현 ---
    public void ChangeState(BaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // 요리를 전달받았을 때 호출되는 함수
    public void ReceiveDish(Dish dish)
    {
        if (dish.foodData == orderedFood)
        {
            // 💡 중요: 서빙받은 정보를 저장합니다.
            this.receivedDish = dish;

            Debug.Log($"<color=cyan>[서빙 성공] {currentData.customerName}이(가) 요리를 받았습니다!</color>");
            ChangeState(new CustomerLeaveState(this, true));
        }
        else
        {
            Debug.Log($"<color=red>[서빙 실패] 주문한 요리가 아닙니다!</color>");
        }
    }

    public void SetupCustomer(CustomerData data, ref GenderParts visualParts)
    {
        this.currentData = data;
        this.currentPatience = currentData.maxPatience;

        // 전달받은 성별 맞춤형 파츠에서 랜덤으로 장착
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
        else if (renderer != null)
        {
            //renderer.sprite = null;
        }
    }

    public void UpdateTargetPosition(Vector3 newPos)
    {
        targetPosition = newPos;
    }
}