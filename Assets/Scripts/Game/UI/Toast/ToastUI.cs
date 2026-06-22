using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

// 개별 Toast 객체의 애니메이션과 컴포넌트를 관리하는 클래스
public class ToastUI : MonoBehaviour, IPointerClickHandler
{
    private CanvasGroup canvasGroup;
    [SerializeField]
    private TMP_Text toastText;
    private RectTransform rectTransform;
    
    private Vector2 originalPosition;
    private Sequence sequence;
    private float currentMoveDistance;

    void Awake()
    {
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        toastText = GetComponentInChildren<TMP_Text>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void Show(string message, float fadeDuration, float displayDuration, float moveDistance)
    {
        currentMoveDistance = moveDistance;
        
        // 매번 부모의 레이아웃 위치는 갱신되더라도, 자식의 초기 로컬 위치는 고정으로 둠
        if (toastText != null) toastText.text = message;
        gameObject.SetActive(true);

        // 실행 중인 애니메이션 취소
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }

        // 초기 상태 설정
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPosition - new Vector2(0, moveDistance);

        // DOTween 애니메이션 구성 (SetUpdate(true)를 추가하여 Time.timeScale의 영향을 받지 않게 함)
        sequence = DOTween.Sequence().SetUpdate(true);
        
        sequence.Append(canvasGroup.DOFade(1f, fadeDuration));
        sequence.Join(rectTransform.DOAnchorPos(originalPosition, fadeDuration).SetEase(Ease.OutBack));
        
        sequence.AppendInterval(displayDuration);
        
        sequence.Append(canvasGroup.DOFade(0f, fadeDuration));
        sequence.Join(rectTransform.DOAnchorPos(originalPosition - new Vector2(0, moveDistance), fadeDuration).SetEase(Ease.InBack));

        sequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 시 실행 중인 애니메이션 취소 후 빠르게 닫기
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }

        float fastFadeDuration = 0.15f;
        sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(canvasGroup.DOFade(0f, fastFadeDuration));
        sequence.Join(rectTransform.DOAnchorPos(originalPosition - new Vector2(0, currentMoveDistance), fastFadeDuration).SetEase(Ease.InBack));
        
        sequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
