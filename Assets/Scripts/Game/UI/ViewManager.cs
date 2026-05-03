using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [Header("State")]
    public bool isInsideTruck = false;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform insideCameraTransform; // InSide일 때 참조할 외부 Transform

    [SerializeField] private CanvasGroup outsideUIPanel;
    [SerializeField] private CanvasGroup insideUIPanel;

    [Header("Transition Settings")]
    [SerializeField] private Image transition; // 화면 전환 효과를 위한 매테리얼
    Color _transitionDefaultColor;
    [SerializeField] private float transitionDuration = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
        _transitionDefaultColor = transition.color;
        SwitchUI(false);
    }

    public void ToggleView()
    {
        if (isTransitioning) return;

        if (isInsideTruck) GoOutside();
        else GoInside();
    }

    public void GoInside()
    {
        if (isTransitioning || isInsideTruck) return;
        StartCoroutine(TransitionCameraRoutine(true));
    }

    public void GoOutside()
    {
        if (isTransitioning || !isInsideTruck) return;
        StartCoroutine(TransitionCameraRoutine(false));
    }

    private IEnumerator TransitionCameraRoutine(bool toInside)
    {
        isTransitioning = true;
        
        // 시간 정지 (Customer, Order, Date 시스템 정지)
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        transition.gameObject.SetActive(true);

        // 1. 페이드 아웃 (화면 가리기 - Progress를 0에서 1로)
        yield return StartCoroutine(PlayTransitionEffect(0f, 1f));

        isInsideTruck = toInside;
        SwitchUI(isInsideTruck);

        // 2. 화면이 가려진 상태에서 카메라 위치 이동
        if (isInsideTruck)
        {
            if (insideCameraTransform != null)
            {
                // InSide: 외부 Transform의 x, y 위치를 참조하되 카메라이므로 z축은 유지
                Vector3 targetPos = insideCameraTransform.position;
                targetPos.z = mainCamera.transform.position.z;
                mainCamera.transform.position = targetPos;
            }
            Debug.Log("<color=cyan>[ViewManager] 트럭 내부 진입: 카메라 이동 완료</color>");
        }
        else
        {
            // OutSide: 카메라의 x, y를 (0, 0)으로, z축은 유지
            Vector3 targetPos = Vector3.zero;
            targetPos.z = mainCamera.transform.position.z;
            mainCamera.transform.position = targetPos;
            Debug.Log("<color=cyan>[ViewManager] 트럭 외부 진입: 카메라 이동 완료</color>");
        }

        // 3. 페이드 인 (화면 보이기 - Progress를 1에서 0으로)
        yield return StartCoroutine(PlayTransitionEffect(1f, 0f));

        // 시간 재개
        Time.timeScale = originalTimeScale;
        isTransitioning = false;
        transition.gameObject.SetActive(false);
    }

    private IEnumerator PlayTransitionEffect(float startValue, float endValue)
    {
        float elapsedTime = 0f;
        
        // 구조체를 한 번만 복사하여 로컬 변수로 캐싱
        Color targetColor = _transitionDefaultColor; 

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            
            targetColor.a = Mathf.Lerp(startValue, endValue, elapsedTime / transitionDuration);
            transition.color = targetColor;
            
            yield return null;
        }

        targetColor.a = endValue;
        transition.color = targetColor;
    }

    private void SetCanvasGroupState(CanvasGroup cg, bool isActive)
    {
        if (cg == null) return;

        cg.alpha = isActive ? 1f : 0f;
        cg.interactable = isActive;
        cg.blocksRaycasts = isActive;
    }

    public void SwitchUI(bool isGoingInside)
    {
        if (isGoingInside)
        {
            SetCanvasGroupState(outsideUIPanel, false);
            SetCanvasGroupState(insideUIPanel, true);
        }
        else
        {
            SetCanvasGroupState(insideUIPanel, false);
            SetCanvasGroupState(outsideUIPanel, true);
        }
    }
}