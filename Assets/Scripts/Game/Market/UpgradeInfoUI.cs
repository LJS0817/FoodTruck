using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeInfoUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    
    [Header("Stats")]
    [SerializeField] private TMP_Text _currentStatsText;
    [SerializeField] private TMP_Text _nextStatsText;
    
    [Header("Action")]
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TMP_Text _upgradeButtonText;

    [Header("Equip")]
    [SerializeField] private Button _equipButton; // 장착 버튼
    [SerializeField] private TMP_Text _equipButtonText;

    private CanvasGroup _canvasGroup;
    private EquipmentData _currentEquipmentData;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_upgradeButton != null) _upgradeButton.onClick.AddListener(OnClickUpgrade);
        if (_equipButton != null) _equipButton.onClick.AddListener(OnClickEquip);

        CloseUI();
    }

    public void OpenInfo(StoreItem item)
    {
        if (item == null || !(item.data is EquipmentData eqData)) return;

        _currentEquipmentData = eqData;
        
        if (_nameText != null) _nameText.text = item.itemName; // "Name Lv.X"
        if (_iconImage != null) _iconImage.sprite = item.icon;
        if (_descText != null) _descText.text = eqData.description;

        int currentLevel = EquipmentStoreManager.Instance.GetEquipmentLevel(eqData);
        int upgradeCost = EquipmentStoreManager.Instance.GetUpgradeCost(eqData);

        if (_currentStatsText != null) _currentStatsText.text = BuildStatsString(eqData, currentLevel);
        if (_nextStatsText != null) _nextStatsText.text = BuildStatsString(eqData, currentLevel + 1);

        if (_costText != null) _costText.text = upgradeCost.ToString("N0");

        if (_upgradeButton != null)
        {
            _upgradeButton.interactable = PlayerManager.Instance.CurrentMoney >= upgradeCost;
            if (_upgradeButtonText != null) _upgradeButtonText.text = "레벨업";
        }

        UpdateEquipState();

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private void UpdateEquipState()
    {
        if (_equipButton == null) return;

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
        if (_currentEquipmentData == null) return;

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

    private void OnClickEquip()
    {
        if (_currentEquipmentData == null) return;

        if (EquipmentStoreManager.Instance.EquipEquipment(_currentEquipmentData))
        {
            UpdateEquipState();
        }
    }
}
