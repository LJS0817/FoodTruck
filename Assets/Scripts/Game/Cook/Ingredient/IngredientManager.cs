using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    public static IngredientManager Instance { get; private set; }

    [SerializeField] InventoryManager _inventoryMng;
    [SerializeField] List<IngredientBoxSetter> _boxSetters;
    [SerializeField] Transform _boxParent;
    
    [Header("Dynamic Box Settings")]
    [Tooltip("UI 버전 IngredientBox 프리팹을 연결하세요.")]
    [SerializeField] IngredientBox _boxPrefab;
    [Tooltip("초기 상자 개수. 추후 업그레이드 시스템과 연동 시 이 값을 변경하거나 UpdateBoxCount를 호출하세요.")]
    public int maxBoxCount = 4;

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

        // 에디터 등에 남아있을 수 있는 더미 데이터 삭제
        for (int i = _boxParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_boxParent.GetChild(i).gameObject);
        }

        UpdateBoxCount(maxBoxCount);
        
        if (_boxSetters != null && _boxSetters.Count > 0 && _boxSetters[0].boxData != null)
        {
            _inventoryMng.AddIngredient(_boxSetters[0].boxData, 100, _boxSetters[0].boxData.maxShelfLifeDays);
        }
    }

    /// <summary>
    /// 상자 개수를 동적으로 변경합니다. (추후 업그레이드 시 호출)
    /// </summary>
    public void UpdateBoxCount(int newCount)
    {
        maxBoxCount = newCount;
        
        ScrollRect parentScrollRect = _boxParent.GetComponentInParent<ScrollRect>();

        while (_boxes.Count < maxBoxCount)
        {
            int index = _boxes.Count;
            IngredientBox newBox = Instantiate(_boxPrefab, _boxParent);
            _boxes.Add(newBox);
            
            newBox.Init(
                onRefill: () => {
                    _currentBoxIndex = index; 
                    OpenInventoryForRefill();  
                },
                onSetup: () => { 
                    _currentBoxIndex = index; 
                    OpenInventoryForSetup(); 
                },
                scrollRect: parentScrollRect
            );
        }
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

    public void SetupBox(int idx, float quality = 1.0f)
    {
        _boxes[_currentBoxIndex].SetupIngredient(_boxSetters[idx], quality);
    }

    public void EmptyCurrentBox()
    {
        if (_currentBoxIndex >= 0 && _currentBoxIndex < _boxes.Count)
        {
            _boxes[_currentBoxIndex].ResetBox();
            
            // 재료가 빠졌으므로 레시피 갱신
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.UpdateAvailableRecipes();
            }
        }
    }

    public void SetupBox(IngredientData data, int amount = -1)
    {
        // 💡 미니게임 체크 로직 추가
        if (data.requiredMiniGame != MiniGameType.None && MiniGameManager.Instance != null)
        {
            Debug.Log($"[IngredientManager] {data.ingredientName} 가공을 위해 {data.requiredMiniGame} 미니게임을 시작합니다.");
            
            // 일회성 이벤트 등록
            Action<MiniGameResult> onFinished = null;
            onFinished = (result) => {
                MiniGameManager.Instance.OnMiniGameFinished -= onFinished;
                
                // 가공 성공 시에만 세팅 (또는 점수에 따른 차등 처리)
                if (result.isSuccess)
                {
                    // 점수 계산 (예: 1.0~1.2 프리미엄 보너스)
                    float finalQuality = 1.0f + (result.qualityScore * 0.2f);
                    CompleteSetup(data, finalQuality, amount);
                }
                else
                {
                    Debug.Log("[IngredientManager] 가공 실패! 일반 품질로 세팅됩니다.");
                    CompleteSetup(data, 1.0f, amount);
                }
            };

            MiniGameManager.Instance.OnMiniGameFinished += onFinished;
            MiniGameManager.Instance.StartMiniGame(data.requiredMiniGame, 0f);
        }
        else
        {
            CompleteSetup(data, 1.0f, amount);
        }
    }

    private void CompleteSetup(IngredientData data, float quality, int amount)
    {
        for (int i = 0; i < _boxSetters.Count; i++)
        {
            if (_boxSetters[i].boxData.ingredientID == data.ingredientID)
            {
                _boxes[_currentBoxIndex].SetupIngredient(_boxSetters[i], quality, amount);
                return;
            }
        }
    }
}