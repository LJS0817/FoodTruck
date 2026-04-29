using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class IngredientBox : MonoBehaviour, IInteractable
{
    [Header("Box Settings")]
    public IngredientData boxData;        // 이 상자에서 나올 재료 데이터
    public IngredientObject prefabToSpawn; // 꺼내질 재료 프리팹 (추후 풀링 시스템 연동 필요)
    public Transform spawnPoint;          // 재료가 등장할 위치

    public IInteractable OnTouchBegin(Vector2 touchPosition)
    {
        // 1. 인벤토리에 재고가 있는지 확인
        if (InventoryManager.Instance.UseIngredient(boxData.ingredientID))
        {
            return SpawnIngredient(touchPosition);
        }
        else
        {
            // 재고가 없을 때의 피드백 (예: 상자가 흔들리는 애니메이션이나 붉은색 깜빡임)
            Debug.Log($"<color=red>재료 부족: {boxData.ingredientName} 상자가 비었습니다!</color>");
        }
        return this;
    }

    public void OnTouchDrag(Vector2 touchPosition)
    {
        // 상자 자체는 드래그되지 않으므로 비워둡니다.
    }

    public void OnTouchEnd()
    {
        // 터치 종료 로직
    }

    private IInteractable SpawnIngredient(Vector2 touchPosition)
    {
        // 2. 재료 생성 
        // (현재는 Instantiate지만, 최적화를 위해 이 부분도 반드시 Object Pool에서 꺼내오도록 수정해야 합니다)
        IngredientObject newIngredient = Instantiate(prefabToSpawn, touchPosition, Quaternion.identity);

        // 3. 생성된 재료에 데이터 덮어씌우기
        newIngredient.SetupIngredient(boxData);
        return newIngredient.OnTouchBegin(touchPosition);

        // 등장 시 약간의 애니메이션 효과 (Scale 튕김 등) 추가 가능
    }
}