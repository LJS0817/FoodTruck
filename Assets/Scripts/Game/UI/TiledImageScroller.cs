using UnityEngine;
using UnityEngine.UI;

namespace FoodTruck.UI
{
    [RequireComponent(typeof(Image))]
    public class TiledImageScroller : MonoBehaviour
    {
        [Header("Scroll Settings")]
        public float scrollSpeedX = 0.5f;
        public float scrollSpeedY = 0.5f;

        private Image _image;
        private Material _materialClone;
        private Vector2 _offset;

        private void Awake()
        {
            _image = GetComponent<Image>();
            
            // Image의 기본 머티리얼을 복제하여 이 객체에만 독립적으로 적용되도록 합니다.
            // 이렇게 하지 않으면 같은 머티리얼을 쓰는 모든 UI가 같이 움직일 수 있습니다.
            _materialClone = new Material(_image.material);
            _image.material = _materialClone;
        }

        private void Update()
        {
            _offset.x += scrollSpeedX * Time.unscaledDeltaTime;
            _offset.y += scrollSpeedY * Time.unscaledDeltaTime;

            if (_offset.x > 1f || _offset.x < -1f) _offset.x %= 1f;
            if (_offset.y > 1f || _offset.y < -1f) _offset.y %= 1f;

            // 머티리얼의 텍스처 오프셋을 변경하여 스크롤 효과를 줍니다.
            _materialClone.mainTextureOffset = _offset;
        }

        private void OnDestroy()
        {
            // 동적으로 생성한 머티리얼은 파괴될 때 메모리에서 명시적으로 해제해야 합니다.
            if (_materialClone != null)
            {
                Destroy(_materialClone);
            }
        }
    }
}
