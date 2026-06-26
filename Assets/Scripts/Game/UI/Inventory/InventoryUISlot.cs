using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUISlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _ingredientAmount;
    [SerializeField] GameObject _warningIcon; // 유통기한 임박 (예: 3일 이하)
    [SerializeField] GameObject _dangerIcon;  // 유통기한 매우 임박 (예: 1일 이하)
    [SerializeField] Image _focus;
    [SerializeField] GameObject _placedObj; // 💡 다른 박스에 이미 배치된 상태 표시용

    private Action<InventoryUISlot> _onSlotClicked;
    public InventoryItem Item { get; private set; }

    public void SetInfo(InventoryItem item, Action<InventoryUISlot> onClicked)
    {
        this.Item = item;
        this._onSlotClicked = onClicked;
        
        Sprite displaySprite = item.data.ingredientSprite;
        if (item.processType != ProcessType.None)
        {
            ProcessMethodData method = item.data.GetProcessMethod(item.processType);
            if (method != null)
            {
                var stateEntry = method.stateSteps.Find(s => s.state == item.state);
                if (stateEntry != null && stateEntry.stateSprite != null)
                {
                    displaySprite = stateEntry.stateSprite;
                }
            }
        }
        _icon.sprite = displaySprite;
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

    // 💡 배치(할당) 상태 시각적 업데이트용
    public void SetPlaced(bool isPlaced)
    {
        if (_placedObj != null)
        {
            _placedObj.SetActive(isPlaced);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        _onSlotClicked?.Invoke(this);
    }

    public void SetFocus(bool active)
    {
        _focus.gameObject.SetActive(active);
    }
}