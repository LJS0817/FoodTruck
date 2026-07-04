using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    [SerializeField] private CanvasGroup transition; // 화면 전환 효과를 위한 CanvasGroup
    [SerializeField] private float transitionDuration = 0.5f;

    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
            
        transition.gameObject.SetActive(false);
        transition.alpha = 0f;

        SwitchUI(false);
    }

    public void ToggleView()
    {
        if (isTransitioning) return;

        if (isInsideTruck) GoOutside();
        else GoInside();
    }

    /// <summary>
    /// 외부에서 콜백(midAction)을 받아 화면 전환(페이드인/아웃) 이펙트를 재사용할 수 있게 합니다.
    /// </summary>
    public void PerformFadeTransition(System.Action midAction)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeTransitionRoutine(midAction));
    }

    private IEnumerator FadeTransitionRoutine(System.Action midAction)
    {
        isTransitioning = true;
        transition.gameObject.SetActive(true);
        transition.alpha = 0f;

        yield return transition.DOFade(1f, transitionDuration).SetUpdate(true).WaitForCompletion();

        midAction?.Invoke();
        
        yield return new WaitForSecondsRealtime(0.75f);

        yield return transition.DOFade(0f, transitionDuration).SetUpdate(true).WaitForCompletion();

        transition.gameObject.SetActive(false);
        isTransitioning = false;
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
        transition.alpha = 0f;

        // 1. 페이드 아웃 (화면 가리기 - Progress를 0에서 1로)
        yield return transition.DOFade(1f, transitionDuration).SetUpdate(true).WaitForCompletion();

        isInsideTruck = toInside;
        SwitchUI(isInsideTruck);

        // 내부에 진입할 때 무조건 카운터 화면으로 초기화
        if (isInsideTruck && TruckInsideNavigation.Instance != null)
        {
            TruckInsideNavigation.Instance.OnEnterTruck();
        }

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

        yield return new WaitForSecondsRealtime(1f);

        // 3. 페이드 인 (화면 보이기 - Progress를 1에서 0으로)
        yield return transition.DOFade(0f, transitionDuration).SetUpdate(true).WaitForCompletion();

        transition.gameObject.SetActive(false);

        // 시간 재개
        Time.timeScale = originalTimeScale;
        isTransitioning = false;
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

        if (BusinessManager.Instance != null)
        {
            BusinessManager.Instance.ChangeBusinessButton(isGoingInside);
        }
    }
}