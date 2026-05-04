using UnityEngine;

public class PackageController : MonoBehaviour
{
    [Header("Package Settings")]
    [Tooltip("용기 포장에 사용될 프리팹이나 이펙트 등을 연결할 수 있습니다.")]
    public GameObject containerPrefab;
    [Tooltip("포장지 포장에 사용될 프리팹이나 이펙트 등을 연결할 수 있습니다.")]
    public GameObject wrapperPrefab;

    /// <summary>
    /// 음식의 포장 타입에 따라 다른 포장 방식을 실행합니다.
    /// </summary>
    public void PackageDish(FoodPackageType packageType)
    {
        switch (packageType)
        {
            case FoodPackageType.Container:
                Debug.Log("<color=cyan>[PackageController] 용기(Container)에 요리를 예쁘게 담아 포장합니다.</color>");
                // TODO: 용기 포장 시각적 효과 (예: 파티클, 애니메이션 재생, UI 팝업 등)
                break;

            case FoodPackageType.Wrapper:
                Debug.Log("<color=cyan>[PackageController] 포장지(Wrapper)로 요리를 정성껏 감싸 포장합니다.</color>");
                // TODO: 포장지 포장 시각적 효과 (예: 파티클, 애니메이션 재생, UI 팝업 등)
                break;

            default:
                Debug.LogWarning($"<color=yellow>[PackageController] 정의되지 않은 포장 타입입니다: {packageType}</color>");
                break;
        }
    }
}
