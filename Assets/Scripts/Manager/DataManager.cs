using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecipeSaveData
{
    public string foodName;     // FoodData의 이름 (고유 ID 역할)
    public bool isUnlocked;     // 한 번이라도 만들었는지?
    public bool hasPremium;     // 프리미엄으로 만들어 본 적 있는지? (왕관 마크 표시용)
}

[System.Serializable]
public class SaveData
{
    public int currentMoney = 0;
    public int currentDay = 1;

    // 💡 유저의 레시피 도감 진행도 기록
    public List<RecipeSaveData> unlockedRecipes = new List<RecipeSaveData>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public SaveData CurrentData { get; private set; }
    private const string SAVE_KEY = "TycoonSaveData";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        // 💡 GameManager가 통제하도록 여기서 LoadGameData()를 부르지 않고 대기합니다.
    }

    // 💡 GameManager에서 호출할 수 있도록 명시적 초기화 함수를 부활시켰습니다.
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