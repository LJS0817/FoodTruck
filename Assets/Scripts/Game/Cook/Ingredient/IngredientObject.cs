using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class IngredientObject : PoolableObject, IInteractable
{
    [Header("Ingredient Info")]
    public IngredientData currentData; // 재료의 고유 데이터 (SO)

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition; // 터치 전 원래 위치
    private bool isDragging = false;
    public bool isProcessing = false; // 가공 진행 중 (드래그 불가)
    public bool isProcessed = false;  // 가공 완료 (장비로 다시 드래그 불가, 냄비로만 가능)

    int _layerMask;

    private void Awake()
    {
        _layerMask = LayerMask.GetMask("Cook");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 오브젝트 풀에서 꺼내질 때 데이터와 스프라이트 세팅
    public void SetupIngredient(IngredientData data)
    {
        currentData = data;
        spriteRenderer.sprite = data.ingredientSprite;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        isDragging = false;
        isProcessing = false;
        isProcessed = false;
    }

    // --- IInteractable 인터페이스 구현 --- //

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        if (isProcessing) return null; // 가공 중에는 터치(드래그) 차단

        isDragging = true;
        originalPosition = transform.position;

        // 터치 시 시각적 피드백 (살짝 커짐)
        transform.localScale = Vector3.one * 1.2f;

        // UI나 다른 재료보다 위로 올라오도록 정렬 순서 조정
        spriteRenderer.sortingOrder = 100;

        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition)
    {
        if (!isDragging) return;

        // 손가락(마우스)을 따라 이동
        transform.position = new Vector3(touchPosition.x, touchPosition.y, 0f);
    }

    public void OnTouchEnd()
    {
        isDragging = false;
        transform.localScale = Vector3.one; // 크기 원상복구
        spriteRenderer.sortingOrder = 0;

        CheckDropLocation();
    }

    // 드래그를 놓았을 때 냄비(또는 장비) 위에 있는지 판별
    private void CheckDropLocation()
    {
        Collider2D overlap = Physics2D.OverlapPoint(transform.position, _layerMask);

        if (overlap == null)
        {
            // 허공에 놓으면 제자리로 (부드러운 복귀는 이후 iTween이나 DOTween 권장)
            transform.position = originalPosition;
            return;
        }

        // 1. 냄비(CookingPot) 위에 놓인 경우
        if (overlap.TryGetComponent<CookingPot>(out CookingPot pot))
        {
            pot.ReceiveIngredient(this.currentData);
            OnDespawn();
            return;
        }

        // 2. 장비(Equipment) 위에 놓인 경우 → 가공 시작
        if (!isProcessed && overlap.TryGetComponent<Equipment>(out Equipment equipment))
        {
            if (equipment.ReceiveIngredient(this))
            {
                // 성공적으로 장비에 올라가면 풀로 반환하지 않고, 장비가 제어합니다.
            }
            else
            {
                // 가공 불가 시 제자리로 복귀
                transform.position = originalPosition;
            }
            return;
        }

        // 어떤 대상에도 해당하지 않거나, 이미 가공된 재료를 장비에 또 올린 경우 제자리로 복귀
        transform.position = originalPosition;
    }
}