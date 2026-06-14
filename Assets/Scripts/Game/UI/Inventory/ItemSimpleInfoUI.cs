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

        if (_nameText != null) 
        {
            if (item.processType != ProcessType.None)
            {
                string stateText = item.state == IngredientState.Optimal ? "완벽" : (item.state == IngredientState.Ruined ? "망친" : "준비중");
                _nameText.text = $"[{stateText} {item.processType}] {item.data.ingredientName} ( x {item.amount} )";
            }
            else
            {
                _nameText.text = $"{item.data.ingredientName} ( x {item.amount} )";
            }
        }

        if (_itemIcon != null) 
        {
            Sprite displaySprite = item.data.ingredientSprite;
            if (item.processType != ProcessType.None)
            {
                ProcessMethodData method = item.data.GetProcessMethod(item.processType);
                if (method != null)
                {
                    var stateEntry = method.stateSteps.Find(s => s.state == item.state);
                    if (stateEntry != null && stateEntry.stateSprite != null)
                        displaySprite = stateEntry.stateSprite;
                }
            }
            _itemIcon.sprite = displaySprite;
        }

        if (_descText != null) 
        {
            string gradeText = item.grade == ItemGrade.Perfect ? "🌟 최고급" : (item.grade == ItemGrade.Premium ? "✨ 고급" : "일반");
            _descText.text = $"[품질: {gradeText}]\n{item.data.description}";
        }
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
