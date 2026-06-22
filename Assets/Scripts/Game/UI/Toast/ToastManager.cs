using UnityEngine;
using TMPro;
using DG.Tweening;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Transform toastContainer;
    [SerializeField] private int poolSize = 5;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float moveDistance = 50f;

    private Transform _container;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _container = toastContainer != null ? toastContainer : transform;
        InitializePool();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            ShowToast("This is a test toast message!");
        }
    }

    private void InitializePool()
    {
        if (toastPrefab == null)
        {
            Debug.LogWarning("[ToastManager] Toast Prefab is not assigned.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(toastPrefab, _container);
            
            // 프리팹에 ToastUI 컴포넌트가 없다면 자동 추가
            if (obj.GetComponent<ToastUI>() == null)
            {
                obj.AddComponent<ToastUI>();
            }
            obj.SetActive(false);
        }
    }

    public void ShowToast(string message)
    {
        if (_container.childCount == 0) return;

        // 1. 하이어라키의 맨 위에 있는(가장 오래된) 객체를 가져옵니다.
        Transform oldestToast = _container.GetChild(0);
        
        // 2. 컴포넌트를 가져와서 애니메이션과 텍스트를 실행합니다.
        ToastUI ui = oldestToast.GetComponent<ToastUI>();
        if (ui != null)
        {
            ui.Show(message, fadeDuration, displayDuration, moveDistance);
        }

        // 3. 방금 사용한 객체를 하이어라키의 맨 마지막으로 보냅니다. 
        // -> 화면 최상단에 그려지며, 다음 호출 시 가장 늦게(5번 뒤에) 재사용되도록 합니다.
        oldestToast.SetAsLastSibling();
    }
}
