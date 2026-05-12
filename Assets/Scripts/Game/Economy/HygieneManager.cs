using UnityEngine;

public class HygieneManager : MonoBehaviour
{
    public static HygieneManager Instance { get; private set; }

    [Header("Hygiene Settings")]
    [Range(0, 100)]
    public float currentHygiene = 100f;
    private const float HYGIENE_DROP_PER_CUSTOMER = 5f; // 손님 1명당 감소량

    public event System.Action<float> OnHygieneChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted += ResetHygiene;
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNewDayStarted -= ResetHygiene;
        }
    }

    private void ResetHygiene()
    {
        currentHygiene = 100f;
        OnHygieneChanged?.Invoke(currentHygiene);
    }

    // 손님이 음식을 받고 갈 때 호출됨
    public void DropHygiene()
    {
        float dropAmount = HYGIENE_DROP_PER_CUSTOMER;

        // 알바생 중 '청소부' 특성이 있으면 청결도 감소량 하락 (예: 50% 방어)
        if (UpgradeManager.Instance.Worker != null)
        {
            // WorkerAbility에 Cleaner 특성을 추가한다고 가정 (임시로 하드코딩 대체 가능하지만 확장성 고려)
            // 여기선 임시로 기본 구현
        }

        currentHygiene -= dropAmount;
        currentHygiene = Mathf.Clamp(currentHygiene, 0f, 100f);

        Debug.Log($"<color=#8B4513>[청결도] 쓰레기가 발생했습니다. 현재 청결도: {currentHygiene}%</color>");
        OnHygieneChanged?.Invoke(currentHygiene);
    }

    // 유저가 쓰레기를 터치해서 청소할 때 호출 (UI 버튼 등과 연결)
    public void CleanUp()
    {
        currentHygiene += 20f; // 한번 청소 시 20 회복
        currentHygiene = Mathf.Clamp(currentHygiene, 0f, 100f);
        
        // 체력 소모 로직 추가 가능
        if (PlayerStaminaManager.Instance != null)
        {
            PlayerStaminaManager.Instance.DrainStamina(2f); // 청소할 때 체력 2 소모
        }

        Debug.Log($"<color=#32CD32>[청소] 트럭 주변을 청소했습니다! 현재 청결도: {currentHygiene}%</color>");
        OnHygieneChanged?.Invoke(currentHygiene);
    }

    // 인내심 시스템 등에서 호출하여 배율을 가져감
    public float GetPatiencePenaltyMultiplier()
    {
        if (currentHygiene < 20f) return 2.0f; // 매우 더러움: 인내심 2배 빨리 닳음
        if (currentHygiene < 50f) return 1.5f; // 약간 더러움: 인내심 1.5배 빨리 닳음
        return 1.0f; // 깨끗함
    }
}
