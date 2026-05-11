using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Pooling Settings")]
    public CustomerController basePrefab;
    public CustomerData[] availableCustomerTypes;
    public int poolSizePerType = 5;
    public CustomerAppearanceDB appearanceDB;

    private Queue<CustomerController> customerPool = new Queue<CustomerController>();
    private List<CustomerController> activeCustomers = new List<CustomerController>(20);

    [Header("Spawn Settings")]
    public Transform spawnPoint;      // 화면 밖 스폰 지점
    public float spawnInterval = 4f;  // 스폰 주기 (초)
    private float spawnTimer = 0f;

    [Header("Queue Settings")]
    public Transform truckCounter;    // 트럭 주문대 위치 (줄의 시작점)
    public float queueSpacing = 1.5f; // 손님 간의 줄서기 간격

    private void Awake()
    {
        if (Instance == null) Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        // 총 풀 사이즈 = 데이터 종류 개수 * 종류당 할당할 배수
        int totalPoolSize = availableCustomerTypes.Length * poolSizePerType;

        // activeCustomers 리스트도 최대치에 맞춰 Capacity를 미리 할당 (GC 최적화)
        activeCustomers.Capacity = totalPoolSize;

        for (int i = 0; i < totalPoolSize; i++)
        {
            CustomerController newCustomer = Instantiate(basePrefab, transform);
            newCustomer.gameObject.SetActive(false);
            customerPool.Enqueue(newCustomer);
        }

        Debug.Log($"<color=cyan>[CustomerManager] 총 {totalPoolSize}개의 통합 손님 껍데기 풀링 완료 (종류 {availableCustomerTypes.Length}개 * {poolSizePerType}배수).</color>");
    }

    private void Update()
    {
        // 💡 장사 시작(Open) 상태일 때만 손님 스폰 타이머 작동
        if (BusinessManager.Instance != null && !BusinessManager.Instance.IsBusinessOpen)
            return;

        // 간단한 스폰 타이머 로직
        float multiplier = WeatherTrendManager.Instance != null ? WeatherTrendManager.Instance.GetSpawnRateMultiplier() : 1f;

        // 💡 평판, 마케팅, 알바생에 따른 스폰 배율 추가 적용
        if (ReputationManager.Instance != null)
            multiplier *= ReputationManager.Instance.GetSpawnRateMultiplier();
        
        if (MarketingManager.Instance != null)
            multiplier *= MarketingManager.Instance.GetSpawnBoostMultiplier();

        if (WorkerManager.Instance != null)
            multiplier += WorkerManager.Instance.GetAbilityTotalValue(WorkerAbility.SpawnRateBoost); // 합산 방식으로 추가 배율

        // 💡 돌발 이벤트 보정
        if (RandomEventManager.Instance != null)
            multiplier *= RandomEventManager.Instance.GetSpawnMultiplier();

        spawnTimer += Time.deltaTime * multiplier;
        
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnCustomer();
        }
    }

    // 1. 손님 스폰 및 풀링 관리
    private void SpawnCustomer()
    {
        // 💡 현재 구역(District)의 손님 풀 가져오기 (없으면 기본값 사용)
        CustomerData[] currentPool = availableCustomerTypes;
        if (DistrictManager.Instance != null && DistrictManager.Instance.CurrentDistrict != null)
        {
            currentPool = DistrictManager.Instance.CurrentDistrict.districtCustomerTypes;
        }

        if (currentPool == null || currentPool.Length == 0 || customerPool.Count == 0 || appearanceDB == null) return;

        // 💡 VIP 등장 확률 체크 (평판 + 마케팅 보너스 + 돌발 이벤트)
        CustomerData selectedData = null;
        float vipChance = ReputationManager.Instance != null ? ReputationManager.Instance.GetVIPChance() : 0f;
        
        if (MarketingManager.Instance != null)
            vipChance += MarketingManager.Instance.GetVIPBoostBonus();

        if (RandomEventManager.Instance != null)
            vipChance += RandomEventManager.Instance.GetVIPChanceBonus();

        if (vipChance > 0f && Random.value < vipChance)
        {
            // VIP 후보 탐색 (LINQ 미사용)
            for (int i = 0; i < currentPool.Length; i++)
            {
                if (currentPool[i].isVIP)
                {
                    selectedData = currentPool[i];
                    break;
                }
            }
        }

        // VIP를 못 찾았거나 VIP 확률에 해당 안 되면 일반 무작위
        if (selectedData == null)
        {
            int randomIndex = Random.Range(0, currentPool.Length);
            selectedData = currentPool[randomIndex];
        }

        // 2. 풀에서 빈 껍데기 꺼내기
        CustomerController customer = customerPool.Dequeue();

        // 3. 성별에 맞춰 조립 (분기 처리)
        if (selectedData.gender == Gender.Male)
        {
            customer.SetupCustomer(selectedData, ref appearanceDB.maleParts);
        }
        else
        {
            customer.SetupCustomer(selectedData, ref appearanceDB.femaleParts);
        }

        // 4. 스폰 및 활성화 파이프라인
        // 물리적 시작 위치 세팅
        customer.transform.position = spawnPoint.position;

        // 💡 Step 1: 리스트에 먼저 추가합니다. (그래야 내 순서가 확정됨)
        activeCustomers.Add(customer);

        // 💡 Step 2: 내 순서(마지막 인덱스)를 바탕으로 목표 지점을 계산하고 주입합니다.
        Vector3 targetPos = GetPositionByIndex(activeCustomers.Count - 1);
        customer.UpdateTargetPosition(targetPos);

        // 💡 Step 3: 모든 세팅(좌표, 데이터)이 끝난 후 상태 머신(FSM)을 가동합니다.
        customer.OnSpawn();
    }

    public void LeaveQueue(CustomerController customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer); // 명단에서 삭제
            UpdateQueuePositions(); // 💡 즉시 뒷사람들에게 앞으로 오라고 명령!
        }
    }

    // 💡 모든 손님 강제 퇴장 (장사 종료 시)
    public void ForceLeaveAllCustomers()
    {
        for (int i = activeCustomers.Count - 1; i >= 0; i--)
        {
            activeCustomers[i].ChangeState(new CustomerLeaveState(activeCustomers[i], false));
        }
    }

    // 2. 손님이 화면 밖으로 완전히 퇴장했을 때 호출 (메모리 회수)
    public void ReturnToPool(CustomerController customer)
    {
        customer.OnDespawn();
        customerPool.Enqueue(customer);
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < activeCustomers.Count; i++)
        {
            // 1. 현재 리스트 순서(i)에 맞는 새로운 좌표를 계산합니다.
            Vector3 newTarget = GetPositionByIndex(i);

            // 2. 해당 손님에게 새로운 목표 지점을 알려줍니다.
            activeCustomers[i].UpdateTargetPosition(newTarget);
        }
    }

    // 인덱스를 넣으면 좌표를 반환하는 헬퍼 함수 (중복 코드 방지)
    private Vector3 GetPositionByIndex(int index)
    {
        float offsetX = index * queueSpacing;
        return new Vector3(truckCounter.position.x - offsetX, truckCounter.position.y, 0f);
    }
}