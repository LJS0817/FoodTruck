using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TruckInsideNavigation : MonoBehaviour
{
    public static TruckInsideNavigation Instance { get; private set; }

    [Header("Data")]
    public TruckData truckData; // 외부 트럭 오브젝트의 레이아웃 데이터 참조

    [Header("UI Buttons")]
    public Button leftButton;
    public Button rightButton;
    public TMP_Text stationNameText; // 현재 스테이션 이름 표시용 (선택)

    [Header("Station GameObjects")]
    [Tooltip("카운터 역할을 하는 오브젝트 (Equipment == null 일 때 활성화)")]
    public GameObject counterObject;
    
    [Tooltip("장비 역할을 하는 오브젝트 (단일 Equipment 오브젝트를 재사용하여 Sprite와 Data만 교체)")]
    public Equipment equipmentObject;

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (leftButton != null) leftButton.onClick.AddListener(MoveLeft);
        if (rightButton != null) rightButton.onClick.AddListener(MoveRight);
    }

    public void OnEnterTruck()
    {
        // "항상 트럭 내부 진입 시에는 카운터 화면이 기본이다"
        currentIndex = GetCounterIndex();
        UpdateView();
    }

    private int GetCounterIndex()
    {
        if (truckData == null || truckData.stationLayout == null) return 0;
        
        // 배열에서 첫 번째로 null인 인덱스를 카운터로 간주합니다.
        for (int i = 0; i < truckData.stationLayout.Length; i++)
        {
            if (truckData.stationLayout[i] == null) return i;
        }
        return 0; // null이 없으면 무조건 0번 
    }

    public void MoveLeft()
    {
        if (truckData == null || truckData.stationLayout == null || truckData.stationLayout.Length == 0) return;
        
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = truckData.stationLayout.Length - 1;
        }
        UpdateView();
    }

    public void MoveRight()
    {
        if (truckData == null || truckData.stationLayout == null || truckData.stationLayout.Length == 0) return;
        
        currentIndex++;
        if (currentIndex >= truckData.stationLayout.Length)
        {
            currentIndex = 0;
        }
        UpdateView();
    }

    private void UpdateView()
    {
        if (truckData == null || truckData.stationLayout == null || truckData.stationLayout.Length == 0) return;

        EquipmentData currentEq = truckData.stationLayout[currentIndex];

        if (currentEq == null) 
        {
            // 카운터 화면
            if (counterObject != null) counterObject.SetActive(true);
            if (equipmentObject != null) equipmentObject.gameObject.SetActive(false);
            
            if (stationNameText != null) stationNameText.text = "Counter";
            Debug.Log($"[TruckInsideNavigation] 현재 위치: {currentIndex} (카운터)");
        }
        else 
        {
            // 장비 화면
            if (counterObject != null) counterObject.SetActive(false);
            if (equipmentObject != null) 
            {
                equipmentObject.gameObject.SetActive(true);
                
                // 장비 데이터 교체
                equipmentObject.equipmentData = currentEq;
                
                // 스프라이트 교체
                SpriteRenderer sr = equipmentObject.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = currentEq.equipmentSprite;
                
                // 백그라운드 매니저와 연동하여 현재 장비의 시각적 진행 상태(이펙트/UI) 갱신
                equipmentObject.SyncState();
            }
            
            if (stationNameText != null) stationNameText.text = currentEq.equipmentName;
            Debug.Log($"[TruckInsideNavigation] 현재 위치: {currentIndex} ({currentEq.equipmentName})");
        }
    }
}
