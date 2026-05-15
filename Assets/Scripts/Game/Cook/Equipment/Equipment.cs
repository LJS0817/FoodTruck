using UnityEngine;

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
    public EquipmentData equipmentData;

    [Header("비주얼")]
    [Tooltip("가공 진행 중 표시할 이펙트 (선택)")]
    [SerializeField] private GameObject processingEffect;

    private bool isProcessing = false;

    // --- IInteractable 인터페이스 구현 --- //

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition) { }

    public void OnTouchEnd() { }

    // --- 재료 수신 및 가공 시작 --- //

    /// <summary>
    /// IngredientObject가 이 장비 위에 놓였을 때 호출됩니다.
    /// EquipmentData의 supportedProcessTypes를 순회하며, 드롭된 재료에 대해
    /// 가능한 가공 레시피를 자동으로 찾아 ProcessManager에 위임합니다.
    /// </summary>
    /// <param name="ingredientObj">가공할 재료 오브젝트</param>
    /// <returns>가공 수신 성공 여부 (이미 가공 중이거나 레시피가 없으면 false)</returns>
    public bool ReceiveIngredient(IngredientObject ingredientObj)
    {
        IngredientData ingredientData = ingredientObj.currentData;

        if (isProcessing)
        {
            Debug.LogWarning("<color=yellow>[Equipment] 이미 가공 중입니다. 완료 후 다시 시도하세요.</color>");
            return false;
        }

        if (equipmentData == null)
        {
            Debug.LogWarning("<color=red>[Equipment] EquipmentData가 연결되지 않았습니다.</color>");
            return false;
        }

        if (ProcessManager.Instance == null)
        {
            Debug.LogError("[Equipment] ProcessManager 인스턴스를 찾을 수 없습니다.");
            return false;
        }

        // EquipmentData의 supportedProcessTypes를 순회하며 매칭되는 레시피를 탐색
        ProcessRecipeData matchedRecipe = null;
        ProcessType matchedType = ProcessType.None;

        for (int i = 0; i < equipmentData.supportedProcessTypes.Count; i++)
        {
            ProcessType pt = equipmentData.supportedProcessTypes[i].processType;
            ProcessRecipeData recipe = ProcessManager.Instance.GetProcessRecipe(ingredientData, pt);
            if (recipe != null)
            {
                matchedRecipe = recipe;
                matchedType = pt;
                break;
            }
        }

        if (matchedRecipe == null)
        {
            Debug.LogWarning($"<color=red>[Equipment] {ingredientData.ingredientName}은(는) " +
                             $"{equipmentData.equipmentName}(으)로 가공할 수 없습니다.</color>");
            return false;
        }

        // 가공 시작
        isProcessing = true;
        SetProcessingEffect(true);
        ingredientObj.isProcessing = true; // 가공 중 드래그 불가
        // 위치 고정 로직 제거 - 드래그를 놓은 위치 그대로 유지

        Debug.Log($"<color=cyan>[Equipment] {equipmentData.equipmentName}: " +
                  $"{ingredientData.ingredientName} → {matchedType} 가공 시작!</color>");

        ProcessManager.Instance.ExecuteProcess(ingredientData, matchedType, (success, result) =>
        {
            isProcessing = false;
            SetProcessingEffect(false);
            ingredientObj.isProcessing = false;

            if (success && result != null)
            {
                Debug.Log($"<color=green>[Equipment] 가공 완료! → {result.ingredientName}</color>");
                // 1. 오브젝트 데이터 및 스프라이트 변경
                ingredientObj.SetupIngredient(result);
                // 2. 가공된 재료는 CookingPot으로만 이동 가능하게 설정
                ingredientObj.isProcessed = true;
            }
            else
            {
                Debug.LogWarning("<color=red>[Equipment] 가공에 실패했습니다.</color>");
            }
        }, consumeInventory: false, addToInventory: false);

        return true;
    }

    private void SetProcessingEffect(bool active)
    {
        if (processingEffect != null)
            processingEffect.SetActive(active);
    }
}