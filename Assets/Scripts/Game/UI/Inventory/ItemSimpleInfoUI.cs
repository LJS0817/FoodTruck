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
    
    private RectTransform _rectTransform;
    private RectTransform _canvasGroupRectTransform;
    private TMP_Text _applyBtnText;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        if (_canvasGroup != null)
        {
            _canvasGroupRectTransform = _canvasGroup.GetComponent<RectTransform>();
        }

        if (_applyButton != null) 
        {
            _applyButton.onClick.AddListener(OnClickApply);
            _applyBtnText = _applyButton.GetComponentInChildren<TMP_Text>();
        }

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
                string stateText = "";
                switch (item.state)
                {
                    case IngredientState.Optimal: stateText = "완벽"; break;
                    case IngredientState.Ruined: stateText = "망친"; break;
                    case IngredientState.Prep1: stateText = "Ver.1"; break;
                    case IngredientState.Prep2: stateText = "Ver.2"; break;
                    case IngredientState.Prep3: stateText = "Ver.3"; break;
                    case IngredientState.Raw: stateText = "미가공"; break;
                }
                _nameText.text = $"[{item.processType} {stateText}] {item.data.ingredientName} ( x {item.amount} )";
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

        if (_applyButton != null) 
        {
            _applyButton.gameObject.SetActive(true);
            if (_applyBtnText != null)
            {
                _applyBtnText.text = IsSameAsCurrentBox(item) ? "비우기" : "채우기";
            }
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // 정보 변경 후 즉시 레이아웃 크기 재계산 (ContentSizeFitter, LayoutGroup)
        Canvas.ForceUpdateCanvases();
        if (_canvasGroupRectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_canvasGroupRectTransform);
        }
        if (_rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }

    public void CloseUI()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _currentItem = null;
    }

    private bool IsSameAsCurrentBox(InventoryItem item)
    {
        if (item == null || IngredientManager.Instance == null) return false;
        
        IngredientBox currentBox = IngredientManager.Instance.GetCurrentBox();
        if (currentBox == null || currentBox.GetCurrentData() == null) return false;

        ItemGrade boxGrade = currentBox.qualityScore >= 1.15f ? ItemGrade.Premium : ItemGrade.Normal;
        
        return currentBox.GetCurrentData().ingredientID == item.data.ingredientID &&
               currentBox.targetState == item.state &&
               currentBox.targetProcess == item.processType &&
               boxGrade == item.grade;
    }

    private void OnClickApply()
    {
        if (_currentItem != null)
        {
            if (IsSameAsCurrentBox(_currentItem))
            {
                IngredientManager.Instance.EmptyCurrentBox();
            }
            else
            {
                IngredientManager.Instance.SetupBox(_currentItem, -1);
            }
            
            InventoryManager.Instance.CloseUI();
            CloseUI();
        }
    }
}
