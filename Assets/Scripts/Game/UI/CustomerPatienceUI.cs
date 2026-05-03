using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPatienceUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image foodIconImage;           // 주문한 음식 아이콘
    public TMP_Text foodNameText;         // 주문한 음식 이름
    public Image patienceFillImage;       // 인내심 슬라이더 (Image Type: Filled)
    public GameObject processingIndicator; // 현재 조리 중 표시 (예: 테두리, 진행 중 아이콘 등)

    [Header("Settings")]
    public float fadeDuration = 0.3f;     // 나타나고 사라지는 데 걸리는 시간

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine fadeCoroutine;
    private CustomerController linkedCustomer;

    float _maxPatience;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // 초기 상태는 투명하게
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        _maxPatience = 0f;
    }

    /// <summary>
    /// UI 초기화 (주문한 음식 데이터 주입)
    /// </summary>
    public void Setup(FoodData orderedFood, CustomerController customer)
    {
        linkedCustomer = customer;

        if (orderedFood != null)
        {
            // FoodData에 음식 스프라이트가 있다고 가정 (없다면 FoodData에 추가 필요)
            // foodIconImage.sprite = orderedFood.foodSprite; 

            foodNameText.text = orderedFood.foodName;
        }
        _maxPatience = customer.currentData.maxPatience;
        patienceFillImage.fillAmount = 1f; // 인내심 가득 찬 상태로 시작
        SetProcessingState(false);         // 초기에는 조리 중이 아님
    }

    /// <summary>
    /// 좌표를 주입받아 UI 위치를 정렬하는 함수
    /// </summary>
    /// <param name="targetPosition">UI가 위치할 스크린 좌표 또는 월드 좌표 (Canvas 렌더 모드에 따라 다름)</param>
    public void SetPosition(Vector3 targetPosition)
    {
        rectTransform.position = targetPosition;
    }

    /// <summary>
    /// 인내심 슬라이더 업데이트
    /// </summary>
    public void UpdatePatience(float currentPatience)
    {
        // 0.0 ~ 1.0 사이의 비율로 계산
        patienceFillImage.fillAmount = Mathf.Clamp01(currentPatience / _maxPatience);
    }

    /// <summary>
    /// 현재 이 주문이 처리 중(조리 중)인지 표시 상태 변경
    /// </summary>
    public void SetProcessingState(bool isProcessing)
    {
        // SetActive를 사용하는 방식
        if (processingIndicator != null)
        {
            processingIndicator.SetActive(isProcessing);
        }

        /* // 만약 Image의 알파값으로 표시하고 싶다면 아래 주석을 해제하고 사용하세요.
        Image indicatorImage = processingIndicator.GetComponent<Image>();
        if(indicatorImage != null)
        {
            Color c = indicatorImage.color;
            c.a = isProcessing ? 1f : 0.3f; // 켜지면 100%, 꺼지면 30%
            indicatorImage.color = c;
        }
        */
    }

    /// <summary>
    /// UI 표시 애니메이션 (Fade In)
    /// </summary>
    public void ShowUI()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(1f));
    }

    /// <summary>
    /// UI 숨김 애니메이션 (Fade Out)
    /// </summary>
    public void HideUI()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(0f));
        linkedCustomer = null;
    }

    /// <summary>
    /// 연결된 손님 정보 반환
    /// </summary>
    public CustomerController GetLinkedCustomer()
    {
        return linkedCustomer;
    }

    // CanvasGroup Alpha 페이드 코루틴
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // 💡 alpha가 0이 되면 gameObject를 비활성화하고 Destroy
        if (targetAlpha <= 0f)
        {
            gameObject.SetActive(false);
            // Instantiate 방식이므로 Destroy 처리
            Destroy(gameObject);
        }
    }
}
