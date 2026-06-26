using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 사장님 피로도 게이지 UI. Image(Filled) 방식으로 표시합니다.
/// </summary>
public class PlayerStaminaUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectMask2D staminaMask;
    [SerializeField] private Image fillImage; // 색상 변경을 위한 실제 이미지
    [SerializeField] private TMP_Text staminaText; // 수치를 텍스트로 표시
    [SerializeField] private GameObject warningIcon; // 피로도 낮을 때 경고 아이콘 (선택)

    [Header("Warning Threshold")]
    [SerializeField] private float warningThreshold = 0.25f; // 25% 이하일 때 경고

    private float _initialWidth;

    private void Start()
    {
        _initialWidth = staminaMask.rectTransform.rect.width;
    }

    public void UpdateUI(float current, float max)
    {
        staminaText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

        float ratio = max > 0f ? current / max : 0f;

        // RectMask2D Padding Z(Right) 조절
        Vector4 padding = staminaMask.padding;
        padding.z = _initialWidth * (1f - ratio);
        staminaMask.padding = padding;

        // 낮을 때 빨갛게
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
