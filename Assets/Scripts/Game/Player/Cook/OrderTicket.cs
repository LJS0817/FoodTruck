using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrderTicket : MonoBehaviour
{
    public OrderData currentOrder;
    
    [SerializeField] Image _foodIcon;
    [SerializeField] TMP_Text _foodNameText;
    [SerializeField] CustomerPatienceUI _patience;


    public enum TicketType { Inside, Outside }
    public TicketType ticketType = TicketType.Inside;

    [Header("Settings")]
    public float effectDuration = 0.3f;     // 나타나고 사라지는 데 걸리는 시간
    [SerializeField] Vector2 _effectOffset;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine fadeCoroutine;

    private Vector2 originalPosition;
    private bool positionInitialized = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();

        // 초기 상태는 투명하게
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// UI 초기화 (주문한 음식 데이터 주입)
    /// </summary>
    public void SetupTicket(OrderData orderData)
    {
        this.currentOrder = orderData;

        if (orderData.orderedFood != null)
        {
            // FoodData에 음식 스프라이트가 있다고 가정 (없다면 FoodData에 추가 필요)
            // foodIconImage.sprite = orderedFood.foodSprite; 

            _foodNameText.SetText(orderData.orderedFood.foodName);
        }
        _patience.Init(orderData.owner.currentData.maxPatience);         // 초기에는 조리 중이 아님
    }

    public void UpdatePatience(float currentPatience)
    {
        _patience.UpdatePatience(currentPatience);
    }

    public void SetProcessingState(bool isProcessing)
    {
        _patience.SetProcessingState(isProcessing);
    }

    /// <summary>
    /// UI 표시 애니메이션 (Fade In)
    /// </summary>
    public void ShowUI()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(1f));
    }

    /// <summary>
    /// UI 숨김 애니메이션 (Fade Out)
    /// </summary>
    public void HideUI()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(0f));
    }

    // CanvasGroup Alpha 페이드 및 슬라이딩 이동 코루틴
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        // 최초 시작 위치 기억 (LayoutGroup 내에서 자식의 로컬 위치를 기억)
        if (!positionInitialized)
        {
            originalPosition = rectTransform.anchoredPosition;
            positionInitialized = true;
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        // 목표 위치 오프셋 결정
        Vector2 startOffset = rectTransform.anchoredPosition - originalPosition;
        Vector2 targetOffset = Vector2.zero;

        if (targetAlpha > 0f) // ShowUI (Fade In)
        {
            // 처음 나타나는 경우 시작 오프셋 강제 지정
            if (startAlpha <= 0.01f) 
            {
                startOffset = _effectOffset;
                // if (ticketType == TicketType.Inside) 
                //     startOffset = new Vector2(0f, -100f); // 아래에서 위로
                // else 
                //     startOffset = new Vector2(150f, 0f);  // 오른쪽에서 왼쪽으로
            }
            targetOffset = Vector2.zero; // 원래 자리로
        }
        else // HideUI (Fade Out)
        {
            // 사라질 때는 현재 위치에서 밖으로
            targetOffset = _effectOffset; // 원래 자리로
            // if (ticketType == TicketType.Inside)
            //     targetOffset = new Vector2(0f, -100f); // 아래로 내려감
            // else
            //     targetOffset = new Vector2(150f, 0f);  // 오른쪽으로 슬라이드
        }

        while (time < effectDuration)
        {
            time += Time.unscaledDeltaTime; // 화면 전환/정지 시에도 자연스럽게 애니메이션되도록 unscaled 사용 권장
            float t = time / effectDuration;
            
            // 자연스러운 감속 (Ease Out Cubic)
            float easeT = 1f - Mathf.Pow(1f - t, 3f); 

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easeT);
            rectTransform.anchoredPosition = originalPosition + Vector2.Lerp(startOffset, targetOffset, easeT);
            
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        rectTransform.anchoredPosition = originalPosition + targetOffset;

        // 💡 alpha가 0이 되면 풀링을 위해 gameObject를 비활성화만 함 (Destroy 안 함)
        if (targetAlpha <= 0f)
        {
            gameObject.SetActive(false);
            // 풀링으로 재사용될 때를 대비해 원상 복구해둠
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // 주문표를 터치했을 때 실행 (서빙 시도)
    public void ServeDish()
    {
        // 1. 조리대(CookingManager)에 완성된 요리가 있는지 확인
        Dish completedDish = CookingManager.Instance.GetCompletedDish();

        if (completedDish != null)
        {
            // 2. 완성된 요리가 이 주문표의 요리와 일치하는지 확인
            if (completedDish.foodData == currentOrder.orderedFood)
            {
                Debug.Log($"<color=cyan>[서빙 성공] {currentOrder.orderedFood.foodName}을(를) 손님에게 전달합니다!</color>");

                // 손님에게 요리 전달
                currentOrder.owner.ReceiveDish(completedDish);

                // 냄비 비우기 및 주문표 제거
                CookingManager.Instance.ClearDish();
                OrderManager.Instance.CompleteOrderOf(currentOrder.owner);
            }
            else
            {
                Debug.LogWarning("<color=orange>이 주문표에 맞는 요리가 아닙니다!</color>");
                CookingManager.Instance.ClearDish();
                OrderManager.Instance.CancelOrderOf(currentOrder.owner);
                currentOrder.owner.ReceiveDish(null);
            }
        }
        else
        {
            Debug.Log("<color=yellow>아직 완성된 요리가 없습니다.</color>");
        }
    }
}