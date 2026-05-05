using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientManager : MonoBehaviour
{
    public static IngredientManager Instance { get; private set; }

    [SerializeField] InventoryManager _inventoryMng;
    [SerializeField] List<IngredientBoxSetter> _boxSetters;
    [SerializeField] Transform _boxParent;
    List<IngredientBox> _boxes;

    int _currentBoxIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _currentBoxIndex = -1;
        _boxes = new List<IngredientBox>();
        for(int i = 0; i < _boxParent.childCount; i++)
        {
            int index = i; // 람다 캡처용 변수
            _boxes.Add(_boxParent.GetChild(index).GetComponent<IngredientBox>());
            
            _boxes[index].Init(
                onRefill: () => {
                    _currentBoxIndex = index; 
                    OpenInventoryForRefill();  
                },
                onSetup: () => { 
                    _currentBoxIndex = index; 
                    OpenInventoryForSetup(); 
                }
            );
        }
        _inventoryMng.AddIngredient(_boxSetters[0].boxData, 100, DateTime.Now.AddDays(_boxSetters[0].boxData.maxShelfLifeDays));
    }

    private void OpenInventoryForSetup()
    {
        Debug.Log($"[IngredientBoxManager] {_currentBoxIndex}번 상자 세팅을 위해 인벤토리를 엽니다.");
        _inventoryMng.OpenUIWithApplyBtn();
    }

    private void OpenInventoryForRefill()
    {
        IngredientData targetData = _boxes[_currentBoxIndex].GetCurrentData();
        Debug.Log($"[IngredientBoxManager] {_currentBoxIndex}번 상자 세팅/리필을 위해 인벤토리를 엽니다. (대상: {targetData?.ingredientName})");
        _inventoryMng.OpenUIWithApplyBtn(targetData);
    }

    public void SetupBox(int idx)
    {
        _boxes[_currentBoxIndex].SetupIngredient(_boxSetters[idx]);
    }

    public void SetupBox(IngredientData data)
    {
        // _boxSetters에서 해당 data를 가진 세터를 찾아 SetupBox 호출
        for (int i = 0; i < _boxSetters.Count; i++)
        {
            if (_boxSetters[i].boxData.ingredientID == data.ingredientID)
            {
                SetupBox(i);
                return;
            }
        }
    }
}