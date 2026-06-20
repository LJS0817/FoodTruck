using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [Header("Generation Settings")]
    public List<WorkerAbilityData> availableAbilities;
    public List<string> firstNames = new List<string> { "김", "이", "박", "최", "정", "조", "윤", "장", "임", "오" };
    public List<string> lastNames = new List<string> { "철수", "영희", "민수", "수진", "지훈", "동욱", "수아", "지은", "민재", "서연" };
    
    public int poolSize = 3;

    private List<WorkerData> _recruitmentPool = new List<WorkerData>();
    private List<WorkerData> _hiredWorkers = new List<WorkerData>();

    public IReadOnlyList<WorkerData> RecruitmentPool => _recruitmentPool;
    public IReadOnlyList<WorkerData> HiredWorkers => _hiredWorkers;

    public static WorkerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 인스펙터(또는 AutoAssetInjector)로 주입된 데이터가 없을 때만 Resources 폴더에서 폴백으로 로드
        if (availableAbilities == null || availableAbilities.Count == 0)
        {
            var loaded = Resources.LoadAll<WorkerAbilityData>("WorkerAbilities");
            if (loaded != null && loaded.Length > 0)
            {
                availableAbilities = new List<WorkerAbilityData>(loaded);
            }
            else
            {
                availableAbilities = new List<WorkerAbilityData>();
            }
        }
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += OnNewDayStarted;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= OnNewDayStarted;
        }
    }

    private void OnNewDayStarted()
    {
        PayDailySalaries();
    }

    // 수동 갱신 시스템 (하루 1회 무료)
    public bool ManualRefreshRecruitmentPool()
    {
        int cost = GetRefreshCost();
        
        // 비용이 0(무료)이거나 돈을 지불할 수 있는 경우
        if (cost == 0 || PlayerManager.Instance.SpendMoney(cost))
        {
            if (cost > 0)
            {
                SettlementManager.Instance?.AddExpense(cost);
            }
            RefreshRecruitmentPool();
            return true;
        }
        return false;
    }

    public int GetRefreshCost()
    {
        // 마지막 갱신일보다 현재 날짜가 크다면 무료(0원)
        if (DataManager.Instance != null && DataManager.Instance.CurrentData != null && GameTimeManager.Instance != null)
        {
            if (GameTimeManager.Instance.GetCurrentDay() > DataManager.Instance.CurrentData.lastWorkerRefreshDay)
            {
                return 0;
            }
        }

        int baseCost = 1000;
        // 정보원(MarketRefreshDiscount) 능력이 있는 직원이 있다면 갱신 비용 감소
        float discount = GetAbilityTotalValue(WorkerAbility.MarketRefreshDiscount);
        // 할인율 상한선 설정 (최대 80% 할인)
        discount = Mathf.Min(discount, 0.8f);
        return Mathf.RoundToInt(baseCost * (1f - discount));
    }

    private void RefreshRecruitmentPool()
    {
        _recruitmentPool.Clear();
        for (int i = 0; i < poolSize; i++)
        {
            _recruitmentPool.Add(GenerateRandomWorker());
        }
        
        if (DataManager.Instance != null && DataManager.Instance.CurrentData != null)
        {
            DataManager.Instance.CurrentData.lastWorkerRefreshDay = GameTimeManager.Instance?.GetCurrentDay() ?? 1;
        }
        SyncToSaveData();
        
        Debug.Log($"<color=yellow>[WorkerManager] 채용 인력 풀 갱신 완료 ({poolSize}명)</color>");
    }

    private WorkerData GenerateRandomWorker()
    {
        WorkerData worker = new WorkerData();
        worker.workerID = System.Guid.NewGuid().ToString();
        
        // 1. 이름 결정
        string first = firstNames.Count > 0 ? firstNames[Random.Range(0, firstNames.Count)] : "";
        string last = lastNames.Count > 0 ? lastNames[Random.Range(0, lastNames.Count)] : "Unknown";
        worker.workerName = first + last;

        // 2. 등급(Grade) 결정 RNG (가챠형 확률)
        // S: 1%, A: 4%, B: 10%, C: 15%, D: 20%, E: 20%, F: 30%
        float r = Random.value;
        if (r < 0.01f) worker.grade = WorkerGrade.S;
        else if (r < 0.05f) worker.grade = WorkerGrade.A;
        else if (r < 0.15f) worker.grade = WorkerGrade.B;
        else if (r < 0.30f) worker.grade = WorkerGrade.C;
        else if (r < 0.50f) worker.grade = WorkerGrade.D;
        else if (r < 0.70f) worker.grade = WorkerGrade.E;
        else worker.grade = WorkerGrade.F;

        // 3. 등급별 보정치 설정 (타이쿤 후반부 경제를 고려한 기하급수적 밸런스)
        int abilityCount = 0;
        float valueMultiplier = 1.0f;
        int maxLevel = 3;
        int baseHire = 1000;
        int baseSalary = 100;

        switch (worker.grade)
        {
            case WorkerGrade.S:
                abilityCount = 3; valueMultiplier = 2.5f; maxLevel = 10;
                baseHire = 100000; baseSalary = 5000; break;
            case WorkerGrade.A:
                abilityCount = 2; valueMultiplier = 1.6f; maxLevel = 7;
                baseHire = 25000; baseSalary = 1500; break;
            case WorkerGrade.B:
                abilityCount = 2; valueMultiplier = 1.3f; maxLevel = 5;
                baseHire = 8000; baseSalary = 500; break;
            case WorkerGrade.C:
                abilityCount = 1; valueMultiplier = 1.0f; maxLevel = 4;
                baseHire = 3000; baseSalary = 200; break;
            case WorkerGrade.D:
                abilityCount = 1; valueMultiplier = 0.8f; maxLevel = 3;
                baseHire = 1000; baseSalary = 80; break;
            case WorkerGrade.E:
                abilityCount = 1; valueMultiplier = 0.5f; maxLevel = 2;
                baseHire = 300; baseSalary = 30; break;
            case WorkerGrade.F:
                abilityCount = 0; valueMultiplier = 0.0f; maxLevel = 1;
                baseHire = 50; baseSalary = 10; break;
        }

        worker.maxLevel = maxLevel;
        worker.currentLevel = 1;
        worker.hiringCost = Mathf.RoundToInt(baseHire * Random.Range(0.9f, 1.1f));
        worker.baseDailySalary = Mathf.RoundToInt(baseSalary * Random.Range(0.9f, 1.1f));
        worker.incentiveMultiplier = 1.0f;

        // 4. 특화 분야(Specialty) 및 기본 스탯 배분
        worker.specialty = (WorkerSpecialty)Random.Range(0, 4);
        
        // 등급별 기준 스탯
        int baseStat = 10;
        switch (worker.grade)
        {
            case WorkerGrade.S: baseStat = 90; break;
            case WorkerGrade.A: baseStat = 70; break;
            case WorkerGrade.B: baseStat = 50; break;
            case WorkerGrade.C: baseStat = 40; break;
            case WorkerGrade.D: baseStat = 30; break;
            case WorkerGrade.E: baseStat = 20; break;
            case WorkerGrade.F: baseStat = 10; break;
        }

        int cook = Mathf.Max(1, baseStat + Random.Range(-5, 6));
        int human = Mathf.Max(1, baseStat + Random.Range(-5, 6));
        int stam = Mathf.Max(1, baseStat + Random.Range(-5, 6));
        int clean = Mathf.Max(1, baseStat + Random.Range(-5, 6));

        // 특화에 따른 가중치 부여 (나중에 기획적으로 쉽게 수정 가능하도록 구성)
        switch (worker.specialty)
        {
            case WorkerSpecialty.Cook:
                cook = Mathf.RoundToInt(cook * 1.5f);
                human = Mathf.RoundToInt(human * 0.8f);
                break;
            case WorkerSpecialty.Service:
                human = Mathf.RoundToInt(human * 1.5f);
                clean = Mathf.RoundToInt(clean * 0.8f);
                break;
            case WorkerSpecialty.Maintenance:
                clean = Mathf.RoundToInt(clean * 1.5f);
                stam = Mathf.RoundToInt(stam * 1.5f);
                cook = Mathf.RoundToInt(cook * 0.8f);
                break;
            case WorkerSpecialty.Balanced:
                cook = Mathf.RoundToInt(cook * 1.1f);
                human = Mathf.RoundToInt(human * 1.1f);
                stam = Mathf.RoundToInt(stam * 1.1f);
                clean = Mathf.RoundToInt(clean * 1.1f);
                break;
        }

        worker.cookSkill = cook;
        worker.humanSkill = human;
        worker.stamina = stam;
        worker.cleanSkill = clean;

        // 5. 특화 능력 무작위 배정
        if (availableAbilities != null && availableAbilities.Count > 0)
        {
            List<WorkerAbilityData> pool = new List<WorkerAbilityData>(availableAbilities);
            for (int i = 0; i < abilityCount; i++)
            {
                if (pool.Count == 0) break;
                int idx = Random.Range(0, pool.Count);
                WorkerAbilityData ab = pool[idx];
                pool.RemoveAt(idx); // 중복 방지

                float val = Random.Range(ab.minBaseValue, ab.maxBaseValue) * valueMultiplier;
                worker.abilities.Add(new WorkerAbilityNode { abilityType = ab.abilityType, baseValue = val });
            }
        }

        return worker;
    }

    // ===== 고용 시스템 =====

    public bool HireWorker(WorkerData worker)
    {
        if (_hiredWorkers.Contains(worker)) return false;

        if (PlayerManager.Instance.SpendMoney(worker.hiringCost))
        {
            _recruitmentPool.Remove(worker);
            _hiredWorkers.Add(worker);
            SettlementManager.Instance?.AddExpense(worker.hiringCost);
            Debug.Log($"<color=cyan>[알바생] {worker.workerName} 고용 완료!</color>");

            SyncToSaveData();
            return true;
        }
        return false;
    }

    public void FireWorker(WorkerData worker)
    {
        if (_hiredWorkers.Remove(worker))
        {
            SyncToSaveData();
            Debug.Log($"<color=orange>[알바생] {worker.workerName} 해고됨.</color>");
        }
    }

    // ===== 업그레이드 및 인센티브 =====

    public bool UpgradeWorker(WorkerData worker, int cost)
    {
        if (worker.currentLevel >= worker.maxLevel) return false;

        if (PlayerManager.Instance.SpendMoney(cost))
        {
            worker.currentLevel++;
            SettlementManager.Instance?.AddExpense(cost);
            SyncToSaveData();
            return true;
        }
        return false;
    }

    public void SetWorkerIncentive(WorkerData worker, float multiplier)
    {
        worker.incentiveMultiplier = multiplier;
        SyncToSaveData();
    }

    private void PayDailySalaries()
    {
        int totalSalary = 0;
        for (int i = 0; i < _hiredWorkers.Count; i++)
        {
            // 실제 지불하는 일급 = 기본 일급 * 인센티브
            totalSalary += Mathf.RoundToInt(_hiredWorkers[i].baseDailySalary * _hiredWorkers[i].incentiveMultiplier);
        }

        if (totalSalary > 0)
        {
            PlayerManager.Instance.SpendMoney(totalSalary);
            SettlementManager.Instance?.AddExpense(totalSalary);
            Debug.Log($"<color=red>[알바생 일급] 총 {totalSalary}원이 차감되었습니다.</color>");
        }
    }

    // ===== 능력치 제공 API =====

    public float GetAbilityTotalValue(WorkerAbility targetAbility)
    {
        float total = 0f;
        for (int i = 0; i < _hiredWorkers.Count; i++)
        {
            var worker = _hiredWorkers[i];
            for (int j = 0; j < worker.abilities.Count; j++)
            {
                if (worker.abilities[j].abilityType == targetAbility)
                {
                    // 기본값 + 렙업당 성장치(예: 레벨당 0.05) -> 이후 인센티브 배율 곱하기
                    float growth = (worker.currentLevel - 1) * 0.05f; 
                    float val = (worker.abilities[j].baseValue + growth) * worker.incentiveMultiplier;
                    total += val;
                }
            }
        }
        return total;
    }

    // ===== 저장 연동 =====

    public void LoadFromSaveData(List<WorkerData> hired, List<WorkerData> pool, int lastRefreshDay)
    {
        _hiredWorkers.Clear();
        _recruitmentPool.Clear();

        if (hired != null) _hiredWorkers.AddRange(hired);
        if (pool != null) _recruitmentPool.AddRange(pool);

        // 만약 풀이 비어있다면 새로고침
        if (_recruitmentPool.Count == 0 && DayCycleManager.Instance != null)
        {
            RefreshRecruitmentPool();
        }
    }

    private void SyncToSaveData()
    {
        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null) return;
        
        DataManager.Instance.CurrentData.hiredWorkers = new List<WorkerData>(_hiredWorkers);
        DataManager.Instance.CurrentData.recruitmentPool = new List<WorkerData>(_recruitmentPool);
    }
}
