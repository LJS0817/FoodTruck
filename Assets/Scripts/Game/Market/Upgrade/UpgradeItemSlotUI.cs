using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private GameObject _equippedTag;
    [SerializeField] private GameObject _upgradableTag;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Transform _currentLevelStarParent;
    Image[] _currentLevelStars;
    

    private StoreItem _item;
    public StoreItem Item => _item;

    // public System.Action<StoreItem> onClickAction;

    private void Awake()
    {
        if (_currentLevelStarParent != null)
        {
            _currentLevelStars = new Image[_currentLevelStarParent.childCount];
            for (int i = 0; i < _currentLevelStarParent.childCount; i++)
            {
                if (_currentLevelStarParent.GetChild(i).childCount > 0)
                {
                    _currentLevelStars[i] = _currentLevelStarParent.GetChild(i).GetChild(0).GetComponent<Image>();
                }
            }
        }
    }

    public void Setup(StoreItem item, System.Action<StoreItem> onClick = null)
    {
        _item = item;
        // onClickAction = onClick;

        // 이름, 아이콘 표시
        if (_nameText != null) _nameText.text = item.itemName;
        if (_iconImage != null && item.icon != null) _iconImage.sprite = item.icon;

        int currentLevel = 0;
        bool canUpgrade = false;

        // 장비 데이터
        if (item.data is EquipmentData eqData)
        {
            currentLevel = EquipmentStoreManager.Instance.GetEquipmentLevel(eqData);
            int upgradeCost = EquipmentStoreManager.Instance.GetUpgradeCost(eqData);

            if (_equippedTag != null)
                _equippedTag.SetActive(EquipmentStoreManager.Instance.IsEquipped(eqData));

            int maxLevel = 5;
            if (_currentLevelStars != null && _currentLevelStars.Length > 0)
                maxLevel = _currentLevelStars.Length;

            if (currentLevel < maxLevel)
            {
                canUpgrade = PlayerManager.Instance.CurrentMoney >= upgradeCost;
            }
        }
        // 플레이어 업그레이드 데이터
        else if (item.data is PlayerUpgradeData upgradeData)
        {
            if (_equippedTag != null) _equippedTag.SetActive(false);

            if (UpgradeManager.Instance != null && UpgradeManager.Instance.Upgrade != null)
            {
                currentLevel = UpgradeManager.Instance.Upgrade.GetCurrentLevel(upgradeData.upgradeID);
                bool isMax = UpgradeManager.Instance.Upgrade.IsMaxLevel(upgradeData.upgradeID);
                if (!isMax)
                {
                    int nextLevelCost = upgradeData.levels[currentLevel + 1].cost;
                    canUpgrade = PlayerManager.Instance.CurrentMoney >= nextLevelCost;
                }
            }
        }
        else
        {
            if (_equippedTag != null) _equippedTag.SetActive(false);
        }

        // 레벨 텍스트
        if (_levelText != null)
        {
            _levelText.text = $"Lv.{currentLevel}";
        }

        // 업그레이드 가능 태그
        if (_upgradableTag != null)
        {
            _upgradableTag.SetActive(canUpgrade);
        }

        // 별 UI 갱신
        if (_currentLevelStars != null)
        {
            for (int i = 0; i < _currentLevelStars.Length; i++)
            {
                if (_currentLevelStars[i] != null)
                {
                    _currentLevelStars[i].enabled = (i < currentLevel);
                }
            }
        }
    }

    /// <summary>
    /// 슬롯 클릭 시 설정된 onClickAction 또는 UpgradeUIController를 통해 UpgradeInfoUI에 아이템 정보를 전달합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item == null) return;
        // if (onClickAction != null) onClickAction(_item);
        // else UpgradeManager.Instance.UIController.ShowUpgradeInfo(_item);
        UpgradeManager.Instance.UIController.ShowUpgradeInfo(_item);
    }
}
