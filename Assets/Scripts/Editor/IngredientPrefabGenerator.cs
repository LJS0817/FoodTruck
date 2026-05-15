using UnityEngine;
using UnityEditor;
using System.IO;

public class IngredientPrefabGenerator : EditorWindow
{
    private const string basePrefabPath = "Assets/Prefabs/Food/Ingredient/Ingredient.prefab";
    private const string soRootPath = "Assets/ScriptableObjects/Ingredient";
    private const string prefabRootPath = "Assets/Prefabs/Food/Ingredient";

    [MenuItem("Tycoon/Generate Ingredient Prefabs")]
    public static void GeneratePrefabs()
    {
        // 1. 기본 프리팹 로드
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
        if (basePrefab == null)
        {
            Debug.LogError($"[IngredientPrefabGenerator] 기본 프리팹을 찾을 수 없습니다: {basePrefabPath}");
            return;
        }

        // 2. ScriptableObjects/Ingredient 폴더 내의 모든 IngredientData 찾기
        string[] guids = AssetDatabase.FindAssets("t:IngredientData", new[] { soRootPath });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[IngredientPrefabGenerator] {soRootPath} 경로에서 IngredientData를 찾을 수 없습니다.");
            return;
        }

        int count = 0;

        foreach (string guid in guids)
        {
            string soPath = AssetDatabase.GUIDToAssetPath(guid);
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(soPath);

            if (data == null) continue;

            // 3. SO 경로를 분석하여 카테고리(서브 폴더) 파악
            // soRootPath 이후의 상대 경로 추출 (예: /Vegetables/Tomato.asset -> \Vegetables)
            string relativePath = soPath.Substring(soRootPath.Length + 1);
            string folderName = Path.GetDirectoryName(relativePath); // "Vegetables" 또는 "" (루트)

            // 타겟 폴더 경로 조합
            string targetFolder = prefabRootPath;
            if (!string.IsNullOrEmpty(folderName))
            {
                targetFolder = targetFolder + "/" + folderName.Replace('\\', '/');
            }

            // 폴더가 없으면 생성
            CreateFolderRecursively(targetFolder);

            // 4. 새 프리팹 경로 생성 (SO의 에셋 이름 그대로 사용)
            string newPrefabPath = $"{targetFolder}/{Path.GetFileNameWithoutExtension(soPath)}.prefab";

            // 만약 기본 프리팹을 자기 자신(Ingredient.prefab)에 덮어쓰려 하는 거라면 건너뛰기
            if (newPrefabPath == basePrefabPath) continue;

            // 5. 기본 프리팹을 인스턴스화하여 값 수정 후 저장
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            
            // 이름 변경
            instance.name = data.name;

            // IngredientObject 컴포넌트에 Data 할당
            IngredientObject ingredientObject = instance.GetComponent<IngredientObject>();
            if (ingredientObject != null)
            {
                ingredientObject.currentData = data;
                
                // 에디터 상에서 아이콘이 바로 보이도록 SpriteRenderer도 세팅해줍니다.
                SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null && data.ingredientSprite != null)
                {
                    sr.sprite = data.ingredientSprite;
                }
            }
            else
            {
                Debug.LogWarning($"[IngredientPrefabGenerator] {basePrefab.name}에 IngredientObject 컴포넌트가 없습니다.");
            }

            // 프리팹으로 저장
            PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath);
            DestroyImmediate(instance);

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>[IngredientPrefabGenerator] 총 {count}개의 프리팹이 성공적으로 생성되었습니다!</color>");
    }

    private static void CreateFolderRecursively(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] folders = folderPath.Split('/');
        string currentPath = folders[0]; // "Assets"

        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = nextPath;
        }
    }

    [MenuItem("Tycoon/Update Ingredient Manager in Scene")]
    public static void UpdateIngredientManager()
    {
        IngredientManager manager = Object.FindFirstObjectByType<IngredientManager>();
        if (manager == null)
        {
            Debug.LogError("[IngredientPrefabGenerator] 현재 씬에서 IngredientManager를 찾을 수 없습니다. (GameScene이 열려 있는지 확인하세요)");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabRootPath });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[IngredientPrefabGenerator] {prefabRootPath} 경로에서 프리팹을 찾을 수 없습니다.");
            return;
        }

        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty boxSettersProp = serializedManager.FindProperty("_boxSetters");
        
        if (boxSettersProp == null)
        {
            Debug.LogError("[IngredientPrefabGenerator] IngredientManager에서 _boxSetters 필드를 찾을 수 없습니다.");
            return;
        }

        boxSettersProp.ClearArray();
        
        int addedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == basePrefabPath) continue; // 기본 프리팹 제외

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            IngredientObject ingredientObject = prefab.GetComponent<IngredientObject>();
            if (ingredientObject != null)
            {
                boxSettersProp.InsertArrayElementAtIndex(addedCount);
                SerializedProperty elementProp = boxSettersProp.GetArrayElementAtIndex(addedCount);
                
                SerializedProperty prefabProp = elementProp.FindPropertyRelative("prefabToSpawn");
                if (prefabProp != null)
                {
                    prefabProp.objectReferenceValue = ingredientObject;
                }

                addedCount++;
            }
        }

        serializedManager.ApplyModifiedProperties();
        
        Debug.Log($"<color=green>[IngredientPrefabGenerator] IngredientManager의 _boxSetters에 총 {addedCount}개의 데이터가 성공적으로 자동 등록되었습니다!</color>");
    }
}
