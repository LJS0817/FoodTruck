using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _ingredientName;
    [SerializeField] TMP_Text _ingredientAmount;
    [SerializeField] TMP_Text _expiration;
    [SerializeField] Image _focus;

    private Action<InventoryUISlot> _onSlotClicked;
    public InventoryItem Item { get; private set; }

    public void SetInfo(InventoryItem item, Action<InventoryUISlot> onClicked)
    {
        this.Item = item;
        this._onSlotClicked = onClicked;
        
        _icon.sprite = item.data.ingredientSprite;
        _ingredientName.SetText(item.data.ingredientName);
        _expiration.SetText(item.expirationDate.ToString("yyyy-MM-dd"));
        _focus.gameObject.SetActive(false);
        if (_ingredientAmount != null) _ingredientAmount.SetText(item.amount.ToString());
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