using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 조리 장비 오브젝트 (도마, 그릴, 믹서기 등).
/// EquipmentData ScriptableObject를 참조하여, 지원하는 가공 방식·보너스를 자동으로 읽어옵니다.
/// IngredientObject가 드래그되어 이 위에 놓이면 해당 재료에 맞는 가공을 자동 시작합니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Equipment : MonoBehaviour, IInteractable
{
    [Header("장비 데이터")]
    [Tooltip("이 장비의 ScriptableObject 데이터. 지원 가공 방식, 보너스 등이 자동 연동됩니다.")]
    [HideInInspector]
    public EquipmentData equipmentData;

    [Header("비주얼 및 UI")]
    [Tooltip("가공 진행 중 표시할 이펙트 (선택)")]
    [SerializeField] private GameObject processingEffect;

    [Tooltip("조리 완료 시 표시할 이펙트 (선택)")]
    [SerializeField] private GameObject completeEffect;

    [Tooltip("타버렸을 때 표시할 이펙트 (선택)")]
    [SerializeField] private GameObject spoiledEffect;

    [Tooltip("조리 중인 재료의 상태 변화를 보여줄 스프라이트 렌더러")]
    public SpriteRenderer ingredientVisual;

    [Tooltip("진행률 슬라이더 UI (선택)")]
    public Slider progressBar;

    [Tooltip("남은 시간 텍스트 UI (선택)")]
    public TMP_Text timerText;

    [Tooltip("조리 중간에 재료를 강제로 빼내는 회수 버튼 (선택)")]
    public Button extractButton;

    private void Awake()
    {
        if (extractButton != null)
        {
            extractButton.onClick.AddListener(OnExtractButtonClicked);
        }
    }

    private void OnExtractButtonClicked()
    {
        if (equipmentData == null || ProcessManager.Instance == null) return;
        
        ProcessManager.Instance.ExtractTask(equipmentData.type, (success, result) => 
        {
            SyncState();
        });
    }

    private void Update()
    {
        SyncState();
    }

    /// <summary>
    /// ProcessManager의 현재 장비 작업 상태를 읽어와 시각적 효과와 UI를 동기화합니다.
    /// TruckInsideNavigation에서 화면을 전환할 때에도 즉시 호출하여 상태를 맞춥니다.
    /// </summary>
    public void SyncState()
    {
        if (equipmentData == null || ProcessManager.Instance == null) return;

        ProcessTask task = ProcessManager.Instance.GetActiveTask(equipmentData.type);
        
        if (task == null)
        {
            SetProcessingEffect(false);
            SetCompleteEffect(false);
            SetSpoiledEffect(false);
            SetUIActive(false);
            if (extractButton != null) extractButton.gameObject.SetActive(false);
            if (ingredientVisual != null) ingredientVisual.sprite = null;
            return;
        }

        // 재료 스프라이트 갱신
        if (ingredientVisual != null && task.method != null)
        {
            var stateEntry = task.method.GetStateAtTime(task.elapsedTime);
            if (stateEntry != null)
            {
                ingredientVisual.sprite = stateEntry.stateSprite;
            }
        }

        float optimalTime = task.method.GetOptimalTime();
        float ruinedTime = task.method.GetRuinedTime();

        if (task.state == ProcessState.Processing)
        {
            SetProcessingEffect(true);
            SetCompleteEffect(false);
            SetSpoiledEffect(false);
            SetUIActive(true);
            if (extractButton != null) extractButton.gameObject.SetActive(true); // 조리 중일 때만 활성화

            if (progressBar != null)
            {
                progressBar.value = Mathf.Clamp01(task.elapsedTime / optimalTime);
            }
            if (timerText != null)
            {
                float remain = Mathf.Max(0f, optimalTime - task.elapsedTime);
                timerText.text = $"{remain:F1}s";
                timerText.color = Color.white;
            }
        }
        else if (task.state == ProcessState.Completed)
        {
            SetProcessingEffect(false);
            SetCompleteEffect(true);
            SetSpoiledEffect(false);
            SetUIActive(true);
            if (extractButton != null) extractButton.gameObject.SetActive(false);

            if (progressBar != null)
            {
                progressBar.value = 1f; // 꽉 찬 상태
            }
            if (timerText != null)
            {
                // 타버리기까지 남은 시간
                float remainSpoil = Mathf.Max(0f, ruinedTime - task.elapsedTime);
                timerText.text = $"Burn in: {remainSpoil:F1}s";
                timerText.color = Color.red;
            }
        }
        else if (task.state == ProcessState.Spoiled)
        {
            SetProcessingEffect(false);
            SetCompleteEffect(false);
            SetSpoiledEffect(true);
            
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = "Ruined";
                timerText.color = Color.red;
            }

            if (extractButton != null) extractButton.gameObject.SetActive(false);
        }
    }

    // --- IInteractable 인터페이스 구현 --- //

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        if (equipmentData == null || ProcessManager.Instance == null) return this;

        ProcessTask task = ProcessManager.Instance.GetActiveTask(equipmentData.type);
        if (task != null)
        {
            // 상태에 따라 미니게임 실행 또는 요리 수거/폐기 처리
            ProcessManager.Instance.InteractWithTask(equipmentData.type, (success, result) =>
            {
                // 상호작용 완료 후 상태 강제 갱신
                SyncState();
            });
        }
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }

    // --- 재료 수신 및 가공 시작 --- //

    public bool ReceiveIngredient(IngredientObject ingredientObj)
    {
        IngredientData inputData = ingredientObj.currentData;

        if (equipmentData == null || ProcessManager.Instance == null) return false;

        ProcessTask existingTask = ProcessManager.Instance.GetActiveTask(equipmentData.type);
        if (existingTask != null)
        {
            Debug.LogWarning($"<color=yellow>[Equipment] {equipmentData.type}은(는) 이미 작업 중입니다. 수거 후 사용하세요.</color>");
            return false;
        }

        // 현재 장비가 지원하는 ProcessType 순회하며 레시피 매칭
        ProcessMethodData matchedMethod = null;
        ProcessType matchedType = ProcessType.None;

        for (int i = 0; i < equipmentData.supportedProcessTypes.Count; i++)
        {
            ProcessType pt = equipmentData.supportedProcessTypes[i].processType;
            ProcessMethodData method = inputData.GetProcessMethod(pt);
            if (method != null)
            {
                matchedMethod = method;
                matchedType = pt;
                break;
            }
        }

        if (matchedMethod == null)
        {
            Debug.LogWarning($"<color=red>[Equipment] {inputData.ingredientName}은(는) {equipmentData.equipmentName}(으)로 가공할 수 없습니다.</color>");
            return false;
        }

        // 백그라운드 가공 시작
        bool started = ProcessManager.Instance.StartProcess(equipmentData.type, inputData, matchedType, consumeInventory: false);

        if (started)
        {
            // 조리 시작 시 장비에 들어갔으므로 화면의 재료 오브젝트는 파기합니다.
            ingredientObj.OnDespawn();
            SyncState();
            return true;
        }

        return false;
    }

    private void SetProcessingEffect(bool active)
    {
        if (processingEffect != null) processingEffect.SetActive(active);
    }

    private void SetCompleteEffect(bool active)
    {
        if (completeEffect != null) completeEffect.SetActive(active);
    }

    private void SetSpoiledEffect(bool active)
    {
        if (spoiledEffect != null) spoiledEffect.SetActive(active);
    }

    private void SetUIActive(bool active)
    {
        if (progressBar != null) progressBar.gameObject.SetActive(active);
        if (timerText != null) timerText.gameObject.SetActive(active);
    }
}