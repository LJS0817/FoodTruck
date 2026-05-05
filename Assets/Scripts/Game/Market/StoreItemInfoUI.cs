using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StoreItemInfoUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _currentAmountText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _buyButton;

    private StoreItem _currentItem;

    CanvasGroup _canvasGroup;

    /// <summary>
    /// ShopItemSlotUI에서 슬롯 클릭 시 호출됩니다.
    /// StoreItem 데이터를 받아 상세 정보를 화면에 표시합니다.
    /// </summary>
    public void OpenInfo(StoreItem item)
    {
        if (item == null || item.data == null) return;

        _currentItem = item;

        // 공통 표시 (StoreItem 팩토리에서 통합됨)
        _priceText.text = item.finalCost.ToString("N0");
        _nameText.text = item.itemName;
        if (item.icon != null) _iconImage.sprite = item.icon;

        // 설명은 데이터 타입별로 분기
        if (item.data is IngredientData ingredient)
        {
            _descText.text = ingredient.description;
        }
        else if (item.data is EquipmentData equipment)
        {
            _descText.text = equipment.description;
        }
        else
        {
            _descText.text = "";
        }

        // 구매 버튼 이벤트 연결
        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(OnClickBuy);

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void CloseUI()
    {
        if(_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _currentItem = null;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnClickBuy()
    {
        if (_currentItem == null) return;
        StoreManager.Instance.TryBuyItem(_currentItem);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 배경 클릭 시 닫기 등의 용도로 확장 가능
    }
}