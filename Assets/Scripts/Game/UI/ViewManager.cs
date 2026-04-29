using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [Header("State")]
    public bool isInsideTruck = false;

    [Header("UI Canvas Settings")]
    public GameObject outsideCanvas; // 트럭 외부 요소가 담긴 캔버스 (손님 줄, 트럭 외관 등)
    public GameObject insideCanvas;  // 트럭 내부 요소가 담긴 캔버스 (조리대, 재료, 화구 등)

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 무조건 밖(외부 뷰)에서 시작
        //GoOutside();
    }

    public void ToggleView()
    {
        if (isInsideTruck) GoOutside();
        else GoInside();
    }

    public void GoInside()
    {
        isInsideTruck = true;

        // 카메라 이동 없이 캔버스만 전환
        outsideCanvas.SetActive(false);
        insideCanvas.SetActive(true);

        Debug.Log("<color=cyan>[ViewManager] 트럭 내부 진입: 수동 조리 모드 UI 활성화</color>");
    }

    public void GoOutside()
    {
        isInsideTruck = false;

        // 카메라 이동 없이 캔버스만 전환
        insideCanvas.SetActive(false);
        outsideCanvas.SetActive(true);

        Debug.Log("<color=cyan>[ViewManager] 트럭 외부 진입: 자동 조리 모드 UI 활성화</color>");
    }
}