using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사장님 피로도 게이지 UI. Image(Filled) 방식으로 표시합니다.
/// </summary>
public class PlayerStaminaUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private GameObject warningIcon; // 피로도 낮을 때 경고 아이콘 (선택)

    [Header("Warning Threshold")]
    [SerializeField] private float warningThreshold = 0.25f; // 25% 이하일 때 경고

    private void Start()
    {
        if (PlayerStaminaManager.Instance != null)
        {
            PlayerStaminaManager.Instance.OnStaminaChanged += UpdateUI;
        }
    }

    private void OnDestroy()
    {
        if (PlayerStaminaManager.Instance != null)
        {
            PlayerStaminaManager.Instance.OnStaminaChanged -= UpdateUI;
        }
    }

    private void UpdateUI(float current, float max)
    {
        if (staminaFillImage == null) return;

        float ratio = max > 0f ? current / max : 0f;
        staminaFillImage.fillAmount = ratio;

        // 낮을 때 빨갛게
        if (ratio <= warningThreshold)
        {
            staminaFillImage.color = Color.red;
            if (warningIcon != null) warningIcon.SetActive(true);
        }
        else if (ratio <= 0.5f)
        {
            staminaFillImage.color = Color.yellow;
            if (warningIcon != null) warningIcon.SetActive(false);
        }
        else
        {
            staminaFillImage.color = Color.green;
            if (warningIcon != null) warningIcon.SetActive(false);
        }
    }
}
