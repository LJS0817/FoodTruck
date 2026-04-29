using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class IngredientObject : PoolableObject, IInteractable
{
    [Header("Ingredient Info")]
    public IngredientData currentData; // 재료의 고유 데이터 (SO)

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition; // 터치 전 원래 위치
    private bool isDragging = false;

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
    }

    // --- IInteractable 인터페이스 구현 --- //

    public void OnTouchBegin()
    {
        isDragging = true;
        originalPosition = transform.position;

        // 터치 시 시각적 피드백 (살짝 커짐)
        transform.localScale = Vector3.one * 1.2f;

        // UI나 다른 재료보다 위로 올라오도록 정렬 순서 조정
        spriteRenderer.sortingOrder = 100;
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

    // 드래그를 놓았을 때 냄비(또는 도마) 위에 있는지 판별
    private void CheckDropLocation()
    {
        Collider2D overlap = Physics2D.OverlapPoint(transform.position, _layerMask);
        Debug.Log(overlap);

        if (overlap != null && overlap.TryGetComponent<CookingPot>(out CookingPot pot))
        {
            // 냄비에 재료 데이터 전달
            pot.ReceiveIngredient(this.currentData);

            // 사용된 재료 오브젝트는 풀로 반환
            OnDespawn();
        }
        else
        {
            // 허공에 놓으면 제자리로 (부드러운 복귀는 이후 iTween이나 DOTween 권장)
            transform.position = originalPosition;
        }
    }
}