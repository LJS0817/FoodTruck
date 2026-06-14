using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSimpleInfoUI : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _expirationText;

    [SerializeField] private Button _applyButton;

    private InventoryItem _currentItem;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();

        if (_applyButton != null) _applyButton.onClick.AddListener(OnClickApply);

        CloseUI();
    }

    public void OpenInfo(InventoryItem item)
    {
        if (item == null) return;
        _currentItem = item;

        if (_nameText != null) _nameText.text = $"{item.data.ingredientName} ( x {item.amount} )";
        if (_itemIcon != null) _itemIcon.sprite = item.data.ingredientSprite;
        if (_descText != null) _descText.text = item.data.description;
        // if (_amountText != null) _amountText.text = $"보유량: {item.amount}개";
        if (_expirationText != null) _expirationText.text = $"유통기한\n{item.remainingDays}일";

        if (_applyButton != null) _applyButton.gameObject.SetActive(true);

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

    private void OnClickApply()
    {
        if (_currentItem != null)
        {
            IngredientManager.Instance.SetupBox(_currentItem.data, -1);
            InventoryManager.Instance.CloseUI();
            CloseUI();
        }
    }
}
