using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSimpleInfoUI : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private TMP_Text _expirationText;

    [SerializeField] private AmountSetter _amountSetter;

    [SerializeField] private Button _discardButton;
    [SerializeField] private Button _applyButton;

    private InventoryItem _currentItem;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_discardButton != null) _discardButton.onClick.AddListener(OnClickDiscard);
        if (_applyButton != null) _applyButton.onClick.AddListener(OnClickApply);

        CloseUI();
    }

    public void OpenInfo(InventoryItem item)
    {
        if (item == null) return;
        _currentItem = item;

        if (_nameText != null) _nameText.text = item.data.ingredientName;
        if (_itemIcon != null) _itemIcon.sprite = item.data.ingredientSprite;
        if (_descText != null) _descText.text = item.data.description;
        if (_amountText != null) _amountText.text = $"보유량: {item.amount}개";
        if (_expirationText != null) _expirationText.text = $"유통기한: {item.remainingDays}일";

        if (_amountSetter != null)
        {
            _amountSetter.gameObject.SetActive(true);
            _amountSetter.SetAmountInfo(0, item.amount);
        }

        if (_applyButton != null) _applyButton.gameObject.SetActive(true);
        if (_discardButton != null) _discardButton.gameObject.SetActive(true);

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void CloseUI()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _currentItem = null;
    }

    private void OnClickDiscard()
    {
        if (_currentItem != null)
        {
            int amount = _amountSetter != null ? _amountSetter.CurrentAmount : 1;
            int total = InventoryManager.Instance.GetTotalAmount(_currentItem.data.ingredientID);
            if (amount > total) amount = total;

            InventoryManager.Instance.DiscardIngredients(_currentItem.data.ingredientID, amount);
            Debug.Log($"[ItemSimpleInfoUI] {_currentItem.data.ingredientName} {amount}개 폐기 완료.");
            
            InventoryManager.Instance.CloseUI();
            CloseUI();
        }
    }

    private void OnClickApply()
    {
        if (_currentItem != null)
        {
            int amount = _amountSetter != null ? _amountSetter.CurrentAmount : 1;
            IngredientManager.Instance.SetupBox(_currentItem.data, amount);
            InventoryManager.Instance.CloseUI();
            CloseUI();
        }
    }
}
