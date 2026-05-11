using UnityEngine;

[RequireComponent(typeof(CustomerPatienceController))]
public class CustomerController : PoolableObject, IStateMachine
{
    [Header("Data & UI")]
    public CustomerData currentData;
    (OrderTicket, OrderTicket) _tickets;

    // 인내심 컨트롤러: 이 손님 GameObject에 자동으로 부착됩니다.
    [HideInInspector] public CustomerPatienceController PatienceController { get; private set; }

    // 💡 단일 spriteRenderer를 삭제하고, 파츠별 렌더러 변수를 선언했습니다.
    [Header("Visual Renderers")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer faceRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer legRenderer;

    [Header("VIP Visual")]
    [SerializeField] private GameObject vipIndicator; // VIP 반짝이 아이콘 (프리팹에 배치)

    // FSM 관련
    private BaseState currentState;

    // 런타임 변수
    [HideInInspector] public Vector3 targetPosition;
    // currentPatience는 PatienceController를 통해 접근합니다.
    public float currentPatience => PatienceController != null ? PatienceController.CurrentPatience : 0f;
    [HideInInspector] public FoodData orderedFood;
    [HideInInspector] public Dish receivedDish;

    protected override void Awake()
    {
        base.Awake();
        PatienceController = GetComponent<CustomerPatienceController>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        receivedDish = null; // 💡 풀링 사용 시 이전 데이터가 남지 않도록 초기화
        PatienceController?.ResetState();
        ChangeState(new CustomerEnterState(this));
    }

    public bool TrySetPatience((OrderTicket, OrderTicket) ui)
    {
        _tickets = ui;
        return _tickets.Item1 != null && _tickets.Item2 != null;
    }

    public void RemoveOrder()
    {
        _tickets.Item1?.HideUI();
        _tickets.Item2?.HideUI();
    }

    private void Update()
    {
        currentState?.Tick();
    }

    public void UpdatePatience()
    {
        float patience = PatienceController != null ? PatienceController.CurrentPatience : 0f;
        _tickets.Item1?.UpdatePatience(patience);
        _tickets.Item2?.UpdatePatience(patience);
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
        if (dish != null && dish.foodData == orderedFood)
        {
            // 💡 중요: 서빙받은 정보를 저장합니다.
            this.receivedDish = dish;

            Debug.Log($"<color=cyan>[서빙 성공] {currentData.customerName}이(가) 요리를 받았습니다!</color>");
            ChangeState(new CustomerLeaveState(this, true));
        }
        else
        {
            Debug.Log($"<color=red>[서빙 실패] 주문한 요리가 아닙니다!</color>");
            ChangeState(new CustomerLeaveState(this, false));
        }

        _tickets.Item1 = null;
        _tickets.Item2 = null;
    }

    public void SetupCustomer(CustomerData data, ref GenderParts visualParts)
    {
        currentData = data;
        // 인내심 초기화는 WaitState 진입 시 PatienceController.StartDecreasing()에서 수행합니다.

        // 💡 VIP 시각 효과 토글
        if (vipIndicator != null)
            vipIndicator.SetActive(data.isVIP);

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

    public void WorkOnOrder()
    {
        _tickets.Item2?.SetProcessingState(true);
    }
}