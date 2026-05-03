using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPatienceUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image patienceFillImage;       // 인내심 슬라이더 (Image Type: Filled)
    public GameObject processingIndicator; // 현재 조리 중 표시 (예: 테두리, 진행 중 아이콘 등)
    float _maxPatience;

    private void Awake()
    {
        _maxPatience = 0f;
    }

    /// <summary>
    /// UI 초기화 (주문한 음식 데이터 주입)
    /// </summary>
    public void Init(float max)
    {
        _maxPatience = max;
        patienceFillImage.fillAmount = 1f; // 인내심 가득 찬 상태로 시작
        SetProcessingState(false);         // 초기에는 조리 중이 아님
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
}
