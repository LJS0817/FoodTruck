using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class AmountSetter : MonoBehaviour
{
    [SerializeField] private int maxAmount = 99;
    
    [SerializeField] CanvasGroup _group;
    [SerializeField] private Slider _quantityInput;
    [SerializeField] TMP_Text priceText;
    [SerializeField] private TMP_Text amountText;


    Action<int> onAmountConfirmed;

    private int currentAmount = 1;
    private int _basePrice = 0;

    public int CurrentAmount => currentAmount;

    private void Start()
    {
        if (_quantityInput != null)
        {
            _quantityInput.onValueChanged.AddListener(OnSliderValueChanged);
        }
        Close();
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

    private void UpdateAmountText()
    {
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

    public void Open(int max, int basePrice, Action<int> onConfirm)
    {
        onAmountConfirmed = onConfirm;
        _basePrice = basePrice;
        SetMaxAmount(max);
        
        currentAmount = 1;
        UpdateAmountText();
        
        if (_group != null)
        {
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }
    }

    public void Close()
    {
        onAmountConfirmed = null;
        if (_group != null)
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }
    }

    public void OnSubmit()
    {
        onAmountConfirmed?.Invoke(currentAmount);
        Close();
    }
}