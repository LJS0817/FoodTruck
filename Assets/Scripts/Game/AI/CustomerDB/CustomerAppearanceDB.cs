using UnityEngine;

// 성별 열거형
public enum Gender
{
    Male,
    Female
}

// 부위별 파츠 배열을 묶은 구조체
[System.Serializable]
public struct GenderParts
{
    public Sprite[] headParts;   // 머리(헤어스타일)
    public Sprite[] faceParts;   // 얼굴(표정/수염 등)
    public Sprite[] bodyParts;   // 몸통(상의)
    public Sprite[] legParts;    // 다리(하의/신발)
}

[CreateAssetMenu(fileName = "CustomerAppearanceDB", menuName = "Tycoon/Appearance DB")]
public class CustomerAppearanceDB : ScriptableObject
{
    [Header("Male Parts")]
    public GenderParts maleParts;

    [Header("Female Parts")]
    public GenderParts femaleParts;

    // (선택) 공용으로 쓰는 파츠가 있다면 여기에 추가할 수 있습니다.
    // public Sprite[] unisexAccessories; 
}