using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _ingredientAmount;
    [SerializeField] GameObject _warningIcon; // 유통기한 임박 (예: 3일 이하)
    [SerializeField] GameObject _dangerIcon;  // 유통기한 매우 임박 (예: 1일 이하)
    [SerializeField] Image _focus;

    private Action<InventoryUISlot> _onSlotClicked;
    public InventoryItem Item { get; private set; }

    public void SetInfo(InventoryItem item, Action<InventoryUISlot> onClicked)
    {
        this.Item = item;
        this._onSlotClicked = onClicked;
        
        _icon.sprite = item.data.ingredientSprite;
        _focus.gameObject.SetActive(false);
        if (_ingredientAmount != null) _ingredientAmount.SetText(item.amount.ToString());

        // 유통기한 시각화 (Danger, Warning 아이콘)
        if (_dangerIcon != null) _dangerIcon.SetActive(false);
        if (_warningIcon != null) _warningIcon.SetActive(false);

        if (item.remainingDays <= 1)
        {
            if (_dangerIcon != null) _dangerIcon.SetActive(true);
        }
        else if (item.remainingDays <= 3)
        {
            if (_warningIcon != null) _warningIcon.SetActive(true);
        }
    }

    public void OnClicked() {
        StoreManager.Instance.UIController.ShowItemInfo(StoreItem.FromIngredient(Item.data, Item.data.basePrice), false);
        _onSlotClicked?.Invoke(this);
    }

    public void SetFocus(bool active)
    {
        _focus.gameObject.SetActive(active);
    }
}