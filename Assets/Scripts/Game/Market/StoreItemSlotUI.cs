using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemSlotUI : MonoBehaviour
{
    [SerializeField] private Button _slotButton;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private GameObject _saleTag;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _categoryIcon;

    private StoreItem _item;

    public System.Action<StoreItem> onClickAction;

    public void Setup(StoreItem item, System.Action<StoreItem> onClick = null)
    {
        _item = item;
        onClickAction = onClick;

        // 이름, 가격, 아이콘 표시 (StoreItem 팩토리에서 이미 통합됨)
        _nameText.text = item.itemName;
        _priceText.text = item.finalCost.ToString("N0");
        if (item.icon != null) _iconImage.sprite = item.icon;

        // 카테고리 아이콘 설정
        if (_categoryIcon != null)
        {
            // 필요 시 각 카테고리별 Sprite를 필드로 받아와서 표시
            // _categoryIcon.sprite = item.categoryIcon;
        }

        // 잠금 아이콘
        if (_lockIcon != null)
        {
            _lockIcon.SetActive(false); // 임시
        }

        // 할인가 태그
        if (_saleTag != null)
        {
            _saleTag.SetActive(false); // 임시
        }

        // 버튼 이벤트 연결 → 슬롯 클릭 시 상세 정보 표시
        _slotButton.onClick.RemoveAllListeners();
        _slotButton.onClick.AddListener(OnClickSlot);
    }

    /// <summary>
    /// 슬롯 클릭 시 StoreUIController를 통해 ItemInfoUI에 아이템 정보를 전달합니다.
    /// </summary>
    private void OnClickSlot()
    {
        if (_item == null) return;
        if (onClickAction != null) onClickAction(_item);
        else StoreManager.Instance.UIController.ShowItemInfo(_item);
    }
}
