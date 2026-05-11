using System.Collections.Generic;
using UnityEngine;

// 1. 입장 상태: 스폰 지점에서 트럭 앞(또는 웨이팅 줄)까지 걸어갑니다.
public class CustomerEnterState : BaseState
{
    private CustomerController controller;

    public CustomerEnterState(CustomerController controller) : base(controller)
    {
        this.controller = controller;
    }

    public override void Enter() { }

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
        // 💡 MenuManager에서 현재 판매 가능한 레시피 목록을 가져옵니다.
        List<FoodData> available = MenuManager.Instance != null ? MenuManager.Instance.GetAvailableRecipes() : null;

        if (available == null || available.Count == 0)
        {
            Debug.LogWarning("[CustomerWaitState] 판매 가능한 메뉴가 없습니다! 기본 데이터베이스에서 무작위 선택합니다.");
            int idx = Random.Range(0, GameManager.Instance.recipeManager.allFoodDatabase.Count);
            controller.orderedFood = GameManager.Instance.recipeManager.allFoodDatabase[idx];
        }
        else
        {
            controller.orderedFood = SelectMenuByPreference(available);
        }

        // 💡 WaitState 진입 시 인내심 감소 시작 (PatienceController에 위임)
        controller.PatienceController?.StartDecreasing();
    }

    /// <summary>
    /// 손님의 선호도에 따라 메뉴를 선택합니다.
    /// 우선순위: ① 선호 음식(favoriteFoods) → ② 선호 맛 태그(preferredFlavors) → ③ 무작위
    /// </summary>
    private FoodData SelectMenuByPreference(List<FoodData> available)
    {
        // 1단계: 구체적 선호 음식이 판매 중인지 확인
        FoodData[] favorites = controller.currentData.favoriteFoods;
        if (favorites != null && favorites.Length > 0)
        {
            List<FoodData> playableFavorites = new List<FoodData>();
            for (int i = 0; i < favorites.Length; i++)
            {
                if (available.Contains(favorites[i]))
                    playableFavorites.Add(favorites[i]);
            }
            if (playableFavorites.Count > 0)
                return playableFavorites[Random.Range(0, playableFavorites.Count)];
        }

        // 2단계: 선호 맛 태그(FlavorTag)와 일치하는 메뉴 탐색
        FlavorTag[] preferred = controller.currentData.preferredFlavors;
        if (preferred != null && preferred.Length > 0)
        {
            List<FoodData> flavorMatches = new List<FoodData>();
            for (int i = 0; i < available.Count; i++)
            {
                if (available[i].flavorTags == null) continue;
                for (int j = 0; j < preferred.Length; j++)
                {
                    if (available[i].flavorTags.Contains(preferred[j]))
                    {
                        flavorMatches.Add(available[i]);
                        break; // 하나라도 맞으면 후보에 추가
                    }
                }
            }
            if (flavorMatches.Count > 0)
                return flavorMatches[Random.Range(0, flavorMatches.Count)];
        }

        // 3단계: 아무것도 안 맞으면 전체에서 무작위
        return available[Random.Range(0, available.Count)];
    }

    public override void Tick()
    {
        // 💡 대기 중에도 목표 지점과 거리가 멀다면 계속 이동합니다 (앞으로 당겨 앉기)
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            controller.targetPosition,
            controller.currentData.walkSpeed * Time.deltaTime
        );

        // 주문 시도 (한 번만)
        if (!hasOrdered)
        {
            if (OrderManager.Instance.TryAddOrder(controller, controller.orderedFood))
                hasOrdered = true;
        }

        // 💡 주문 후 인내심 UI 업데이트 (OrderTicket 게이지 갱신)
        if (hasOrdered)
        {
            controller.UpdatePatience();
        }

        // 💡 인내심 바닥 체크
        if (controller.PatienceController != null && controller.PatienceController.IsOut)
        {
            if (hasOrdered)
                OrderManager.Instance.CancelOrderOf(controller);

            controller.ChangeState(new CustomerLeaveState(controller, false));
        }
    }

    public override void Exit()
    {
        // 💡 상태 종료 시 인내심 UI 숨기기
        controller.PatienceController?.StopDecreasing();
    }
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
        // 💡 핵심: 퇴장 상태에 진입하자마자 매니저에게 명단에서 빼달라고 보고합니다.
        CustomerManager.Instance.LeaveQueue(controller);

        if (isSatisfied)
        {
            int finalPrice = controller.orderedFood.basePrice;

            // 💡 유행 및 날씨에 따른 보너스 배율 적용
            if (WeatherTrendManager.Instance != null)
            {
                float trendMult = WeatherTrendManager.Instance.GetTrendMultiplier(controller.orderedFood);
                if (trendMult > 1.0f)
                {
                    finalPrice = Mathf.RoundToInt(finalPrice * trendMult);
                    Debug.Log($"[유행 보너스] {controller.orderedFood.foodName} 유행 배율 {trendMult}배 적용!");
                }
            }

            if (HygieneManager.Instance != null)
            {
                HygieneManager.Instance.DropHygiene();
            }

            // 💡 프리미엄 요리이면 가격 할증 (기본 1.5배 * 돌발 이벤트)
            bool isPremium = controller.receivedDish != null && controller.receivedDish.isPremium;
            if (isPremium)
            {
                float premiumMult = 1.5f;
                if (RandomEventManager.Instance != null)
                    premiumMult *= RandomEventManager.Instance.GetPremiumTipMultiplier();

                finalPrice = Mathf.RoundToInt(finalPrice * premiumMult);
                Debug.Log($"[팁 획득] 프리미엄 요리로 {finalPrice}원 획득! (배율: {premiumMult})");
            }

            // 💡 VIP 손님이면 호감도(단골) 시스템 연동 및 추가 팁 배율 적용
            if (controller.currentData.isVIP)
            {
                string vipName = controller.currentData.customerName;

                // 1. 기본 VIP 배율
                float vipMult = controller.currentData.vipTipMultiplier;

                // 2. 단골(호감도) 기반 추가 배율
                if (VIPLoyaltyManager.Instance != null)
                {
                    vipMult *= VIPLoyaltyManager.Instance.GetLoyaltyTipMultiplier(vipName);
                    // 서빙 성공 시 호감도 상승
                    VIPLoyaltyManager.Instance.AddLoyalty(vipName, 1);
                }

                finalPrice = Mathf.RoundToInt(finalPrice * vipMult);
                Debug.Log($"<color=yellow>[VIP 팁] {vipName} VIP 배율 {vipMult}배 적용! 최종: {finalPrice}원</color>");
            }

            // 💡 사장님의 장착 칭호 보너스 배율 적용
            if (AchievementManager.Instance != null)
            {
                float titleMult = AchievementManager.Instance.GetEquippedTitleTipMultiplier();
                if (titleMult > 1.0f)
                {
                    finalPrice = Mathf.RoundToInt(finalPrice * titleMult);
                    Debug.Log($"<color=#00FFFF>[칭호 보너스] 장착한 칭호 덕분에 팁 배율 {titleMult}배 적용! 최종: {finalPrice}원</color>");
                }
            }

            PlayerManager.Instance.AddMoney(finalPrice);

            // 💡 정산 매니저에게 오늘 매출 + 메뉴 이름 보고
            if (SettlementManager.Instance != null)
            {
                string menuName = controller.orderedFood != null ? controller.orderedFood.foodName : string.Empty;
                SettlementManager.Instance.AddSales(finalPrice, isPremium, menuName);
            }

            // 💡 평판 상승
            ReputationManager.Instance?.OnServed(isPremium);

            // TODO: 기뻐하는 표정
        }
        else
        {
            // 💡 평판 하락 (화나서 이탈)
            ReputationManager.Instance?.OnCustomerLeft();

            // 💡 잃어버린 손님 수 카운트 증가 (리뷰에 반영)
            if (SettlementManager.Instance != null)
            {
                SettlementManager.Instance.AddLostCustomer();
            }

            // TODO: 화내는 표정
        }

        // 화면 밖으로 퇴장할 목표 지점 설정
        exitPosition = new Vector3(10f, controller.transform.position.y, 0);
    }

    public override void Tick()
    {
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            exitPosition,
            controller.currentData.walkSpeed * Time.deltaTime
        );

        if (Vector3.Distance(controller.transform.position, exitPosition) < 0.01f)
        {
            CustomerManager.Instance.ReturnToPool(controller);
        }
    }

    public override void Exit() { }
}