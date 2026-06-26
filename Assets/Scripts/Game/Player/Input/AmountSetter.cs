using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class AmountSetter : MonoBehaviour
{
    [SerializeField] private int maxAmount = 99;
    
    [SerializeField] private Slider _quantityInput;
    [SerializeField] TMP_Text priceText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    private int currentAmount = 1;
    private int _basePrice = 0;

    public int CurrentAmount => currentAmount;

    private void Start()
    {
        if (_quantityInput != null)
        {
            _quantityInput.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    public void IncreaseAmount()
    {
        if (currentAmount < maxAmount)
        {
            currentAmount++;
            UpdateAmountText();
        }
    }

    public void DecreaseAmount()
    {
        if (currentAmount > 1)
        {
            currentAmount--;
            UpdateAmountText();
        }
    }

    private void OnSliderValueChanged(float value)
    {
        currentAmount = Mathf.RoundToInt(value);
        UpdateAmountText();
    }

    private void UpdateQuantityUI()
    {
        // 수량 범위에 따른 버튼 활성화/비활성화
        if (_minusButton != null) _minusButton.interactable = (currentAmount > 1);
        if (_plusButton != null) _plusButton.interactable = (currentAmount < maxAmount);
    }

    private void UpdateAmountText()
    {
        UpdateQuantityUI();
        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }
        if (priceText != null)
        {
            priceText.text = (_basePrice * currentAmount).ToString("N0");
        }
        if (_quantityInput != null && _quantityInput.value != currentAmount)
        {
            _quantityInput.value = currentAmount;
        }

        // Content SizeFitter 및 부모 레이아웃이 즉시 크기를 반영하도록 강제 갱신
        if (amountText != null)
        {
            // 텍스트의 크기 먼저 맞춤
            LayoutRebuilder.ForceRebuildLayoutImmediate(amountText.rectTransform);
            // 텍스트를 감싸고 있는 부모(Horizontal Layout Group 등)의 크기/위치도 즉시 맞춤
            if (amountText.transform.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(amountText.transform.parent.GetComponent<RectTransform>());
            }
        }
    }

    public void SetMaxAmount(int max)
    {
        maxAmount = max;
        if (_quantityInput != null)
        {
            _quantityInput.minValue = 1;
            _quantityInput.maxValue = maxAmount;
        }
        if (currentAmount > maxAmount)
        {
            currentAmount = maxAmount;
            UpdateAmountText();
        }
    }

    public void SetAmountInfo(int basePrice, int max, bool isStoreMode = true)
    {
        _basePrice = basePrice;
        
        SetMaxAmount(max);
        
        if (isStoreMode)
        {
            // 상점에서는 기본 수량을 1로 초기화
            currentAmount = 1;
        }
        else
        {
            // 인벤토리에서는 슬라이더 위치와 수량을 중앙(절반)으로 초기화
            currentAmount = Mathf.Max(1, Mathf.RoundToInt(max / 2f));
        }
        
        UpdateAmountText();
    }
}