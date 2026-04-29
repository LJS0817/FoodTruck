using UnityEngine;

// 1. 입장 상태: 스폰 지점에서 트럭 앞(또는 웨이팅 줄)까지 걸어갑니다.
public class CustomerEnterState : BaseState
{
    private CustomerController controller;
    private Vector3 targetPosition;

    public CustomerEnterState(CustomerController controller) : base(controller)
    {
        this.controller = controller;
    }

    public override void Enter()
    {
        // 대기열 매니저(미구현)에게 내 자리를 물어보고 목표 좌표를 설정합니다.
        // 임시로 트럭 앞 좌표를 하드코딩
        //targetPosition = CustomerManager.Instance.GetQueuePosition(controller);
    }

    public override void Tick()
    {
        // 컨트롤러에 저장된 최신 목표 지점으로 이동
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            controller.targetPosition,
            controller.currentData.walkSpeed * Time.deltaTime
        );

        if (Vector3.Distance(controller.transform.position, controller.targetPosition) < 0.01f)
        {
            controller.ChangeState(new CustomerWaitState(controller));
        }
    }

    public override void Exit() { }
}

// 2. 대기 및 주문 상태: 줄을 서서 인내심이 깎이며, 0이 되면 화를 내며 돌아갑니다.
public class CustomerWaitState : BaseState
{
    private CustomerController controller;
    private bool hasOrdered = false;

    public CustomerWaitState(CustomerController controller) : base(controller)
    {
        this.controller = controller;
    }

    public override void Enter()
    {
        // 머릿속으로 먹고 싶은 메뉴만 미리 정해둡니다.
        if (controller.currentData.favoriteFoods.Length > 0)
        {
            int randomIndex = Random.Range(0, controller.currentData.favoriteFoods.Length);
            controller.orderedFood = controller.currentData.favoriteFoods[randomIndex];
        }
    }

    public override void Tick()
    {
        // 💡 대기 중에도 목표 지점과 거리가 멀다면 계속 이동합니다 (앞으로 당겨 앉기)
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            controller.targetPosition,
            controller.currentData.walkSpeed * Time.deltaTime
        );

        // 인내심 감소 및 주문 로직은 그대로 유지
        controller.currentPatience -= Time.deltaTime;

        if (!hasOrdered)
        {
            if (OrderManager.Instance.TryAddOrder(controller, controller.orderedFood))
            {
                hasOrdered = true;
            }
        }

        // 인내심 바닥 시 퇴장
        if (controller.currentPatience <= 0f)
        {
            // 💡 앗, 화나서 돌아가기 전에 내 주문표 빼주세요!
            if (hasOrdered)
            {
                OrderManager.Instance.CancelOrderOf(controller);
            }

            controller.ChangeState(new CustomerLeaveState(controller, false));
        }
    }

    public override void Exit() { }
}

// 3. 퇴장 상태: 서빙을 받았거나 인내심이 바닥나서 화면 밖으로 나갑니다.
public class CustomerLeaveState : BaseState
{
    private CustomerController controller;
    private bool isSatisfied;
    private Vector3 exitPosition;

    public CustomerLeaveState(CustomerController controller, bool success) : base(controller)
    {
        this.controller = controller;
        this.isSatisfied = success;
    }

    public override void Enter()
    {
        // 💡 핵심 추가: 퇴장 상태에 진입하자마자 매니저에게 명단에서 빼달라고 보고합니다.
        CustomerManager.Instance.LeaveQueue(controller);

        if (isSatisfied)
        {
            int finalPrice = controller.orderedFood.basePrice;

            // 💡 방금 받은 요리가 프리미엄이라면 가격 할증 (예: 1.5배)
            // (서빙 시 Dish 객체의 정보를 컨트롤러 어딘가에 임시 저장해두거나 직접 확인합니다)
            if (controller.receivedDish != null && controller.receivedDish.isPremium)
            {
                finalPrice = Mathf.RoundToInt(finalPrice * 1.5f);
                Debug.Log($"[팁 획득] 프리미엄 요리로 {finalPrice}원 획득!");
            }

            PlayerManager.Instance.AddMoney(finalPrice);
            // TODO: 기뻐하는 표정
        }
        else
        {
            // TODO: 화내는 표정
        }

        // 화면 밖으로 퇴장할 목표 지점 설정 (오른쪽으로 나간다고 가정)
        exitPosition = new Vector3(10f, controller.transform.position.y, 0);
    }

    public override void Tick()
    {
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            exitPosition,
            controller.currentData.walkSpeed * Time.deltaTime
        );

        // 화면 밖으로 완전히 나가면 매니저를 통해 풀(Pool)로 복귀
        if (Vector3.Distance(controller.transform.position, exitPosition) < 0.01f)
        {
            // 💡 기존의 controller.OnDespawn() 대신 매니저의 함수를 호출합니다.
            CustomerManager.Instance.ReturnToPool(controller);
        }
    }

    public override void Exit() { }
}