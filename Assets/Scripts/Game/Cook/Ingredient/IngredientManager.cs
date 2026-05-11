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
        _inventoryMng.AddIngredient(_boxSetters[0].boxData, 100, _boxSetters[0].boxData.maxShelfLifeDays);
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

    public void SetupBox(IngredientData data)
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
                    CompleteSetup(data, finalQuality);
                }
                else
                {
                    Debug.Log("[IngredientManager] 가공 실패! 일반 품질로 세팅됩니다.");
                    CompleteSetup(data, 1.0f);
                }
            };

            MiniGameManager.Instance.OnMiniGameFinished += onFinished;
            MiniGameManager.Instance.StartMiniGame(data.requiredMiniGame.ToString());
        }
        else
        {
            CompleteSetup(data, 1.0f);
        }
    }

    private void CompleteSetup(IngredientData data, float quality)
    {
        for (int i = 0; i < _boxSetters.Count; i++)
        {
            if (_boxSetters[i].boxData.ingredientID == data.ingredientID)
            {
                SetupBox(i, quality);
                return;
            }
        }
    }
}