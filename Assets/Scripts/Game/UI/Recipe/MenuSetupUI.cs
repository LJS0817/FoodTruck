using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSetupUI : MonoBehaviour
{
    public static MenuSetupUI Instance { get; private set; }

    [SerializeField] private CanvasGroup _canvasGroup;
    [Header("UI References")]
    [SerializeField] private Transform _slotParent;
    [SerializeField] private MenuSetupSlotUI _slotPrefab;
    
    [Header("Selected UI References")]
    [Tooltip("선택한 메뉴들을 띄워줄 작은 스크롤뷰의 부모 트랜스폼")]
    [SerializeField] private Transform _selectedSlotParent;
    [Tooltip("선택한 메뉴 전용 슬롯 프리팹 (버튼 또는 X 버튼으로 제거 가능)")]
    [SerializeField] private MenuSelectedSlotUI _selectedSlotPrefab;

    [Header("Control References")]
    [SerializeField] private TMP_Text _ingredientCountText;
    [SerializeField] private Button _startBusinessButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _resetButton;

    private List<MenuSetupSlotUI> _slots = new List<MenuSetupSlotUI>();
    private List<MenuSelectedSlotUI> _selectedSlots = new List<MenuSelectedSlotUI>();
    private List<FoodData> _selectedRecipes = new List<FoodData>();
    
    private int _maxBoxCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (_startBusinessButton != null)
            _startBusinessButton.onClick.AddListener(OnStartBusinessClicked);
        
        if (_closeButton != null)
            _closeButton.onClick.AddListener(() => CloseUI());

        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetClicked);
    }

    public void OpenUI()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        InitializeSlots();
        UpdateUIState();
    }

    public void CloseUI()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void InitializeSlots()
    {
        // 기존 전체 리스트 슬롯 초기화
        for (int i = 0; i < _slots.Count; i++)
        {
            Destroy(_slots[i].gameObject);
        }
        _slots.Clear();

        // 기존 선택 리스트 슬롯 초기화
        for (int i = 0; i < _selectedSlots.Count; i++)
        {
            Destroy(_selectedSlots[i].gameObject);
        }
        _selectedSlots.Clear();

        _selectedRecipes.Clear();

        // 이전 장사(어제)에 선택했던 메뉴 목록 캐싱
        List<FoodData> previousMenu = new List<FoodData>();
        if (MenuManager.Instance != null)
        {
            previousMenu.AddRange(MenuManager.Instance.GetAvailableRecipes());
        }

        if (CookingManager.Instance == null || CookingManager.Instance.recipeManager == null) return;

        // 모든 해금된 레시피 가져오기
        List<FoodData> allUnlocked = CookingManager.Instance.recipeManager.GetAllUnlockedRecipes();
        _maxBoxCount = IngredientManager.Instance.maxBoxCount;

        for (int i = 0; i < allUnlocked.Count; i++)
        {
            FoodData recipe = allUnlocked[i];
            
            // 요구 재료 계산
            HashSet<int> reqIngs = new HashSet<int>();
            if (recipe.ingredientConfigs != null)
            {
                for (int j = 0; j < recipe.ingredientConfigs.Length; j++)
                {
                    if (recipe.ingredientConfigs[j].rawIngredient != null)
                        reqIngs.Add(recipe.ingredientConfigs[j].rawIngredient.ingredientID);
                }
            }

            MenuSetupSlotUI slot = Instantiate(_slotPrefab, _slotParent);
            _slots.Add(slot);

            // 단일 레시피만으로도 최대 조리대 개수를 초과하면 아예 비활성화
            if (reqIngs.Count > _maxBoxCount)
            {
                slot.Init(recipe, false, null); // 락 상태로 초기화
                slot.SetInteractable(false);
            }
            else
            {
                bool wasSelected = previousMenu.Contains(recipe);
                slot.Init(recipe, wasSelected, OnSlotToggled);

                // 어제 팔았던 메뉴라면 자동으로 선택 상태로 연동
                if (wasSelected)
                {
                    // 수동으로 OnSlotToggled를 호출하여 선택된 리스트 뷰에도 추가하고 내부 카운트 갱신
                    OnSlotToggled(slot, true);
                }
            }
        }
    }

    private void OnSlotToggled(MenuSetupSlotUI slot, bool isOn)
    {
        if (isOn)
        {
            _selectedRecipes.Add(slot.FoodData);
            
            // 💡 선택된 레시피 전용 UI 생성
            if (_selectedSlotParent != null && _selectedSlotPrefab != null)
            {
                MenuSelectedSlotUI selectedSlot = Instantiate(_selectedSlotPrefab, _selectedSlotParent);
                selectedSlot.Init(slot.FoodData, OnSelectedSlotRemoved);
                _selectedSlots.Add(selectedSlot);
            }
        }
        else
        {
            _selectedRecipes.Remove(slot.FoodData);
            RemoveSelectedSlotUI(slot.FoodData);
        }

        // 현재 선택된 모든 레시피의 고유 재료 수 계산
        HashSet<int> currentUniqueIngredients = GetUniqueIngredientsFromSelected();

        // 제한 초과 시 선택 롤백
        if (currentUniqueIngredients.Count > _maxBoxCount)
        {
            Debug.LogWarning("[MenuSetupUI] 재료통 한도 초과! 더 이상 선택할 수 없습니다.");
            _selectedRecipes.Remove(slot.FoodData);
            slot.SetToggleWithoutNotify(false);
            RemoveSelectedSlotUI(slot.FoodData); // 롤백이므로 생성된 슬롯도 삭제
            currentUniqueIngredients = GetUniqueIngredientsFromSelected(); // 롤백 후 재계산
        }

        UpdateUIState(currentUniqueIngredients.Count);
    }

    private void OnSelectedSlotRemoved(FoodData removedFood)
    {
        // 1. 전체 리스트에서 해당 슬롯 찾아 끄기 (콜백 발생 안함)
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].FoodData == removedFood)
            {
                _slots[i].SetToggleWithoutNotify(false);
                break;
            }
        }

        // 2. 내부 데이터 및 선택 리스트 뷰에서 제거
        _selectedRecipes.Remove(removedFood);
        RemoveSelectedSlotUI(removedFood);
        
        // 3. UI 업데이트
        UpdateUIState();
    }

    private void RemoveSelectedSlotUI(FoodData food)
    {
        for (int i = _selectedSlots.Count - 1; i >= 0; i--)
        {
            if (_selectedSlots[i].FoodData == food)
            {
                Destroy(_selectedSlots[i].gameObject);
                _selectedSlots.RemoveAt(i);
                break;
            }
        }
    }

    private HashSet<int> GetUniqueIngredientsFromSelected()
    {
        HashSet<int> uniqueIngredients = new HashSet<int>();
        for (int i = 0; i < _selectedRecipes.Count; i++)
        {
            var recipe = _selectedRecipes[i];
            if (recipe.ingredientConfigs != null)
            {
                for (int j = 0; j < recipe.ingredientConfigs.Length; j++)
                {
                    if (recipe.ingredientConfigs[j].rawIngredient != null)
                        uniqueIngredients.Add(recipe.ingredientConfigs[j].rawIngredient.ingredientID);
                }
            }
        }
        return uniqueIngredients;
    }

    private void UpdateUIState(int currentUniqueCount = -1)
    {
        if (currentUniqueCount == -1)
        {
            currentUniqueCount = GetUniqueIngredientsFromSelected().Count;
        }

        if (_ingredientCountText != null)
        {
            int remainingBoxes = _maxBoxCount - currentUniqueCount;
            _ingredientCountText.text = $"조리대 공간: {currentUniqueCount} / {_maxBoxCount}\n(남은 칸: {remainingBoxes}개)";
            _ingredientCountText.color = currentUniqueCount == _maxBoxCount ? Color.yellow : Color.white;
        }

        // 1개라도 메뉴가 선택되어야 장사 시작 가능
        if (_startBusinessButton != null)
        {
            _startBusinessButton.interactable = _selectedRecipes.Count > 0;
        }
    }

    private void OnResetClicked()
    {
        _selectedRecipes.Clear();
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetToggleWithoutNotify(false);
        }

        for (int i = 0; i < _selectedSlots.Count; i++)
        {
            Destroy(_selectedSlots[i].gameObject);
        }
        _selectedSlots.Clear();

        UpdateUIState(0);
        Debug.Log("[MenuSetupUI] 선택된 모든 메뉴가 초기화되었습니다.");
    }

    private void OnStartBusinessClicked()
    {
        if (_selectedRecipes.Count == 0) return;

        // 1. MenuManager에 선택된 레시피 전달
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.SetTodayMenu(_selectedRecipes);
        }

        // 2. IngredientManager를 통해 빈 조리대에 필요 재료 자동 세팅
        if (IngredientManager.Instance != null)
        {
            List<IngredientData> uniqueDataList = new List<IngredientData>();
            HashSet<int> uniqueIds = new HashSet<int>();

            for (int i = 0; i < _selectedRecipes.Count; i++)
            {
                var recipe = _selectedRecipes[i];
                if (recipe.ingredientConfigs != null)
                {
                    for (int j = 0; j < recipe.ingredientConfigs.Length; j++)
                    {
                        var ing = recipe.ingredientConfigs[j].rawIngredient;
                        if (ing != null && !uniqueIds.Contains(ing.ingredientID))
                        {
                            uniqueIds.Add(ing.ingredientID);
                            uniqueDataList.Add(ing);
                        }
                    }
                }
            }

            IngredientManager.Instance.AutoFillBoxes(uniqueDataList);
        }

        // 3. 팝업 닫기 및 실제 장사 시작
        CloseUI();
        BusinessManager.Instance.ToggleBusiness(true); // 팝업에서 승인했으므로 실제 시작
    }
}
