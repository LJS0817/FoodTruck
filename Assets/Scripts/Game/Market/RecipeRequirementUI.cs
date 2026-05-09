using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 레시피 상세 정보에서 필요한 재료나 도구를 표시하는 개별 요소입니다.
/// </summary>
public class RecipeRequirementUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;

    public void Setup(Sprite icon, string name)
    {
        if (_iconImage != null) _iconImage.sprite = icon;
        if (_nameText != null) _nameText.text = name;
    }
}
