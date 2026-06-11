using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeInPopupUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descText;
    
    [SerializeField] private Button _tradeInButton; // 보상 판매 버튼
    [SerializeField] private TMP_Text _tradeInPriceText;
    
    [SerializeField] private Button _normalBuyButton; // 일반 구매 버튼
    [SerializeField] private TMP_Text _normalPriceText;

    [SerializeField] private Button _cancelButton; // 취소 버튼

    private CanvasGroup _canvasGroup;
    private EquipmentData _targetEquipment;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_tradeInButton != null) _tradeInButton.onClick.AddListener(OnClickTradeIn);
        if (_normalBuyButton != null) _normalBuyButton.onClick.AddListener(OnClickNormalBuy);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(ClosePopup);
        
        ClosePopup();
    }

    public void OpenPopup(EquipmentData equipment, int normalCost, int tradeInCost)
    {
        _targetEquipment = equipment;

        if (_titleText != null) _titleText.text = "장비 구매 방식 선택";
        
        EquipmentData currentEq = EquipmentStoreManager.Instance.GetEquippedEquipment(equipment.type);
        if (_descText != null && currentEq != null)
        {
            _descText.text = $"현재 장착 중인 <color=orange>{currentEq.equipmentName}</color> 장비가 있습니다.\n보상 판매를 통해 할인받으시겠습니까?";
        }

        if (_normalPriceText != null) _normalPriceText.text = normalCost.ToString("N0") + "원";
        if (_tradeInPriceText != null) _tradeInPriceText.text = tradeInCost.ToString("N0") + "원";

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void ClosePopup()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _targetEquipment = null;
    }

    private void OnClickTradeIn()
    {
        if (_targetEquipment != null)
        {
            StoreManager.Instance.ExecuteEquipmentPurchase(_targetEquipment, true);
        }
        ClosePopup();
    }

    private void OnClickNormalBuy()
    {
        if (_targetEquipment != null)
        {
            StoreManager.Instance.ExecuteEquipmentPurchase(_targetEquipment, false);
        }
        ClosePopup();
    }
}
