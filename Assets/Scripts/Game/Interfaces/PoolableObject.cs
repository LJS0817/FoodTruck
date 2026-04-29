using UnityEngine;

// 오브젝트 풀에서 관리될 모든 객체의 부모 클래스입니다.
public abstract class PoolableObject : MonoBehaviour
{
    [HideInInspector]
    public string poolKey; // 자신이 속한 풀의 식별자

    // 풀에서 막 꺼내어졌을 때 초기화하는 가상 메서드 (자식 클래스에서 재정의)
    public virtual void OnSpawn()
    {
        gameObject.SetActive(true);
    }

    // 사용이 끝나고 풀로 돌아갈 때 호출하는 가상 메서드
    public virtual void OnDespawn()
    {
        // 런타임 성능을 위해 Destroy 대신 비활성화합니다.
        gameObject.SetActive(false);
    }
}