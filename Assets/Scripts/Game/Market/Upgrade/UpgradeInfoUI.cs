using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeInfoUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    
    [Header("Stats")]
    [SerializeField] private TMP_Text _currentTitleText;
    [SerializeField] private TMP_Text _currentStatsText;
    [SerializeField] Transform _currentStarParent;
    Image[] _currentStars;

    [SerializeField] private TMP_Text _nextTitleText;
    [SerializeField] private TMP_Text _nextStatsText;
    [SerializeField] Transform _nextStarParent;
    Image[] _nextStars;
    
    [Header("Action")]
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TMP_Text _upgradeButtonText;

    [Header("Equip")]
    [SerializeField] private Button _equipButton; // 장착 버튼
    [SerializeField] private TMP_Text _equipButtonText;

    private CanvasGroup _canvasGroup;
    private EquipmentData _currentEquipmentData;
    private PlayerUpgradeData _currentPlayerUpgradeData;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_upgradeButton != null) _upgradeButton.onClick.AddListener(OnClickUpgrade);
        if (_equipButton != null) _equipButton.onClick.AddListener(OnClickEquip);

        _currentStars = new Image[_currentStarParent.childCount];
        _nextStars = new Image[_nextStarParent.childCount];
        // 별 이미지 초기화 ( 항상 개수는 같음 )
        for (int i = 0; i < _currentStarParent.childCount; i++)
        {
            if (_currentStarParent.GetChild(i).childCount > 0)
                _currentStars[i] = _currentStarParent.GetChild(i).GetChild(0).GetComponent<Image>();
                
            if (_nextStarParent.GetChild(i).childCount > 0)
                _nextStars[i] = _nextStarParent.GetChild(i).GetChild(0).GetComponent<Image>();
        }
        CloseUI();
    }

    public void OpenInfo(StoreItem item)
    {
        if (item == null) return;

        if (!(item.data is EquipmentData) && !(item.data is PlayerUpgradeData)) return;

        _currentEquipmentData = item.data as EquipmentData;
        _currentPlayerUpgradeData = item.data as PlayerUpgradeData;
        
        if (_nameText != null) _nameText.text = item.itemName;
        if (_iconImage != null) 
        {
            if (item.icon != null)
            {
                _iconImage.sprite = item.icon;
                _iconImage.color = Color.white;
            }
            else
            {
                _iconImage.sprite = null;
                _iconImage.color = new Color(1, 1, 1, 0); // 아이콘이 없으면 투명하게
            }
        }

        if (_currentEquipmentData != null)
        {
            if (_descText != null) _descText.text = _currentEquipmentData.description;

            int currentLevel = EquipmentStoreManager.Instance.GetEquipmentLevel(_currentEquipmentData);
            bool isMax = EquipmentStoreManager.Instance.IsMaxLevel(_currentEquipmentData);
            int upgradeCost = isMax ? 0 : EquipmentStoreManager.Instance.GetUpgradeCost(_currentEquipmentData);

            if(_currentTitleText != null) _currentTitleText.text = $"현재 (Lv.{currentLevel})";
            if(_nextTitleText != null) _nextTitleText.text = isMax ? "최대 레벨" : $"다음 (Lv.{currentLevel + 1})";

            if (_currentStatsText != null) _currentStatsText.text = BuildStatsString(_currentEquipmentData, currentLevel);
            if (_nextStatsText != null) _nextStatsText.text = isMax ? "-" : BuildStatsString(_currentEquipmentData, currentLevel + 1);

            if (_costText != null) _costText.text = isMax ? "MAX" : upgradeCost.ToString("N0");

            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = !isMax && PlayerManager.Instance.CurrentMoney >= upgradeCost;
                if (_upgradeButtonText != null) _upgradeButtonText.text = isMax ? "최대 레벨" : "레벨업";
            }

            UpdateEquipState();
        }
        else if (_currentPlayerUpgradeData != null)
        {
            if (_descText != null) _descText.text = _currentPlayerUpgradeData.description;

            int currentLevel = UpgradeManager.Instance.Upgrade.GetCurrentLevel(_currentPlayerUpgradeData.upgradeID);
            bool isMax = UpgradeManager.Instance.Upgrade.IsMaxLevel(_currentPlayerUpgradeData.upgradeID);
            
            int upgradeCost = isMax ? 0 : _currentPlayerUpgradeData.levels[currentLevel + 1].cost;

            if(_currentTitleText != null) _currentTitleText.text = $"현재 (Lv.{currentLevel})";
            if(_nextTitleText != null) _nextTitleText.text = isMax ? "최대 레벨" : $"다음 (Lv.{currentLevel + 1})";

            if (_currentStatsText != null) _currentStatsText.text = $"효과 수치: {_currentPlayerUpgradeData.levels[currentLevel].value}";
            
            if (_nextStatsText != null) 
                _nextStatsText.text = isMax ? "-" : $"효과 수치: {_currentPlayerUpgradeData.levels[currentLevel + 1].value}";

            if (_costText != null) _costText.text = isMax ? "MAX" : upgradeCost.ToString("N0");

            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = !isMax && PlayerManager.Instance.CurrentMoney >= upgradeCost;
                if (_upgradeButtonText != null) _upgradeButtonText.text = isMax ? "최대 레벨" : "레벨업";
            }

            if (_equipButton != null) _equipButton.gameObject.SetActive(false);
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private void UpdateEquipState()
    {
        if (_equipButton == null) return;

        if (_currentEquipmentData == null)
        {
            _equipButton.gameObject.SetActive(false);
            return;
        }
        
        _equipButton.gameObject.SetActive(true);

        bool isEquipped = EquipmentStoreManager.Instance.IsEquipped(_currentEquipmentData);

        if (isEquipped)
        {
            _equipButton.interactable = false;
            if (_equipButtonText != null) _equipButtonText.text = "현재 장착 중";
        }
        else
        {
            _equipButton.interactable = true;
            if (_equipButtonText != null) _equipButtonText.text = "장착하기";
        }
    }

    private string BuildStatsString(EquipmentData eqData, int level)
    {
        if (eqData.supportedProcessTypes == null || eqData.supportedProcessTypes.Count == 0) return "특수 효과 없음";
        
        ProcessTypeEntry entry = eqData.GetEntryWithLevel(eqData.supportedProcessTypes[0].processType, level);
        
        string stats = $"Lv.{level}\n";
        stats += $"가공 시간: x{entry.timeMultiplier:F2}\n";
        stats += $"체력 소모: x{entry.staminaMultiplier:F2}\n";
        stats += $"품질 보너스: +{entry.qualityBonus * 100:F0}%\n";
        stats += $"미니게임 완화: +{entry.miniGameEaseBonus * 100:F0}%";
        return stats;
    }

    public void CloseUI()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnClickUpgrade()
    {
        if (_currentEquipmentData != null)
        {
            if (EquipmentStoreManager.Instance.LevelUpEquipment(_currentEquipmentData))
            {
                int newLevel = EquipmentStoreManager.Instance.GetEquipmentLevel(_currentEquipmentData);
                
                // 실시간 슬롯 갱신
                UpgradeManager.Instance.UIController.UpdateEquipmentSlot(_currentEquipmentData);

                // 현재 열려있는 창의 데이터도 갱신
                StoreItem updatedItem = StoreItem.FromEquipmentLevel(_currentEquipmentData, newLevel);
                OpenInfo(updatedItem);
            }
        }
        else if (_currentPlayerUpgradeData != null)
        {
            if (UpgradeManager.Instance.Upgrade.PurchaseUpgrade(_currentPlayerUpgradeData.upgradeID))
            {
                int newLevel = UpgradeManager.Instance.Upgrade.GetCurrentLevel(_currentPlayerUpgradeData.upgradeID);
                bool isMax = UpgradeManager.Instance.Upgrade.IsMaxLevel(_currentPlayerUpgradeData.upgradeID);
                int cost = isMax ? 0 : _currentPlayerUpgradeData.levels[newLevel + 1].cost;
                
                UpgradeManager.Instance.UIController.RefreshUI();

                StoreItem updatedItem = StoreItem.FromUpgrade(_currentPlayerUpgradeData, cost);
                OpenInfo(updatedItem);
            }
        }
    }

    private void OnClickEquip()
    {
        if (_currentEquipmentData == null) return;

        if (EquipmentStoreManager.Instance.EquipEquipment(_currentEquipmentData))
        {
            UpdateEquipState();
            UpgradeManager.Instance.UIController.RefreshUI();
        }
    }
}
