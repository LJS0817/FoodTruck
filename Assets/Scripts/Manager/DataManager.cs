using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomRecipeData
{
    public string customFoodName;       // 유저가 직접 지은 메뉴 이름
    public List<int> ingredientIDs = new List<int>(); // 레시피에 들어간 재료 순서
    public int basePrice;               // 재료 가치에 따른 기본 가격
}

[System.Serializable]
public class RecipeSaveData
{
    public string foodName;
    public bool isUnlocked;
    public bool hasPremium;
}

[System.Serializable]
public class SaveData
{
    public int currentMoney = 0;
    public int currentDay = 1;

    public List<RecipeSaveData> unlockedRecipes = new List<RecipeSaveData>();
    // 💡 유저가 새롭게 연구하여 만들어낸 커스텀 레시피 목록
    public List<CustomRecipeData> customRecipes = new List<CustomRecipeData>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public SaveData CurrentData { get; private set; }
    private const string SAVE_KEY = "TycoonSaveData";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Initialize()
    {
        LoadGameData();
        Debug.Log("[DataManager] 초기화 및 데이터 로드 완료");
    }

    public void SaveGameData()
    {
        string jsonText = JsonUtility.ToJson(CurrentData);
        PlayerPrefs.SetString(SAVE_KEY, jsonText);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[DataManager] 모바일 PlayerPrefs 게임 저장 완료</color>");
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string jsonText = PlayerPrefs.GetString(SAVE_KEY);
            CurrentData = JsonUtility.FromJson<SaveData>(jsonText);
            Debug.Log("<color=green>[DataManager] 기존 저장 데이터를 성공적으로 불러왔습니다.</color>");
        }
        else
        {
            CurrentData = new SaveData();
            SaveGameData();
            Debug.Log("<color=yellow>[DataManager] 새 게임 데이터를 생성했습니다.</color>");
        }
    }
}