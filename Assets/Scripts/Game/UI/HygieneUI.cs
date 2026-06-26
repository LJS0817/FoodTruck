using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 청결도(Hygiene) 게이지 UI. Slider 방식을 사용하여 표시합니다.
/// </summary>
public class HygieneUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectMask2D hygieneMask;
    [SerializeField] private Image fillImage; // 색상 변경을 위한 실제 이미지
    [SerializeField] private TMP_Text hygieneText; // 수치를 텍스트로 표시
    [SerializeField] private GameObject warningIcon; // 청결도가 낮을 때 띄울 아이콘

    [Header("Warning Threshold")]
    [SerializeField] private float warningThreshold = 0.25f; // 25% 이하일 때 경고

    private float _initialWidth;
    private float _maxHygiene = 100f; // 청결도 최대값 (기본 100)

    private void Start()
    {
        if (hygieneMask != null)
        {
            _initialWidth = hygieneMask.rectTransform.rect.width;
        }

        if (HygieneManager.Instance != null)
        {
            HygieneManager.Instance.OnHygieneChanged += UpdateUI;
            // 스크립트 시작 시 현재 청결도로 UI 갱신
            UpdateUI(HygieneManager.Instance.currentHygiene);
        }
    }

    private void OnDestroy()
    {
        if (HygieneManager.Instance != null)
        {
            HygieneManager.Instance.OnHygieneChanged -= UpdateUI;
        }
    }

    private void UpdateUI(float currentHygiene)
    {
        if (hygieneText != null)
        {
            hygieneText.text = $"{Mathf.RoundToInt(currentHygiene)} / {_maxHygiene}";
        }

        if (hygieneMask == null) return;

        // 청결도는 0 ~ 100 기준이므로, 비율(0~1)로 변환
        float ratio = Mathf.Clamp01(currentHygiene / _maxHygiene);
        
        // RectMask2D Padding Z(Right) 조절
        Vector4 padding = hygieneMask.padding;
        padding.z = _initialWidth * (1f - ratio);
        hygieneMask.padding = padding;

        if (fillImage != null)
        {
            // 낮을 때 (더러울 때) 빨갛게
            if (ratio <= warningThreshold)
            {
                fillImage.color = Color.red;
                if (warningIcon != null) warningIcon.SetActive(true);
            }
            else if (ratio <= 0.5f)
            {
                fillImage.color = Color.yellow;
                if (warningIcon != null) warningIcon.SetActive(false);
            }
            else
            {
                fillImage.color = Color.green;
                if (warningIcon != null) warningIcon.SetActive(false);
            }
        }
    }
}
