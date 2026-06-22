using UnityEngine;

public class WorkerNPCController : MonoBehaviour
{
    [Header("Visual Renderers")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer faceRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer legRenderer;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public Vector2 minBounds = new Vector2(-5f, -2f);
    public Vector2 maxBounds = new Vector2(0f, 0f); // 트럭 외부 적절한 좌표 (임시)
    
    private Vector3 targetPosition;
    private bool isWaiting;
    private float waitTimer;

    public void Setup(WorkerData data, CustomerAppearanceDB appearanceDB)
    {
        // 시드 고정으로 동일한 외형 생성
        int seed = data.GetAppearanceSeed();
        Random.InitState(seed);

        // 성별 랜덤 결정 (시드 고정이므로 항상 같음)
        bool isMale = Random.value > 0.5f;
        ref GenderParts parts = ref (isMale ? ref appearanceDB.maleParts : ref appearanceDB.femaleParts);

        EquipRandomPart(headRenderer, parts.headParts);
        EquipRandomPart(faceRenderer, parts.faceParts);
        EquipRandomPart(bodyRenderer, parts.bodyParts);
        EquipRandomPart(legRenderer, parts.legParts);

        // 다시 랜덤 시드를 시간 기반으로 복구
        Random.InitState(System.Environment.TickCount);

        PickNewTarget();
    }

    private void EquipRandomPart(SpriteRenderer renderer, Sprite[] parts)
    {
        if (renderer != null && parts != null && parts.Length > 0)
        {
            int randomIndex = Random.Range(0, parts.Length);
            renderer.sprite = parts[randomIndex];
        }
        else if (renderer != null)
        {
            renderer.sprite = null;
        }
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                PickNewTarget();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isWaiting = true;
                waitTimer = Random.Range(2f, 5f); // 2~5초 대기
            }
        }
    }

    private void PickNewTarget()
    {
        float rx = Random.Range(minBounds.x, maxBounds.x);
        float ry = Random.Range(minBounds.y, maxBounds.y);
        targetPosition = new Vector3(rx, ry, 0f);
    }
}
