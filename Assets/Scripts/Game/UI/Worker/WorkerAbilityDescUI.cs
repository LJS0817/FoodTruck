using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerAbilityDescUI : MonoBehaviour
{
    [Header("Base UI Elements")]
    public Image iconImage;
    public Image iconImage_0;
    public TMP_Text nameText;
    public TMP_Text abilityDescText;    // 패시브 스킬 텍스트 표시용

    public void SetAbilityInfo(WorkerAbilityData workerAbility, int valuePercent = 0)
    {
        // 패시브 스킬(능력) 아이콘 및 텍스트 세팅
        if (workerAbility == null)
        {
            abilityDescText.text = "특화 능력 없음";
            nameText.text = "없음";
            iconImage.sprite = null;
            iconImage_0.sprite = null;
        }
        else
        {
            nameText.text = workerAbility.abilityName;
            // 수치가 있을 경우 수치 부분만 노란색(#FFD700) 계열로 강조
            if (valuePercent > 0)
            {
                abilityDescText.text = $"{workerAbility.description} (<color=#FF0000>+{valuePercent}%</color>)";
            }
            else
            {
                abilityDescText.text = workerAbility.description;
            }
            iconImage.sprite = workerAbility.abilityIcon;
            iconImage_0.sprite = workerAbility.abilityIcon;
        }
    }

    private string GetAbilityName(WorkerAbility ability)
    {
        switch (ability)
        {
            case WorkerAbility.PatienceBoost: return "인내심 하락 지연";
            case WorkerAbility.StaminaSaver: return "피로도 감소 저하";
            case WorkerAbility.AutoCookSpeedUp: return "자동 조리 가속";
            case WorkerAbility.SpawnRateBoost: return "손님 스폰 증가";
            case WorkerAbility.TipBonus: return "팁 보너스";
            case WorkerAbility.IngredientDiscount: return "재료 구매 할인";
            case WorkerAbility.HygieneSaver: return "청결도 하락 지연";
            case WorkerAbility.PremiumRateBoost: return "프리미엄 주문 증가";
            case WorkerAbility.WeatherResist: return "악천후 페널티 완화";
            case WorkerAbility.WaitingCapacity: return "웨이팅 한도 증가";
            case WorkerAbility.SellPriceBonus: return "요리 판매가 보너스";
            case WorkerAbility.CookingMinigameEasy: return "미니게임 속도 저하";
            case WorkerAbility.VIPSpawnBoost: return "VIP 등장 확률 증가";
            case WorkerAbility.MarketRefreshDiscount: return "상점 갱신비 할인";
            case WorkerAbility.OvertimeBonus: return "야간 영업 보너스";
            default: return "알 수 없음";
        }
    }
}
